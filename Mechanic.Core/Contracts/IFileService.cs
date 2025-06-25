namespace Mechanic.Core.Contracts;

public interface IFileService
{
    public Task<string> ReadAllText(string path);
    public Task WriteAllText(string path, string contents);
}
