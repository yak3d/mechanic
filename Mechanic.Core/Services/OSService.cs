namespace Mechanic.Core.Services;

using Contracts;
using Errors;
using Infrastructure.Logging;
using LanguageExt;
using Microsoft.Extensions.Logging;

public class OSService(ILogger<OSService> logger, IFileService fileService) : IOSService
{
    private readonly char pathSeparator = OperatingSystem.IsWindows() ? ';' : ':';
    public async Task<Either<OSError, bool>> ExecutableIsInPathAsync(string executableName)
    {
        var pathVariable = Environment.GetEnvironmentVariable("PATH");

        if (string.IsNullOrEmpty(pathVariable))
        {
            logger.PathVariableIsEmpty();
            return false;
        }
        logger.PathVariableContents(pathVariable);

        var pathDirs = pathVariable.Split(this.pathSeparator, StringSplitOptions.RemoveEmptyEntries).ToList();

        foreach (var pathDir in pathDirs)
        {
            if (await this.ExistsInDirectoryAsync(pathDir.Trim(), executableName))
            {
                return true;
            }
        }

        return false;
    }

    private async Task<bool> ExistsInDirectoryAsync(string pathDir, string executableName)
    {
        try
        {
            if (!await fileService.DirectoryExists(pathDir))
            {
                return false;
            }

            if (OperatingSystem.IsWindows())
            {
                List<string> exeExtensions = [".exe", ".cmd", ".bat", ".com"];
                foreach (var extension in exeExtensions)
                {
                    var nameWithExtension = executableName.EndsWith(extension, StringComparison.OrdinalIgnoreCase)
                        ? executableName
                        : executableName + extension;

                    var fullPathToExecutable = Path.Combine(pathDir, nameWithExtension);
                    if (await fileService.FileExistsAsync(fullPathToExecutable))
                    {
                        return true;
                    }
                }
            }
            else
            {
                var fullPathToExecutable = Path.Combine(pathDir, executableName);
                if (await fileService.FileExistsAsync(fullPathToExecutable))
                {
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            logger.ExceptionThrownWhenCheckingForExeInPath(executableName, pathDir, ex);
            return false;
        }

        return false;
    }
}
