using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace file_organizer;

public interface ICategorizeOperation
{
    void Run(List<(string fileName, string category)> files);
}

public abstract class FileOperationBase(string sourceDirectory, ILogger logger) : ICategorizeOperation
{
    public void Run(List<(string fileName, string category)> files) =>                                                                                          
        ProcessFiles(files, GetFileOperation());                                                                                                                
                                              
    protected abstract Action<string, string> GetFileOperation();                                                                                               
                                                    
    private void ProcessFiles(List<(string fileName, string category)> files, Action<string, string> fileOp)                                                    
    {
        foreach (var (fileName, category) in files)
        {
            if (category == "UNKNOWN") continue; 
            
            string destinationPath = Path.Combine(sourceDirectory, category);
            try
            {
                Directory.CreateDirectory(destinationPath);
                fileOp(Path.Combine(sourceDirectory, fileName), Path.Combine(destinationPath, fileName));
            }
            catch (UnauthorizedAccessException)
            {
                logger.LogError(
                    "Insufficient permissions to perform operation for {FileName} in {DestinationPath}, skipping...", fileName, destinationPath);
            }
            catch (FileNotFoundException)
            {
                logger.LogError("Unable to find source file {FileName}, skipping...", fileName);
            }
            catch (IOException)
            {
                logger.LogError("Unable to write file {FileName} to {DestinationPath}, " +
                                "file may already exist there or dest isn't a directory, skipping...", fileName, destinationPath);
            }
        }                                                                                                                               
    }                                             
}


public class DryRunOperation(ILogger logger) : ICategorizeOperation
{
    public void Run(List<(string fileName, string category)> files)
    {
        logger.LogInformation("Dry run initiated");
        var fileNamePaddingSize = 3 + files.Max(p => p.fileName.Length);

        foreach (var (fileName, category) in files)
        {
            logger.LogInformation("{FileName} ->  {Category}", fileName.PadRight(fileNamePaddingSize), category);
        }
    }
}

public class CopyOperation(string sourceDirectory, ILogger logger) : FileOperationBase(sourceDirectory, logger)
{
    protected override Action<string, string> GetFileOperation() => File.Copy;
}

public class ExecuteOperation(string sourceDirectory, ILogger logger) : FileOperationBase(sourceDirectory,  logger)
{
    protected override Action<string, string> GetFileOperation() => File.Move;
}