# DnD Map Builder API Documentation

## Overview

This API provides endpoints for managing D&D campaigns, missions, maps, and tokens with JWT-based authentication.

## Base URL

- Development: `https://localhost:5001`
- Production: Update according to your deployment

## Authentication

All endpoints except `/api/auth/register` and `/api/auth/login` require authentication.

### How to Authenticate

1. Register a new user or login
2. Copy the `token` from the response
3. Include it in the `Authorization` header: `Bearer <token>`

Example:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## API Endpoints

### Authentication

#### Register

```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "SecurePassword123!"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "userId": "user-id",
    "username": "john_doe",
    "email": "john@example.com",
    "role": "user",
    "status": "pending"
  },
  "message": "Registration successful. Awaiting admin approval."
}
```

**Note:** New users require admin approval before they can access protected resources.

#### Login

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "SecurePassword123!"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "userId": "user-id",
    "username": "john_doe",
    "email": "john@example.com",
    "role": "user",
    "status": "approved"
  },
  "message": "Login successful."
}
```

#### Get Pending Users (Admin Only)

```http
GET /api/auth/pending-users
Authorization: Bearer <admin-token>
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "id": "user-id",
      "username": "new_user",
      "email": "newuser@example.com",
      "role": "user",
      "status": "pending"
    }
  ]
}
```

#### Approve User (Admin Only)

```http
POST /api/auth/approve-user
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "userId": "user-id",
  "approved": true
}
```

### Campaigns

#### Get All User Campaigns

```http
GET /api/campaigns
Authorization: Bearer <token>
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "id": "campaign-id",
      "name": "Lost Mines of Phandelver",
      "description": "A classic D&D adventure",
      "missions": [...],
      "ownerId": "user-id",
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": "2024-01-15T10:30:00Z"
    }
  ]
}
```

#### Get Campaign by ID

```http
GET /api/campaigns/{id}
Authorization: Bearer <token>
```

#### Create Campaign

```http
POST /api/campaigns
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "Curse of Strahd",
  "description": "A gothic horror adventure in Barovia"
}
```

#### Update Campaign

```http
PUT /api/campaigns/{id}
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "Curse of Strahd - Updated",
  "description": "Updated description"
}
```

#### Delete Campaign

```http
DELETE /api/campaigns/{id}
Authorization: Bearer <token>
```

### Missions

#### Get Mission by ID

```http
GET /api/missions/{id}
Authorization: Bearer <token>
```

#### Get Missions by Campaign

```http
GET /api/missions/campaign/{campaignId}
Authorization: Bearer <token>
```

#### Create Mission

```http
POST /api/missions
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "Village of Barovia",
  "description": "Explore the mysterious village",
  "campaignId": "campaign-id"
}
```

#### Update Mission

```http
PUT /api/missions/{id}
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "Village of Barovia - Updated",
  "description": "Updated description"
}
```

#### Delete Mission

```http
DELETE /api/missions/{id}
Authorization: Bearer <token>
```

### Maps

#### Get Map by ID

```http
GET /api/maps/{id}
Authorization: Bearer <token>
```

**Response includes all placed tokens on the map.**

#### Get Maps by Mission

```http
GET /api/maps/mission/{missionId}
Authorization: Bearer <token>
```

#### Create Map

```http
POST /api/maps
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "Tavern Interior",
  "imageUrl": "https://example.com/tavern.jpg",
  "rows": 20,
  "cols": 30,
  "gridColor": "#000000",
  "gridOpacity": 0.3,
  "missionId": "mission-id"
}
```

#### Update Map (Including Tokens)

```http
PUT /api/maps/{id}
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "Tavern Interior - Night",
  "imageUrl": "https://example.com/tavern-night.jpg",
  "rows": 20,
  "cols": 30,
  "tokens": [
    {
      "tokenId": "token-definition-id",
      "x": 5,
      "y": 10
    },
    {
      "tokenId": "another-token-id",
      "x": 15,
      "y": 8
    }
  ],
  "gridColor": "#000000",
  "gridOpacity": 0.5
}
```

**Note:** The tokens array replaces all existing tokens on the map.

#### Delete Map

```http
DELETE /api/maps/{id}
Authorization: Bearer <token>
```

### Token Definitions

#### Get All User Tokens

```http
GET /api/tokens
Authorization: Bearer <token>
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "id": "token-id",
      "name": "Warrior",
      "imageUrl": "https://example.com/warrior.png",
      "size": 1,
      "type": "player",
      "userId": "user-id"
    }
  ]
}
```

#### Get Token by ID

```http
GET /api/tokens/{id}
Authorization: Bearer <token>
```

#### Create Token

```http
POST /api/tokens
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "Dragon",
  "imageUrl": "https://example.com/dragon.png",
  "size": 3,
  "type": "enemy"
}
```

**Token Sizes:**
- `1`: 1x1 grid square (Medium creature)
- `2`: 2x2 grid squares (Large creature)
- `3`: 3x3 grid squares (Huge creature)

**Token Types:**
- `player`: Player character or ally
- `enemy`: Enemy or monster

#### Update Token

```http
PUT /api/tokens/{id}
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "Ancient Dragon",
  "imageUrl": "https://example.com/ancient-dragon.png",
  "size": 3,
  "type": "enemy"
}
```

#### Delete Token

```http
DELETE /api/tokens/{id}
Authorization: Bearer <token>
```

**Note:** Deleting a token definition does not remove it from existing maps.

## Error Responses

### 400 Bad Request
```json
{
  "success": false,
  "data": null,
  "message": "Validation error or bad request",
  "errors": ["Error detail 1", "Error detail 2"]
}
```

### 401 Unauthorized
```json
{
  "success": false,
  "data": null,
  "message": "Invalid credentials or account not approved."
}
```

### 403 Forbidden
```json
{
  "success": false,
  "data": null,
  "message": "You don't have permission to perform this action."
}
```

### 404 Not Found
```json
{
  "success": false,
  "data": null,
  "message": "Resource not found."
}
```

### 500 Internal Server Error
```json
{
  "success": false,
  "data": null,
  "message": "An internal error occurred."
}
```

## Data Models

### User
```typescript
{
  id: string;
  username: string;
  email: string;
  role: "admin" | "user";
  status: "pending" | "approved" | "rejected";
}
```

### Campaign
```typescript
{
  id: string;
  name: string;
  description: string;
  missions: Mission[];
  ownerId: string;
  createdAt: string; // ISO 8601
  updatedAt: string; // ISO 8601
}
```

### Mission
```typescript
{
  id: string;
  name: string;
  description: string;
  maps: GameMap[];
  campaignId: string;
}
```

### GameMap
```typescript
{
  id: string;
  name: string;
  imageUrl: string | null;
  rows: number;
  cols: number;
  tokens: MapTokenInstance[];
  gridColor: string; // Hex color
  gridOpacity: number; // 0.0 - 1.0
  missionId: string;
}
```

### TokenDefinition
```typescript
{
  id: string;
  name: string;
  imageUrl: string;
  size: 1 | 2 | 3;
  type: "player" | "enemy";
  userId: string;
}
```

### MapTokenInstance
```typescript
{
  instanceId: string;
  tokenId: string; // References TokenDefinition
  x: number;
  y: number;
}
```

## Rate Limiting

Currently, no rate limiting is implemented. Consider adding rate limiting in production.

## CORS

CORS is configured to allow all origins in development. Update CORS policy for production deployment.

## Versioning

Current API version: v1

Future versions will be accessible via `/api/v2/...`

---

## OAuth Authentication

The API supports OAuth authentication through Google and Apple Sign-In providers. This allows users to authenticate without creating a password.

### OAuth Flows Supported

1. **Authorization Code Flow**: For web applications
2. **ID Token Validation**: For mobile and single-page applications (SPAs)

### Get OAuth Authorization URL

**Endpoint**: `GET /api/v1/auth/oauth/{provider}/url`

**Parameters**:
- `provider` (path): OAuth provider ("google" or "apple")
- `redirectUri` (query, optional): Custom redirect URI (defaults to configured backend redirect URI)

**Response**:
```json
{
  "success": true,
  "data": {
    "authorizationUrl": "https://accounts.google.com/o/oauth2/v2/auth?client_id=...",
    "state": "random-state-string"
  },
  "message": "google authorization URL generated."
}
```

### OAuth Callback (Authorization Code Flow)

**Endpoint**: `POST /api/v1/auth/oauth/callback`

**Body**:
```json
{
  "provider": "google",
  "code": "authorization-code-from-provider",
  "redirectUri": "https://your-app.com/callback"
}
```

**Response**: Same as login endpoint (returns user info and JWT token)

**Example Response**:
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIs...",
    "id": "user-id",
    "username": "john_doe",
    "email": "john@example.com",
    "role": "user",
    "status": "approved"
  },
  "message": "OAuth login successful."
}
```

