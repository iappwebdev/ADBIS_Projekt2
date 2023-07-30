using System.Reflection;

namespace WatDiv.Config.FileConfig;

internal sealed class FileNameProvider10M : IFileNameProvider
{
    public string InputFile => "O:/WatDiv_InputData/watdiv.10M.nt";

    public string OutputFile => "O:/WatDiv_OutputResults/result10M.txt";

    public string OutputFileQueryResultsHashJoin => "O:/WatDiv_OutputResults/result10M_hash_join.txt";

    public string OutputFileQueryResultsSortMergeJoin => "O:/WatDiv_OutputResults/result10M_sort_merge_join.txt";
}