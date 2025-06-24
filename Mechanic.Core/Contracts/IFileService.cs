namespace Mechanic.Core.Contracts;

public interface IFileService
{
    string ReadAllText(string path);
    void WriteAllText(string path, string contents);
}