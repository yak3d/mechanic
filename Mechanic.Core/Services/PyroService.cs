namespace Mechanic.Core.Services;

using Constants;
using Contracts;
using Errors;
using Infrastructure.Logging;
using LanguageExt;
using Microsoft.Extensions.Logging;
using static LanguageExt.Prelude;
using Models;
using Models.Pyro;
using Models.Pyro.Factories;
using Pyro.Models.Generated;

public class PyroService(ILogger<PyroService> logger, IXmlSerializer xmlSerializer, IFileService fileService) : IPyroService
{
    public Task<Either<PyroError, string>> CreatePyroProjectAsync(
        GameName gameName,
        string projectName,
        string namespaceName,
        string gamePath
        ) => GetFactoryForGame(gameName).BindAsync(async factory =>
        {
            try
            {
                var project = await factory.CreateProjectAsync(projectName, namespaceName, gamePath);
                var projectAsXml = await xmlSerializer.SerializeAsync(project);

                var filename = $"{projectName}.ppj";
                var projectFilePath = Path.Combine(
                    gamePath,
                    BgsFileConstants.GameDataDirectoryName,
                    BgsFileConstants.ScriptsDirectoryName,
                    filename
                );
                await fileService.WriteAllText(
                    projectFilePath,
                    projectAsXml
                );

                return projectFilePath;
            }
            catch (InvalidOperationException xmlEx)
            {
                logger.FailedToSerializePyroProject(xmlEx);
                return Left<PyroError, string>(new UnableToSerializeProjectToXml());
            }
        });

    private static Either<PyroError, IPyroProjectFactory> GetFactoryForGame(GameName gameName) => gameName switch
    {
        GameName.Tes4Oblivion
            or GameName.Fallout3
            or GameName.FalloutNewVegas => new GameUnsupportedInPyro(gameName),
        GameName.Tes5Skyrim => new SkyrimPyroProjectFactory(),
        GameName.SkyrimSpecialEdition => new SkyrimSEPyroProjectFactory(),
        GameName.Fallout4 => new Fallout4PyroProjectFactory(),
        GameName.Starfield => new StarfieldPyroProjectFactory(),
        _ => new GameUnsupportedInPyro(gameName)
    };
}
