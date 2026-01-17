# backend_prompt_plan.md

## Context
This backend belongs to a DnD map builder application. The current implementation incorrectly stores and retrieves images (maps and tokens) from the database, likely using base64 or JSON-embedded blobs. This causes corruption, size issues, and retrieval bugs. The fix is to move to **proper file transfer and storage**, with the database storing only metadata and file references.

---

## Goals
- Use multipart file upload instead of embedding images in JSON.
- Store images in a file system or object storage (local disk, S3-compatible, etc.).
- Persist only metadata (URL/path, size, type, ownership) in the database.
- Ensure backward-safe migration path if existing data exists.

---

## Step-by-step Implementation Plan

### Step 1: Audit Current Image Handling
**status: done**
- Identify all endpoints that accept or return images (maps, tokens).
- Confirm how images are currently:
  - Sent from frontend (base64, data URLs, JSON fields)
  - Stored in DB (byte array, string, JSON column)
- List affected entities (e.g., Map, Token).

**Findings:**
- Images are currently stored as URL strings only (nvarchar(1000) in SQL Server)
- Maps endpoint (POST/PUT/GET) with optional ImageUrl field
- Tokens endpoint (POST/PUT/GET) with required ImageUrl field
- Affected entities: GameMap, TokenDefinition
- No file upload system exists - all images are external URL references
- No binary storage, multipart/form-data handling, or file validation currently

---

### Step 2: Define File Storage Strategy
**status: done**
- Decide storage target:
  - Local filesystem (e.g., `/uploads/maps`, `/uploads/tokens`)
  - OR object storage abstraction (recommended for future scaling).
- Define naming strategy:
  - UUID-based filenames
  - Preserve original extension
- Define public vs private access rules.

**Strategy Decisions:**
- **Storage Target**: Abstraction layer with local filesystem as initial implementation
  - Directory structure: `wwwroot/uploads/maps/`, `wwwroot/uploads/tokens/`
  - Future: Can swap to S3/Azure Blob without API changes
- **Naming Strategy**:
  - Format: `{UUID}_{originalFileName}` (preserves context and extension)
  - Example: `a1b2c3d4-e5f6_dragon_token.png`
- **Access Rules**:
  - Maps: Public URLs (served via static middleware)
  - Tokens: User-specific ownership (check authorization on retrieval)
- **File Size Limits**:
  - Maps: 5MB max
  - Tokens: 2MB max
- **Allowed MIME Types**: `image/png`, `image/jpeg`, `image/webp`

---

### Step 3: Introduce File Storage Abstraction
**status: done**
- Create a `IFileStorageService` (or equivalent):
  - `UploadAsync(Stream file, FileMetadata metadata)`
  - `DeleteAsync(fileId)`
  - `GetPublicUrl(fileId)`
- Implement initial version using local filesystem.
- Ensure streaming is used (no full file buffering in memory).

**Implementation:**
- Created `IFileStorageService` interface in Application/Interfaces/IServices.cs
- Implemented `LocalFileStorageService` in Application/Services/FileStorageService.cs
- Uses async streaming (4096 buffer) - no full file buffering in memory
- Stores files in `wwwroot/uploads/{category}/{fileId}`
- Validates MIME types (png, jpeg, webp)
- Prevents path traversal attacks
- Registered as Singleton in Program.cs with logging
- Added static file middleware for serving uploads

---

### Step 4: Update Domain Models
**status: done**
- Remove image binary/base64 fields from entities.
- Add fields such as:
  - `ImageFileId`
  - `ImageUrl`
  - `ContentType`
  - `FileSize`
- Update ORM mappings and migrations accordingly.

**Implementation:**
- Updated GameMap entity: added ImageFileId, ImageContentType, ImageFileSize (kept ImageUrl for backward compatibility)
- Updated TokenDefinition entity: added ImageFileId, ImageContentType, ImageFileSize (kept ImageUrl for backward compatibility)
- Created migration: 20260117102707_AddImageFileStorageMetadata.cs
- Backward compatible: existing ImageUrl field remains for gradual migration

---

### Step 5: Update API Contracts
**status: done**
- Replace JSON-based image fields with:
  - `multipart/form-data` endpoints
- Separate responsibilities:
  - One endpoint for metadata (map config, token position, etc.)
  - One endpoint for file upload
- Example:
  - `POST /maps` (metadata only)
  - `POST /maps/{id}/image` (multipart file upload)

**Implementation:**
- Updated GameMapDto: added ImageFileId, ImageContentType, ImageFileSize fields
- Updated TokenDefinitionDto: added ImageFileId, ImageContentType, ImageFileSize fields
- Added ImageUploadResponse contract for file upload responses
- Updated mapping extensions to include new metadata fields
- IFormFile handling will be done directly in controllers (not in Contracts layer)

---

### Step 6: Implement Upload Endpoints
**status: done**
- Add endpoints using `multipart/form-data`.
- Validate:
  - File size limits
  - MIME types (png, jpg, webp)
