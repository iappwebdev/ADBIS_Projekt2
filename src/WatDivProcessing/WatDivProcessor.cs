using System.Diagnostics;
using WatDiv.Config.FileConfig;
using WatDiv.Config.PropConfig;
using WatDiv.Models;
using WatDiv.Sanitizing;
using WatDiv.WatDivProcessing.Context;
using WatDiv.WatDivProcessing.Joins;

namespace WatDiv.WatDivProcessing;

internal sealed class WatDivProcessor
{
    private readonly IFileNameProvider _fileNameProvider;
    private readonly IPropConfig _propConfig;
    private readonly ISanitizer _sanitizer;
    private readonly Stopwatch _sw = new();
    private readonly Dictionary<string, Hash> _dictPropHashes;
    private readonly string _separator = new('#', 80);

    private StreamWriter _logFile = null!;
    private const bool _calculateHashJoin = true;
    private const bool _calculateSortMergeJoin = true;

    public WatDivProcessor(
        IFileNameProvider fileNameProvider,
        IPropConfig propConfig,
        ISanitizer sanitizer)
    {
        _fileNameProvider = fileNameProvider;
        _propConfig = propConfig;
        _sanitizer = sanitizer;
        _dictPropHashes = _propConfig.GetPropKeys().Select(x => new KeyValuePair<string, Hash>(x, new Hash(x))).ToDictionary();
    }

    public void Start()
    {
        string outputDir = Path.GetDirectoryName(_fileNameProvider.OutputFile)!;
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        using var fileStream = new FileStream(_fileNameProvider.OutputFile, FileMode.Append, FileAccess.Write);
        _logFile = new StreamWriter(fileStream);
        _logFile.AutoFlush = true;

        _logFile.WriteLine(Environment.NewLine);
        _logFile.WriteLine(_separator);
        _logFile.WriteLine($"##### Execution start at {DateTime.Now.ToLongTimeString()} on {DateTime.Now.ToShortDateString()}");
        _logFile.WriteLine(_separator);

        _logFile.WriteLine($"Read lines of relevant properties: {string.Join(", ", _dictPropHashes.Keys)}");

        _sw.Start();
        var triples = GetTriplesFromInput();
        _sw.Stop();
        _logFile.WriteLine($"Elapsed time: {_sw.Elapsed}");
        _logFile.WriteLine($"{triples.Count:#,#} triples");
        _logFile.WriteLine($"Build context for: {string.Join(", ", _dictPropHashes.Keys)}");
        var context = new WatDivContext(_dictPropHashes, _propConfig, triples, _logFile);

        // Release memory for triples
        // ReSharper disable once RedundantAssignment
        triples = null;

        if (_calculateHashJoin)
        {
            _logFile.WriteLine(_separator);
            _logFile.WriteLine("***** HASH JOIN *****");
            var watDivHashJoin = new WatDivHashJoin(context, _logFile, _fileNameProvider.OutputFileQueryResultsHashJoin);
            ExecuteJoin(watDivHashJoin.GetHashJoinOptimized);
        }

        if (_calculateSortMergeJoin)
        {
            _logFile.WriteLine(_separator);
            _logFile.WriteLine("***** SORT MERGE JOIN *****");
            var watDivSorteMergeJoin = new WatDivSortMergeJoin(context, _logFile, _fileNameProvider.OutputFileQueryResultsSortMergeJoin);
            ExecuteJoin(watDivSorteMergeJoin.GetSortMergeJoin);
        }

        _logFile.WriteLine(_separator);
        _logFile.WriteLine($"##### Execution end at {DateTime.Now.ToLongTimeString()} on {DateTime.Now.ToShortDateString()}");
        _logFile.WriteLine(_separator);
    }

    private void ExecuteJoin(Action getJoinQuery)
    {
        _logFile.WriteLine(new string('*', 80));
        _logFile.WriteLine($"Start {getJoinQuery.Method.Name} over properties at {DateTime.Now.ToLongTimeString()}");
        _sw.Restart();
        getJoinQuery.Invoke();
        _sw.Stop();
        _logFile.WriteLine($"Elapsed time: {_sw.Elapsed}");
    }

    private List<Relation> GetTriplesFromInput() =>
        File.ReadLines(_fileNameProvider.InputFile)
            .Select(x => new Row(_sanitizer.GetSanitizedColumns(x)))
            .Where(x => _dictPropHashes.ContainsValue(x.PropertyHash))
            .Select(x => new Relation(x))
            .OrderBy(x => x.Property)
            .ToList();
}