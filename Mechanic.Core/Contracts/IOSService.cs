namespace Mechanic.Core.Contracts;

using LanguageExt;
using Services.Errors;
using static LanguageExt.Prelude;
public interface IOSService
{
    Task<Either<OSError, bool>> ExecutableIsInPathAsync(string executableName);
}
