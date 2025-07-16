namespace Mechanic.Core.Models.Pyro;

using Constants;
using global::Pyro.Models.Generated;

public class SkyrimPyroProjectFactory : IPyroProjectFactory
{
    public Task<PapyrusProject> CreateProjectAsync(string projectName, string namespaceName, string gamePath) =>
        Task.FromResult(
            new PapyrusProjectBuilder()
                .WithGame(PyroConstants.SkyrimGame)
                .WithAnonymize()
                .WithOutput("@OutputPath\\scripts")
                .WithOptimize()
                .WithRelease(false)
                .WithZip(false)
                .WithPackage(false)
                .WithFinal(false)
                .AddVariable("ModName", projectName)
                .AddVariable("Namespace", namespaceName)
                .AddVariable("OutputPath", Path.Combine(gamePath, BgsFileConstants.ScriptsDirectoryName))
                .AddImport(Path.Combine(gamePath, SkyrimFileConstants.ScriptSourceDirectory))
                .AddScript($"{namespaceName}_Script.psc")
                .AddFolder(@".\Scripts")
                .Build()
        );
}
