using Mechanic.Core.Models;

namespace Mechanic.Core.Contracts;

public interface IProjectSerializationService<T>
{
    MechanicProject DeserializeProject(T source);
    void SerializeProject(MechanicProject project, T destination);
}