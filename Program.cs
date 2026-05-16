using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using file_organizer;
using Microsoft.Extensions.Logging;

var logLevel = args.FlagSet("--verbose") ? LogLevel.Debug : LogLevel.Information;
bool jsonLogs = args.FlagSet("--json-logs"); 

var factory = LoggerFactory.Create(builder =>                                                                                                                   
{                                                                                                                                                               
    if (jsonLogs)                                                                                                                                               
        builder.AddJsonConsole();                                                                                                                               
    else                                                                                                                                                        
        builder.AddConsole();                     
    builder.SetMinimumLevel(logLevel);
});
ILogger logger = factory.CreateLogger<Program>();


if (args.Length < 1)
{
    logger.LogError("Usage: file-organizer <directory> [--dry-run] [--copy] [--execute] [--test-data]");
    logger.LogError(
        "       --test-data: populates the directory with sample files. Do not use on directories with real files.");
    return 1; // prefer return over Environment.Exit() — runs cleanup (file handles, using blocks, etc.)
}
var directory = args[0];

if (args.FlagSet("--test-data"))
{
    CreateTestData(directory);
    return 0;
}

bool dryRun = args.FlagSet("--dry-run");
bool copyRun = args.FlagSet("--copy");
bool executeRun = args.FlagSet("--execute");
if ((dryRun && copyRun) || (dryRun && executeRun) || (copyRun && executeRun))
{
    logger.LogError("Only one of --dry-run, --copy, or --execute may be specified.");
    return 1;
}

if (!dryRun && !copyRun && !executeRun)
{
    logger.LogError("No operation selected.");
    return 1;
}


logger.LogInformation("Scanning: {Directory}", directory);
List<(string fileName, string category)> sortedPaths;
try
{
    sortedPaths = CategorizeFiles(directory);
    // var argParamKey = "--test";
    // var testValue = args.GetParamValue(argParamKey);
    // if (!string.IsNullOrEmpty(testValue))
    // {
    //     Console.WriteLine($"{argParamKey}: {testValue}");
    // }
}
catch (UnauthorizedAccessException)
{
    logger.LogError("Permission denied:  {Directory}", directory);
    return 1;
}

if (sortedPaths.Count == 0)
{
    logger.LogError("No files in path");
    return 0;
}

ICategorizeOperation operation = (dryRun, copyRun, executeRun) switch
{
    (true, _, _) => new DryRunOperation(factory.CreateLogger<DryRunOperation>()),
    (_, true, _) => new CopyOperation(directory, factory.CreateLogger<CopyOperation>()),
    (_, _, true) => new ExecuteOperation(directory, factory.CreateLogger<ExecuteOperation>()),
    _ => throw new UnreachableException("Impossible to reach, one run must be selected to get here")
};

operation.Run(sortedPaths);

return 0;


List<(string filePath, string category)> CategorizeFiles(string directoryPath)
{
    if (!Directory.Exists(directoryPath))
    {
        logger.LogWarning("Directory not found: {DirectoryPath}", directoryPath);
        return [];
    }

    List<(string, string)> categorizedFiles = new();

    string[] filepaths = Directory.GetFiles(directoryPath);
    Array.Sort(filepaths);

    foreach (var filepath in filepaths)
    {
        var ext = Path.GetExtension(filepath);
        var category = Categories.ExtToCategory.GetValueOrDefault(ext, "UNKNOWN");

        var filename = Path.GetFileName(filepath);

        categorizedFiles.Add((filename, category));
    }

    return categorizedFiles;
}

void CreateTestData(string directoryPath)
{
    foreach (var dir in Directory.GetDirectories(directoryPath))
        Directory.Delete(dir, recursive: true);

    foreach (var file in Directory.GetFiles(directoryPath))
        File.Delete(file);

    var testFiles = new[] { "photo.jpg", "screenshot.png", "video.mp4", "clip.mov", "unknown.xyz" };
    foreach (var file in testFiles)
        File.Create(Path.Combine(directoryPath, file)).Dispose();

    logger.LogInformation("Created {TestFilesLength} test files in {DirectoryPath}", testFiles.Length, directoryPath);
}

static class CollectionExtensions
{
    /// <summary>Returns true if the flag (e.g. "--dry-run") is present in the args array.</summary>
    public static bool FlagSet(this string[] args, string flag)
    {
        return args.Contains(flag);
    }

    /// <summary>Returns the index of val in the array, or -1 if not found. Wraps the static Array.IndexOf.</summary>
    public static int Index(this string[] args, string val)
    {
        return Array.IndexOf(args, val);
    }

    /// <summary>
    /// Returns the value following a named parameter (e.g. "--output value").
    /// Returns empty string if the flag is absent, duplicated, at the end of args, or followed by another flag.
    /// </summary>
    public static string GetParamValue(this string[] args, string parameter)
    {
        var count = args.Count(a => a == parameter);
        if (count == 0)
        {
            return "";
        }

        if (count > 1)
        {
            return "";
        }

        var paramKeyIndex = args.Index(parameter);
        if (paramKeyIndex >= args.Length - 1)
        {
            return "";
        }

        // works as long as valid value doesn't start with --
        // otherwise need --param=value pattern
        if (args[paramKeyIndex + 1].StartsWith("--"))
        {
            return "";
        }

        return args[paramKeyIndex + 1];
    }
}