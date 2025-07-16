namespace Mechanic.Core.Models.Pyro;

using System.Collections.ObjectModel;
using global::Pyro.Models.Generated;

public class PapyrusProjectBuilder
{
    private readonly PapyrusProject project;

    public PapyrusProjectBuilder() => this.project = new PapyrusProject();

    public PapyrusProjectBuilder WithGame(string game) =>
        (this.project.Game = game, this).Item2;

    public PapyrusProjectBuilder WithOutput(string output) =>
        (this.project.Output = output, this).Item2;

    public PapyrusProjectBuilder WithFlags(string flags) =>
        (this.project.Flags = flags, this).Item2;

    public PapyrusProjectBuilder WithAssembly(string asm) =>
        (this.project.Asm = asm, this).Item2;

    public PapyrusProjectBuilder WithOptimize(bool optimize = true) =>
        (this.project.Optimize = optimize ? "true" : "false", this).Item2;

    public PapyrusProjectBuilder WithRelease(bool release = true) =>
        (this.project.Release = release ? "true" : "false", this).Item2;

    public PapyrusProjectBuilder WithFinal(bool final = true) =>
        (this.project.Final = final ? "true" : "false", this).Item2;

    public PapyrusProjectBuilder WithAnonymize(bool anonymize = true) =>
        (this.project.Anonymize = anonymize ? "true" : "false", this).Item2;

    public PapyrusProjectBuilder WithPackage(bool package = true) =>
        (this.project.Package = package ? "true" : "false", this).Item2;

    public PapyrusProjectBuilder WithZip(bool zip = true) =>
        (this.project.Zip = zip ? "true" : "false", this).Item2;

    public PapyrusProjectBuilder AddVariable(string name, string value)
    {
        if (this.project.Variables.Count == 0)
        {
            this.project.Variables.Add(new VariableList());
        }

        this.project.Variables[0].Variable.Add(new NameValuePair
        {
            Name = name,
            Value = value
        });

        return this;
    }

    public PapyrusProjectBuilder AddVariables(Dictionary<string, string> variables)
    {
        foreach (var kvp in variables)
        {
            this.AddVariable(kvp.Key, kvp.Value);
        }
        return this;
    }

    public PapyrusProjectBuilder AddImport(string import)
    {
        if (this.project.Imports.Count == 0)
        {
            this.project.Imports.Add(new ImportList());
        }

        this.project.Imports[0].Import.Add(import);
        return this;
    }

    public PapyrusProjectBuilder AddImports(params string[] imports)
    {
        foreach (var import in imports)
        {
            this.AddImport(import);
        }
        return this;
    }

    public PapyrusProjectBuilder AddFolder(string path, bool noRecurse = false)
    {
        if (this.project.Folders.Count == 0)
        {
            this.project.Folders.Add(new FolderList());
        }

        this.project.Folders[0].Folder.Add(new RecursablePath
        {
            Text = new[] { path },
            NoRecurse = noRecurse ? "true" : "false"
        });

        return this;
    }

    public PapyrusProjectBuilder AddScript(string script)
    {
        if (this.project.Scripts.Count == 0)
        {
            this.project.Scripts.Add(new ScriptList());
        }

        this.project.Scripts[0].Script.Add(script);
        return this;
    }

    public PapyrusProjectBuilder AddScripts(params string[] scripts)
    {
        foreach (var script in scripts)
        {
            this.AddScript(script);
        }
        return this;
    }

    public PapyrusProjectBuilder AddPackage(string name, string rootDir, string? output = null)
    {
        if (this.project.Packages.Count == 0)
        {
            this.project.Packages.Add(new PackageList { Output = output });
        }

        this.project.Packages[0].Package.Add(new IncludeBase
        {
            Name = name,
            RootDir = rootDir
        });

        return this;
    }

    public PapyrusProjectBuilder WithPackageInclude(int packageIndex, string path, bool noRecurse = false)
    {
        if (packageIndex < this.project.Packages[0].Package.Count)
        {
            this.project.Packages[0].Package[packageIndex].Include.Add(new IncludePattern
            {
                Path = path,
                NoRecurse = noRecurse ? "true" : "false"
            });
        }
        return this;
    }

    public PapyrusProjectBuilder AddZipFile(string name, string rootDir, string compression = "deflate")
    {
        if (this.project.ZipFiles.Count == 0)
        {
            this.project.ZipFiles.Add(new ZipList());
        }

        this.project.ZipFiles[0].ZipFile.Add(new IncludeZip
        {
            Name = name,
            RootDir = rootDir,
            Compression = compression
        });

        return this;
    }

    public PapyrusProjectBuilder AddPreBuildCommand(string command, string? description = null, bool useInBuild = true) => this.AddBuildEvent(this.project.PreBuildEvent, command, description, useInBuild);

    public PapyrusProjectBuilder AddPostBuildCommand(string command, string? description = null, bool useInBuild = true) => this.AddBuildEvent(this.project.PostBuildEvent, command, description, useInBuild);

    public PapyrusProjectBuilder AddPreCompileCommand(string command, string? description = null, bool useInBuild = true) => this.AddBuildEvent(this.project.PreCompileEvent, command, description, useInBuild);

    public PapyrusProjectBuilder AddPostCompileCommand(string command, string? description = null, bool useInBuild = true) => this.AddBuildEvent(this.project.PostCompileEvent, command, description, useInBuild);

    public PapyrusProjectBuilder AddPrePackageCommand(string command, string? description = null, bool useInBuild = true) => this.AddBuildEvent(this.project.PrePackageEvent, command, description, useInBuild);

    public PapyrusProjectBuilder AddPostPackageCommand(string command, string? description = null, bool useInBuild = true) => this.AddBuildEvent(this.project.PostPackageEvent, command, description, useInBuild);

    private PapyrusProjectBuilder AddBuildEvent(Collection<CommandList> eventCollection, string command, string? description, bool useInBuild)
    {
        if (eventCollection.Count == 0)
        {
            eventCollection.Add(new CommandList
            {
                Description = description,
                UseInBuild = useInBuild ? "true" : "false"
            });
        }

        eventCollection[0].Command.Add(command);
        return this;
    }

    public PapyrusProject Build() => this.project;

    public static PapyrusProjectBuilder CreateFallout4Project(string outputPath) =>
        new PapyrusProjectBuilder()
            .WithGame("fo4")
            .WithOutput(outputPath);

    public static PapyrusProjectBuilder CreateDebugProject(string outputPath) =>
        CreateFallout4Project(outputPath)
            .WithOptimize(false)
            .WithRelease(false);

    public static PapyrusProjectBuilder CreateReleaseProject(string outputPath) =>
        CreateFallout4Project(outputPath)
            .WithOptimize()
            .WithRelease()
            .WithFinal()
            .WithPackage();
}
