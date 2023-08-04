using WatDiv.Config.PropConfig;
using WatDiv.Models;

namespace WatDiv.WatDivProcessing.Context;

internal sealed class WatDivContext
{
    public WatDivContext(Dictionary<string, Hash> dictPropHashes, IPropConfig propConfig, IReadOnlyCollection<Relation> relations, TextWriter resultFile)
    {
        var tables = dictPropHashes.Select(x => new KeyValuePair<Hash, List<Relation>>(x.Value, new List<Relation>())).ToDictionary();
        foreach (var table in tables)
        {
            table.Value.AddRange(relations.Where(x => x.PropertyHash == table.Key));
        }

        foreach (string prop in dictPropHashes.Keys)
        {
            resultFile.WriteLine($"   {prop,-10}: {tables[dictPropHashes[prop]].Count:#,#} Entries");
        }

        Follows = tables[dictPropHashes[propConfig.Follows]];
        FriendOf = tables[dictPropHashes[propConfig.FriendOf]];
        Likes = tables[dictPropHashes[propConfig.Likes]];
        HasReview = tables[dictPropHashes[propConfig.HasReview]];
    }

    public IReadOnlyList<Relation> Follows { get; }
    public IReadOnlyList<Relation> FriendOf { get; }
    public IReadOnlyList<Relation> Likes { get; }
    public IReadOnlyList<Relation> HasReview { get; }
}