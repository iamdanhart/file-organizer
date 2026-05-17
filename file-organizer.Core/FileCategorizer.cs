using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;

namespace file_organizer;

public class FileCategorizer(ILogger<FileCategorizer> logger)
{
    public List<(string fileName, string category)> CategorizeFiles(
        string directoryPath,
        IReadOnlyDictionary<string, string> categoriesMap)
    {
        if (!Directory.Exists(directoryPath))
        {
            logger.LogWarning("Directory not found: {DirectoryPath}", directoryPath);
            return [];
        }

        List<(string, string)> categorizedFiles = new();

        string[] filepaths = Directory.GetFiles(directoryPath);
        System.Array.Sort(filepaths);

        foreach (var filepath in filepaths)
        {
            var ext = Path.GetExtension(filepath);
            var category = categoriesMap.GetValueOrDefault(ext, "UNKNOWN");
            var filename = Path.GetFileName(filepath);
            categorizedFiles.Add((filename, category));
        }

        return categorizedFiles;
    }
}