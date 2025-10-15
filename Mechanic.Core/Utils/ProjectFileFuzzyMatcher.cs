namespace Mechanic.Core.Utils;

using FuzzySharp;
using Models;

public static class ProjectFileFuzzyMatcher
{
    /// <summary>
    /// Compares an enumerable of ProjectFiles and compares their paths to find ones that fuzzy match.
    /// </summary>
    /// <param name="files">The files you want to search through</param>
    /// <param name="pathToMatch">The file to match to based on path</param>
    /// <typeparam name="T">The type of the file, should be an inheritor of ProjectFile</typeparam>
    /// <returns>An IEnumerable of the matching files and ordered by their score</returns>
    public static IEnumerable<ScoredProjectFile<T>> FuzzyMatch<T>(IEnumerable<T> files, string pathToMatch) where T : ProjectFile => files
        .Select(gf => new ScoredProjectFile<T>(gf, Fuzz.Ratio(pathToMatch, Path.GetFileNameWithoutExtension((string?)gf.Path))))
        .Where(match => match.Score >= 70)
        .OrderByDescending(match => match.Score);
}
