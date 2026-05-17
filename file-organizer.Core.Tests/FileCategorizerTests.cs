using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace file_organizer.Core.Tests;

public class FileCategorizerTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    public FileCategorizerTests()
    {
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public void CategorizeFiles_KnownExtension_MapsToCategory()
    {
        File.Create(Path.Combine(_tempDir, "photo.jpg")).Dispose();
        var lookup = new Dictionary<string, string> { { ".jpg", "Images" } };
        var categorizer = new FileCategorizer(NullLogger<FileCategorizer>.Instance);

        var result = categorizer.CategorizeFiles(_tempDir, lookup);

        Assert.Single(result);
        Assert.Equal("Images", result[0].category);
    }

    [Fact]
    public void CategorizeFiles_UnknownExtension_MapsToUnknown()
    {
        File.Create(Path.Combine(_tempDir, "file.xyz")).Dispose();
        var lookup = new Dictionary<string, string>(); // intentionally empty — no mappings to simulate unknown extension
        var categorizer = new FileCategorizer(NullLogger<FileCategorizer>.Instance);

        var result = categorizer.CategorizeFiles(_tempDir, lookup);

        Assert.Single(result);
        Assert.Equal("UNKNOWN", result[0].category);
    }

    [Fact]
    public void CategorizeFiles_EmptyDirectory_ReturnsEmpty()
    {
        var categorizer = new FileCategorizer(NullLogger<FileCategorizer>.Instance);

        var result = categorizer.CategorizeFiles(_tempDir, new Dictionary<string, string>());

        Assert.Empty(result);
    }

    [Fact]
    public void CategorizeFiles_MissingDirectory_ReturnsEmpty()
    {
        var categorizer = new FileCategorizer(NullLogger<FileCategorizer>.Instance);

        var result = categorizer.CategorizeFiles("/nonexistent/path", new Dictionary<string, string>());

        Assert.Empty(result);
    }
}