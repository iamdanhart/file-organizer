using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using file_organizer;
using Microsoft.Extensions.Logging;

var logLevel = args.FlagSet("--verbose") ? LogLevel.Debug : LogLevel.Information;
bool jsonLogs = args.FlagSet("--json-logs");

using var factory = LoggerFactory.Create(builder =>                                                                                                                   
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
    logger.LogError("Usage: file-organizer <directory> [--dry-run] [--copy] [--execute] [--test-data] [--config <path>]");
    logger.LogError("       --test-data: populates the directory with sample files. Do not use on directories with real files.");
    logger.LogError("       --config <path>: path to a JSON config file. Defaults to config.json next to the executable.");
    return 1; // prefer return over Environment.Exit() — runs cleanup (file handles, using blocks, etc.)
}

var directory = args[0];

var configLocation = args.GetParamValue("--config");
if (string.IsNullOrEmpty(configLocation))
    configLocation = Path.Combine(AppContext.BaseDirectory, "config.json");

if (!File.Exists(configLocation))
{
    logger.LogError("Config file not found: {ConfigLocation}", configLocation);
    return 1;
}
var config = System.Text.Json.JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(configLocation));
if (config is null)                                                                                                                                             
{                                           
    logger.LogError("Failed to parse config file: {ConfigLocation}", configLocation);                                                                           
    return 1;                                                                                                                                                   
}      
var categories = CategoryMapper.BuildLookup(config);

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
    sortedPaths =
        new FileCategorizer(factory.CreateLogger<FileCategorizer>())
            .CategorizeFiles(directory, categories);
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