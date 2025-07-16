namespace Mechanic.Core.Contracts;

using LanguageExt;
using Models;
using Services.Errors;

public interface IPyroService
{
    public Task<Either<PyroError, string>> CreatePyroProjectAsync(
        GameName gameName,
        string projectName,
        string namespaceName,
        string gamePath
    );
}
