namespace Mechanic.Core.Services.Errors;

using Models;

public record PyroError(string message) : ProjectError(message);

public record GameUnsupportedInPyro(GameName gameName) : PyroError($"{gameName.ToString()} is not supported by Pyro");
public record UnableToSerializeProjectToXml() : PyroError($"Unable to serialize object to XML");
