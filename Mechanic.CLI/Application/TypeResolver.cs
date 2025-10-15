using Spectre.Console.Cli;

namespace Mechanic.CLI.Application;

public class TypeResolver(IServiceProvider serviceProvider) : ITypeResolver, IDisposable
{
    private readonly IServiceProvider serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    public object? Resolve(Type? type)
    {
        if (type == null)
        {
            return null;
        }

        return this.serviceProvider.GetService(type);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (this.serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
