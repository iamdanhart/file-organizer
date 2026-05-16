using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;

namespace file_organizer;

public static class Categories
{
    public static readonly FrozenDictionary<string, string> ExtToCategory =
        new Dictionary<string, string[]>
            {
                { "Images", [".jpg", ".png"] },
                { "Videos", [".mp4", ".mov"] },
            }
            .SelectMany(kvp => kvp.Value, (kvp, ext) => (ext, category: kvp.Key))
            .ToDictionary(pair => pair.ext, pair => pair.category)
            .ToFrozenDictionary();
}