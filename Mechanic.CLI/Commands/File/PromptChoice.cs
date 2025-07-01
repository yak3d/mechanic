namespace Mechanic.CLI.Commands.File;

public abstract record PromptChoice(string DisplayName)
{
    public sealed override string ToString() => DisplayName;
}

public record EmptyHeader() : PromptChoice("");