namespace WatDiv.Config.FileConfig;

internal sealed class FileNameProvider10M : IFileNameProvider
{
    private readonly string _dirSln;

    public FileNameProvider10M() => _dirSln = FilePathProvider.TryGetSolutionDirectoryInfo().FullName;

    public string InputFile => Path.Combine(_dirSln, "InputData/watdiv.10M.nt");

    public string OutputFile => Path.Combine(_dirSln, "OutputResults/result10M.txt");

    public string OutputFileQueryResultsHashJoin => Path.Combine(_dirSln, "OutputResults/result10M_hash_join.txt");

    public string OutputFileQueryResultsSortMergeJoin => Path.Combine(_dirSln, "OutputResults/result10M_sort_merge_join.txt");
}