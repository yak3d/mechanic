namespace Mechanic.CLI.Contracts;

public interface IGitService
{
    public bool IsGitInstalled();
    public bool GitInit();
}