### OAuth Token Validation (For Mobile/SPA)

**Endpoint**: `POST /api/v1/auth/oauth/token`

**Body**:
```json
{
  "provider": "google",
  "idToken": "id-token-from-google-sdk"
}
```

**Response**: Same as OAuth callback (returns user info and JWT token)

### OAuth Provider Configuration

#### Google OAuth

To use Google OAuth, you need to:
1. Create a Google Cloud project
2. Create OAuth 2.0 credentials (Web application type)
3. Add redirect URIs to your project
4. Configure credentials in `appsettings.json`:
```json
{
  "OAuth": {
    "Google": {
      "ClientId": "your-client-id.apps.googleusercontent.com",
      "ClientSecret": "your-client-secret"
    }
  }
}
```

#### Apple OAuth

To use Apple Sign-In, you need to:
1. Enroll in Apple Developer Program
2. Create App IDs and sign certificates
3. Create a private key for server-to-server communication
4. Configure credentials in `appsettings.json`:
```json
{
  "OAuth": {
    "Apple": {
      "ClientId": "com.yourcompany.appid",
      "TeamId": "your-team-id",
      "KeyId": "your-key-id",
      "PrivateKey": "your-private-key-in-base64"
    }
  }
}
```

### OAuth User Management

- When a user authenticates via OAuth for the first time, a new account is automatically created
- The account is automatically approved (no admin approval needed)
- If an email already exists, the OAuth provider is linked to the existing account
- User profile pictures from OAuth providers are stored and returned in the API

### JWT Token Usage

After OAuth authentication, the API returns a JWT token in the response. Use this token in subsequent requests:

```bash
curl -H "Authorization: Bearer <your-jwt-token>" http://localhost:5000/api/v1/campaigns
```
