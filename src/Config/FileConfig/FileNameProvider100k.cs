namespace WatDiv.Config.FileConfig;

internal sealed class FileNameProvider100k : IFileNameProvider
{
    private readonly string _dirSln;

    public FileNameProvider100k()
    {
        _dirSln = FilePathProvider.TryGetSolutionDirectoryInfo().FullName;
    }

    public string InputFile => Path.Combine(_dirSln, "InputData/watdiv100k.txt");

    public string OutputFile => Path.Combine(_dirSln, "OutputResults/result100k.txt");

    public string OutputFileQueryResultsHashJoin => Path.Combine(_dirSln, "OutputResults/result100k_hash_join.txt");

    public string OutputFileQueryResultsSortMergeJoin => Path.Combine(_dirSln, "OutputResults/result100k_sort_merge_join.txt");
}