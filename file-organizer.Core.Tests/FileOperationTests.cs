using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace file_organizer.Core.Tests;

public class FileOperationTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    public FileOperationTests()
    {
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        Directory.Delete(_tempDir, recursive: true);
    }

    // params: caller passes tuples directly without creating an array — compiler wraps them
    // [..files]: collection expression with spread — expands files into a new List<...>
    private List<(string fileName, string category)> FilesWithCategory(params (string, string)[] files) =>
        [..files];

    [Fact]
    public void ExecuteOperation_MovesFileToCategory()
    {
        File.Create(Path.Combine(_tempDir, "photo.jpg")).Dispose();
        var files = FilesWithCategory(("photo.jpg", "Images"));

        new ExecuteOperation(_tempDir, NullLogger<ExecuteOperation>.Instance).Run(files);

        Assert.False(File.Exists(Path.Combine(_tempDir, "photo.jpg")));
        Assert.True(File.Exists(Path.Combine(_tempDir, "Images", "photo.jpg")));
    }

    [Fact]
    public void ExecuteOperation_SkipsUnknownCategory()
    {
        File.Create(Path.Combine(_tempDir, "file.xyz")).Dispose();
        var files = FilesWithCategory(("file.xyz", "UNKNOWN"));

        new ExecuteOperation(_tempDir, NullLogger<ExecuteOperation>.Instance).Run(files);

        Assert.True(File.Exists(Path.Combine(_tempDir, "file.xyz")));
        Assert.False(Directory.Exists(Path.Combine(_tempDir, "UNKNOWN")));
    }

    [Fact]
    public void CopyOperation_CopiesFileToCategory()
    {
        File.Create(Path.Combine(_tempDir, "photo.jpg")).Dispose();
        var files = FilesWithCategory(("photo.jpg", "Images"));

        new CopyOperation(_tempDir, NullLogger<CopyOperation>.Instance).Run(files);

        Assert.True(File.Exists(Path.Combine(_tempDir, "photo.jpg")));
        Assert.True(File.Exists(Path.Combine(_tempDir, "Images", "photo.jpg")));
    }

    [Fact]
    public void CopyOperation_SkipsUnknownCategory()
    {
        File.Create(Path.Combine(_tempDir, "file.xyz")).Dispose();
        var files = FilesWithCategory(("file.xyz", "UNKNOWN"));

        new CopyOperation(_tempDir, NullLogger<CopyOperation>.Instance).Run(files);

        Assert.True(File.Exists(Path.Combine(_tempDir, "file.xyz")));
        Assert.False(Directory.Exists(Path.Combine(_tempDir, "UNKNOWN")));
    }
}