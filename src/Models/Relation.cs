namespace WatDiv.Models;

/// <summary>
/// Wrapper for the triple in the dataset. Each Subject and Object are "hashed" by using
/// the Hash-Dictionary
/// </summary>
internal sealed record Relation
{
    public Relation(Row row)
    {
        (Subject, Object, Property, PropertyHash) = (row.Subject, row.Object, row.Property, row.PropertyHash);
        (SubjectHash, ObjectHash) = (new Hash(Subject), new Hash(Object));
    }

    public string Subject { get; }

    public Hash SubjectHash { get; }

    public string Property { get; }

    public Hash PropertyHash { get; }

    public string Object { get; }

    public Hash ObjectHash { get; }

    public override string ToString() => $"{SubjectHash.Value} {Property} {ObjectHash.Value}";
}