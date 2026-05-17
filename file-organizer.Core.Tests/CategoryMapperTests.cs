using System.Collections.Generic;
using Xunit;

namespace file_organizer.Core.Tests;

public class CategoryMapperTests
{
    private static readonly AppConfig Config = new()
    {
        Categories = new Dictionary<string, string[]>
        {
            { "Images", [".jpg", ".png"] },
            { "Documents", [".pdf", ".docx", ".txt"] },
            { "Audio", [".mp3", ".flac", ".wav"] }
        }
    };

    [Theory]
    [InlineData(".jpg", "Images")]
    [InlineData(".png", "Images")]
    [InlineData(".pdf", "Documents")]
    [InlineData(".docx", "Documents")]
    [InlineData(".txt", "Documents")]
    [InlineData(".mp3", "Audio")]
    [InlineData(".flac", "Audio")]
    [InlineData(".wav", "Audio")]
    public void BuildLookup_MapsExtensionToCategory(string ext, string expectedCategory)
    {
        var lookup = CategoryMapper.BuildLookup(Config);

        Assert.Equal(expectedCategory, lookup[ext]);
    }
}