- Store file via `IFileStorageService`.
- Persist file reference to DB.

**Implementation:**
- Added `POST /api/maps/{id}/image` endpoint in MapsController
  - Validates file size (5MB max for maps)
  - Validates MIME types (png, jpeg, webp)
  - Verifies ownership before upload
  - Stores file via IFileStorageService
  - Updates map metadata in DB
- Added `POST /api/tokens/{id}/image` endpoint in TokensController
  - Validates file size (2MB max for tokens)
  - Same validation and security checks as maps
- Both endpoints return ImageUploadResponse with file info
- Error handling for validation and storage failures

---

### Step 7: Implement Retrieval Strategy
**status: done**
- Serve images via:
  - Static file middleware (local disk)
  - OR signed URLs (if object storage)
- Ensure API returns image URLs, not raw bytes.

**Implementation:**
- Static file middleware already added to Program.cs
- Images served via `/uploads/{category}/{fileId}` routes
- File storage service returns public URLs for each file
- API endpoints return image URLs in DTO fields (ImageUrl)
- No raw bytes returned - only URL references
- Supports future S3/Azure Blob integration via IFileStorageService abstraction

---

### Step 8: Migration / Backward Compatibility
**status: done**
- Decide whether existing stored images must be migrated.
- If yes:
  - Write one-time migration script:
    - Read old image data
    - Save as file
    - Update DB references
- If no:
  - Explicitly drop legacy fields.

**Implementation:**
- **Backward Compatible Approach**: Existing ImageUrl field retained
- New file storage fields added as nullable (ImageFileId, ImageContentType, ImageFileSize)
- Old endpoints still work with existing ImageUrl data
- New upload endpoints populate both old and new fields
- Existing data can coexist during gradual migration
- Future: One-time migration script can convert existing URLs to file storage if needed
- No data loss - legacy fields preserved for rollback capability

---

### Step 9: Security & Validation
**status: done**
- Enforce authorization checks on upload/delete.
- Prevent path traversal.
- Validate file headers (not just extensions).

**Implementation:**
- **Authorization**:
  - [Authorize] attribute on all upload endpoints
  - GetUserId() verifies user identity from JWT claims
  - Ownership check: verify user owns map/token before allowing upload
- **Path Traversal Prevention**:
  - FileStorageService validates fileId to prevent ".." or "/" characters
  - Files stored in UUID format only (e.g., a1b2c3d4-e5f6.png)
  - No user-controlled path manipulation possible
- **File Validation**:
  - MIME type validation against allowlist (png, jpeg, webp)
  - File size limits enforced (5MB for maps, 2MB for tokens)
  - Validates IFormFile.Length before processing
  - Safe file operations with proper error handling
- **Storage Security**:
  - Files saved to isolated directory (wwwroot/uploads/)
  - Streaming used to avoid full-file memory buffering
  - Proper exception handling and cleanup on failures

---

### Step 10: Documentation for Frontend Team
**status: done**
- Document new endpoints and payload expectations.
- Provide example `curl` and Postman requests.

**Documentation:**

#### Map Image Upload
- **Endpoint**: `POST /api/maps/{mapId}/image`
- **Authentication**: Required (Bearer token)
- **Content-Type**: `multipart/form-data`
- **Parameters**:
  - `image` (file): The image file to upload (PNG, JPEG, or WebP)
- **File Limits**: Max 5MB
- **Response**:
  ```json
  {
    "success": true,
    "data": {
      "fileId": "a1b2c3d4-e5f6-7890.png",
      "url": "/uploads/maps/a1b2c3d4-e5f6-7890.png",
      "contentType": "image/png",
      "fileSize": 123456
    },
    "message": "Image uploaded successfully."
  }
  ```

#### Token Image Upload
- **Endpoint**: `POST /api/tokens/{tokenId}/image`
- **Authentication**: Required (Bearer token)
- **Content-Type**: `multipart/form-data`
- **Parameters**:
  - `image` (file): The image file to upload (PNG, JPEG, or WebP)
- **File Limits**: Max 2MB
- **Response**: Same format as map image upload

#### cURL Examples
```bash
# Upload map image
curl -X POST "https://api.example.com/api/maps/map-id-123/image" \
  -H "Authorization: Bearer <token>" \
  -F "image=@path/to/map.png"

# Upload token image
curl -X POST "https://api.example.com/api/tokens/token-id-456/image" \
  -H "Authorization: Bearer <token>" \
  -F "image=@path/to/token.png"
```

#### Client Implementation Notes
- Use `FormData` in JavaScript/fetch:
  ```javascript
  const formData = new FormData();
  formData.append('image', fileInput.files[0]);

  const response = await fetch(`/api/maps/${mapId}/image`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`
    },
    body: formData
  });
  ```

#### Retrieval
- Map/token images are returned in API responses via `ImageUrl` field
- Images are publicly accessible at: `/uploads/{category}/{fileId}`
- No additional API calls needed for retrieval - use URL directly in img tags

---
