namespace WatDiv.Sanitizing;

internal sealed class Sanitizer10M : ISanitizer
{
    public IReadOnlyList<string> GetSanitizedColumns(string line)
    {
        string sanitizedLine = line
            .Replace("<http://db.uwaterloo.ca/~galuc/wsdbm/", string.Empty)
            .Replace("<http://purl.org/stuff/rev#", string.Empty)
            .Replace(">\t", "\t")
            .Replace("> .", string.Empty);
        return sanitizedLine.Split('\t');
    }
}