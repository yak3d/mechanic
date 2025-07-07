namespace Mechanic.Core.Services.Errors;

public record CheckError(string message = "Failed to check file") : ProjectError(message);
