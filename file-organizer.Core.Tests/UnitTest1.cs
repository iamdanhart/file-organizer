namespace file_organizer.Core.Tests;

public class UnitTest1
{
    [Fact]
    public void BuildLookup_MapsExtensionsToCategory()
    {
        var config = new AppConfig
        {
            Categories = new Dictionary<string, string[]>
            {
                { "Images", [".jpg", ".png"] }
            }
        };

        var lookup = CategoryMapper.BuildLookup(config);

        Assert.Equal("Images", lookup[".jpg"]);
        Assert.Equal("Images", lookup[".png"]);
    }
}