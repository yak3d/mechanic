namespace Mechanic.Core.Utils;

using Models;

public record ScoredProjectFile<T>(T File, int Score) where T : ProjectFile;
