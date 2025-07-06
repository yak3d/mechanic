namespace Mechanic.Core.Contracts;

using LanguageExt;
using Mechanic.Core.Models;
using Services.Errors;

public interface IProjectService
{
    public Task<MechanicProject> InitializeAsync(string path, string projectId, GameName gameName, string gamePath);
    public Task<MechanicProject> GetCurrentProjectAsync();
    public Task<MechanicProject> UpdateProjectGameAsync(GameName gameName);
    public Task<Either<SourceFileAddError, SourceFile>> AddSourceFileAsync(string path, SourceFileType fileType);
    public Task<Either<SourceFileAddError, SourceFile>> AddSourceFileAsync(string path, SourceFileType fileType,
        Guid? id);
    public Task<Either<SourceFileDoesNotExistAtPathError, SourceFile>> RemoveSourceFileByPathAsync(string path);
    public Task<Either<SourceFileDoesNotExistWithIdError, SourceFile>> RemoveSourceFileByIdAsync(Guid id);
    public Task<Either<GameFileDoesNotExistAtPathError, GameFile>> RemoveGameFileByPathAsync(string path);
    public Task<Either<GameFileDoesNotExistWithIdError, GameFile>> RemoveGameFileByIdAsync(Guid id);
    public Task<Either<GameFileError, GameFile>> AddGameFileAsync(string path, GameFileType fileType);
    /// <summary>
    /// Adds a given <see cref="GameFile"/> to the project, and also links the <see cref="GameFile"/> to <see cref="SourceFile"/> with the Id <c>sourceFileId</c>
    /// </summary>
    /// <param name="path">The relative path where the <see cref="GameFile"/> will be located within the game directory structure</param>
    /// <param name="fileType">The <see cref="GameFileType"/> enumeration value specifying the type of game asset being tracked</param>
    /// <param name="sourceFileId">Optional <see cref="Guid"/> identifier of an existing <see cref="SourceFile"/> to establish bidirectional linkage. If null, the <see cref="GameFile"/> will be created without source linkage</param>
    /// <returns>A <see cref="Task{GameFile}"/> containing the newly created <see cref="GameFile"/> instance with populated metadata and established relationships</returns>
    /// <exception cref="ArgumentException">Thrown when a <see cref="GameFile"/> with the specified path already exists in the project</exception>
    /// <exception cref="InvalidOperationException">Thrown when the provided sourceFileId does not correspond to an existing <see cref="SourceFile"/> in the project</exception>
    public Task<Either<ProjectError, GameFile>> AddGameFileAsync(string path, GameFileType fileType, Guid? sourceFileId);
    /// <summary>
    /// Establishes a bidirectional linkage between an existing <see cref="GameFile"/> and <see cref="SourceFile"/>, creating a traceable relationship within the project's asset dependency graph
    /// </summary>
    /// <param name="gameFileId">The <see cref="Guid"/> identifier of the target <see cref="GameFile"/> to be linked</param>
    /// <param name="sourceFileId">The <see cref="Guid"/> identifier of the target <see cref="SourceFile"/> that will reference the <see cref="GameFile"/></param>
    /// <returns>A <see cref="Task{SourceFile}"/> containing the updated <see cref="SourceFile"/> instance with the newly established <see cref="GameFile"/> linkage added to its GameFileLinks collection</returns>
    public Task LinkGameFileToSourceFileAsync(Guid gameFileId, Guid sourceFileId);
    public Task<GameFile?> FindGameFileByIdAsync(Guid id);
    public Task<SourceFile?> FindSourceFileByIdAsync(Guid id);
    public Task<bool> SourceFileExistsWithPathAsync(string path);
    public Task<bool> GameFileExistsWithPathAsync(string path);
    public Task<Either<CheckError, Dictionary<SourceFile, bool>>> CheckSourceFilesAsync();
    public Task<Either<CheckError, Dictionary<GameFile, bool>>> CheckGameFilesAsync();
}
