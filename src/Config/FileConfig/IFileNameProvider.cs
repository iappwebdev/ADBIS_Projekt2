namespace WatDiv.Config.FileConfig;

internal interface IFileNameProvider
{
    string InputFile { get; }

    string OutputFile { get; }

    string OutputFileQueryResultsHashJoin { get; }

    string OutputFileQueryResultsSortMergeJoin { get; }
}