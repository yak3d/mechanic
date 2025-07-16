namespace Mechanic.Core.Contracts;

public interface IXmlSerializer
{
    Task<string> SerializeAsync<T>(T obj);
}
