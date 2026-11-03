
using DevTrackAPI.DTOs;
using DevTrackAPI.Validation;
using Xunit;

namespace DevTrack.Api.Tests.Unit;

public class ProjectValidationTests
{
    [Fact]
    public void ValidProject_ReturnsNoValidationErrors()
    {
        // Arrange
        var dto = new CreateProjectDto
        {
            Name = "DevTrack API",
            Description = "Backend project management API"
        };

        // Act
        var errors = RequestValidation.Validate(dto);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void EmptyProjectName_ReturnsValidationError()
    {
        // Arrange
        var dto = new CreateProjectDto
        {
            Name = ""
        };

        // Act
        var errors = RequestValidation.Validate(dto);

        // Assert
        Assert.Contains("Name", errors.Keys);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("AB")]
    public void ShortProjectName_ReturnsValidationError(
        string name)
    {
        // Arrange
        var dto = new CreateProjectDto
        {
            Name = name
        };

        // Act
        var errors = RequestValidation.Validate(dto);

        // Assert
        Assert.Contains("Name", errors.Keys);
    }

    [Fact]
    public void ProjectNameOver100Characters_ReturnsError()
    {
        // Arrange
        var dto = new CreateProjectDto
        {
            Name = new string('A', 101)
        };

        // Act
        var errors = RequestValidation.Validate(dto);

        // Assert
        Assert.Contains("Name", errors.Keys);
    }

    [Fact]
    public void DescriptionOver500Characters_ReturnsError()
    {
        // Arrange
        var dto = new CreateProjectDto
        {
            Name = "DevTrack API",
            Description = new string('A', 501)
        };

        // Act
        var errors = RequestValidation.Validate(dto);

        // Assert
        Assert.Contains("Description", errors.Keys);
    }
}
