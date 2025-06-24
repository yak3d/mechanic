using Mechanic.Core.Models;

namespace Mechanic.Core.Contracts;

public interface IProjectSerializationService<T>
{
    public MechanicProject DeserializeProject(T source);
    public void SerializeProject(MechanicProject project, T destination);
}
