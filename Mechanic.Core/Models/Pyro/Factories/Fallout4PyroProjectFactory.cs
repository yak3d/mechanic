namespace Mechanic.Core.Models.Pyro.Factories;

using global::Pyro.Models.Generated;
using Constants;

public class Fallout4PyroProjectFactory : IPyroProjectFactory
{
    public Task<PapyrusProject> CreateProjectAsync(string projectName, string namespaceName, string gamePath) => Task.FromResult(new PapyrusProjectBuilder().WithFlags(PyroConstants.Fallout4PapyrusFlags)
            .WithGame(PyroConstants.Fallout4Game)
            .WithAnonymize()
            .WithOptimize()
            .WithOutput("@OutputPath")
            .WithOptimize()
            .WithZip(false)
            .WithPackage(false)
            .WithFinal(false)
            .AddVariable("ModName", projectName)
            .AddVariable("Namespace", namespaceName)
            .AddVariable("OutputPath", Path.Combine(gamePath, BgsFileConstants.GameDataDirectoryName))
            .AddImport(Path.Combine(gamePath, FalloutFileConstants.ScriptSourceDirectory))
            .AddScript($@"{namespaceName}\{namespaceName}TestScript.psc")
            .AddFolder(@".\Scripts")
            .Build()
    );
}
