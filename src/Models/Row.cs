namespace WatDiv.Models;

internal sealed record Row
{
    public Row(IReadOnlyList<string> cols)
    {
        (Subject, Property, Object) = (cols[0], cols[1], cols[2].Replace(" .", string.Empty));
        PropertyHash = new Hash(Property);
    }

    public string Subject { get; }

    public string Property { get; }

    public Hash PropertyHash { get; }

    public string Object { get; }
}