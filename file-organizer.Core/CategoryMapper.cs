using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;

namespace file_organizer;

public static class CategoryMapper
{
    // Config stores category → extensions[] for readability; invert to extension → category for O(1) lookup
    public static FrozenDictionary<string, string> BuildLookup(AppConfig config) =>
        config.Categories
            .SelectMany(kvp => kvp.Value, (kvp, ext) => (ext, category: kvp.Key))
            .ToDictionary(pair => pair.ext, pair => pair.category)
            .ToFrozenDictionary();
}