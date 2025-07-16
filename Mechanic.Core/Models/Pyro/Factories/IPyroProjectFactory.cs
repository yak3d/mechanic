namespace Mechanic.Core.Models.Pyro;

using global::Pyro.Models.Generated;

public interface IPyroProjectFactory
{
    Task<PapyrusProject> CreateProjectAsync(
        string projectName,
        string namespaceName,
        string gamePath
    );
}
