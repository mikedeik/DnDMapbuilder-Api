using FluentAssertions;
using Xunit;
using DnDMapBuilder.Application.Services;

namespace DnDMapBuilder.UnitTests.Services;

/// <summary>
/// Unit tests for FileValidationService.
/// </summary>
public class FileValidationServiceTests
{
    private readonly FileValidationService _fileValidationService = new();

    [Fact]
    public void ValidateFile_WithValidMapFile_ShouldReturnSuccess()
    {
        // Arrange
        var fileName = "map.png";
        var fileSize = 1024 * 1024; // 1MB
        var contentType = "image/png";
        var storageCategory = "maps";

        // Act
        var result = _fileValidationService.ValidateFile(fileName, fileSize, contentType, storageCategory);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateFile_WithValidTokenFile_ShouldReturnSuccess()
    {
        // Arrange
        var fileName = "token.jpeg";
        var fileSize = 512 * 1024; // 512KB
        var contentType = "image/jpeg";
        var storageCategory = "tokens";

        // Act
        var result = _fileValidationService.ValidateFile(fileName, fileSize, contentType, storageCategory);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateFile_WithEmptyFileName_ShouldReturnError()
    {
        // Arrange
        var fileSize = 1024 * 1024;
        var contentType = "image/png";
        var storageCategory = "maps";

        // Act
        var result = _fileValidationService.ValidateFile("", fileSize, contentType, storageCategory);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Contains("File name cannot be empty"));
    }

    [Fact]
    public void ValidateFile_WithEmptyFile_ShouldReturnError()
    {
        // Arrange
        var fileName = "file.png";
        var fileSize = 0;
        var contentType = "image/png";
        var storageCategory = "maps";

        // Act
        var result = _fileValidationService.ValidateFile(fileName, fileSize, contentType, storageCategory);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Contains("File cannot be empty"));
    }

    [Fact]
    public void ValidateFile_WithFileSizeExceedingMapLimit_ShouldReturnError()
    {
        // Arrange
        var fileName = "large_map.png";
        var fileSize = 6 * 1024 * 1024; // 6MB (limit is 5MB for maps)
        var contentType = "image/png";
        var storageCategory = "maps";

        // Act
        var result = _fileValidationService.ValidateFile(fileName, fileSize, contentType, storageCategory);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Contains("exceeds the maximum limit"));
    }

    [Fact]
    public void ValidateFile_WithFileSizeExceedingTokenLimit_ShouldReturnError()
    {
        // Arrange
        var fileName = "large_token.png";
        var fileSize = 3 * 1024 * 1024; // 3MB (limit is 2MB for tokens)
        var contentType = "image/png";
        var storageCategory = "tokens";

        // Act
        var result = _fileValidationService.ValidateFile(fileName, fileSize, contentType, storageCategory);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Contains("exceeds the maximum limit"));
    }

    [Fact]
    public void ValidateFile_WithInvalidMimeType_ShouldReturnError()
    {
        // Arrange
        var fileName = "file.pdf";
        var fileSize = 1024 * 1024;
        var contentType = "application/pdf";
        var storageCategory = "maps";

        // Act
        var result = _fileValidationService.ValidateFile(fileName, fileSize, contentType, storageCategory);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Contains("not allowed"));
    }

    [Fact]
    public void ValidateFile_WithWebpMimeType_ShouldReturnSuccess()
    {
        // Arrange
        var fileName = "image.webp";
        var fileSize = 1024 * 1024;
        var contentType = "image/webp";
        var storageCategory = "maps";

        // Act
        var result = _fileValidationService.ValidateFile(fileName, fileSize, contentType, storageCategory);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateFile_WithNullContentType_ShouldReturnError()
    {
        // Arrange
        var fileName = "file.png";
        var fileSize = 1024 * 1024;
        var contentType = (string?)null;
        var storageCategory = "maps";

        // Act
        var result = _fileValidationService.ValidateFile(fileName, fileSize, contentType!, storageCategory);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public void GetMaxFileSizeForCategory_Maps_ShouldReturn5MB()
    {
        // Act
        var maxSize = _fileValidationService.GetMaxFileSizeForCategory("maps");

        // Assert
        maxSize.Should().Be(5 * 1024 * 1024);
    }

    [Fact]
    public void GetMaxFileSizeForCategory_Tokens_ShouldReturn2MB()
    {
        // Act
        var maxSize = _fileValidationService.GetMaxFileSizeForCategory("tokens");

        // Assert
        maxSize.Should().Be(2 * 1024 * 1024);
    }

    [Fact]
    public void GetMaxFileSizeForCategory_UnknownCategory_ShouldReturnDefault10MB()
    {
        // Act
        var maxSize = _fileValidationService.GetMaxFileSizeForCategory("unknown");

        // Assert
        maxSize.Should().Be(10 * 1024 * 1024);
    }

    [Fact]
    public void GetAllowedMimeTypesForCategory_Maps_ShouldReturnImageTypes()
    {
        // Act
        var allowedTypes = _fileValidationService.GetAllowedMimeTypesForCategory("maps");

        // Assert
        allowedTypes.Should().Contain(new[] { "image/png", "image/jpeg", "image/webp" });
    }

    [Fact]
    public void ValidateFile_WithMultipleErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var fileName = "file.pdf";
        var fileSize = 6 * 1024 * 1024; // Exceeds limit and wrong type
        var contentType = "application/pdf";
        var storageCategory = "maps";

        // Act
        var result = _fileValidationService.ValidateFile(fileName, fileSize, contentType, storageCategory);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(1);
    }
}
