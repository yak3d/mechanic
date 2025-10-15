using JetBrains.Annotations;
using Mechanic.Core.Project.Models.Json;
using Shouldly;
using MechanicProject = Mechanic.Core.Models.MechanicProject;

namespace Mechanic.Core.Tests.Models;

[TestSubject(typeof(MechanicProject))]
public class MechanicProjectTest
{

    [Fact]
    public void From_Json_Converts_Correctly()
    {
        const string projectId = "com.example.TestProject";
        var sourceFileId1 = Guid.NewGuid().ToString();
        const string meshPath = "meshes/test/test.fbx";
        const SourceFilesFileType sourceFilesFileType = SourceFilesFileType.FBX;

        var input = new Project.Models.Json.MechanicProject
        {
            Id = projectId,
            Namespace = "TEST",
            SourceFiles =
            [
                new SourceFiles
                {
                    Id = sourceFileId1,
                    Path = meshPath,
                    FileType = sourceFilesFileType,
                    DestinationPaths = []
                }
            ]
        };

        var result = MechanicProject.FromJsonObject(input);
        result.ShouldNotBeNull();
        result.Id.ShouldBe(projectId);
        result.SourceFiles[0].Id.ToString().ShouldBe(sourceFileId1);
        result.SourceFiles[0].Path.ShouldBe(meshPath);
        result.SourceFiles[0].FileType.ToString().ToUpper(System.Globalization.CultureInfo.CurrentCulture).ShouldBe(sourceFilesFileType.ToString());
    }
}
