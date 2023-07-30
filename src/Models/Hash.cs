namespace WatDiv.Models;

/// <summary>
/// Manages the assignment of a string to a long value. If a string does not yet exist around the dictionary, then a new long value is assigned to the string.
/// </summary>
/// <param name="StringValue"></param>
internal sealed record Hash(string StringValue)
{
    private static readonly Dictionary<string, long> Dict = new();
    private static long Counter;

    private static long GetValue(string stringValue)
    {
        if (Dict.TryGetValue(stringValue, out long value)) return value;
        Dict[stringValue] = Counter;
        return Counter++;
    }

    public long Value { get; } = GetValue(StringValue);
}