namespace WatDiv.Sanitizing;

internal sealed class Sanitizer100k : ISanitizer
{
    public IReadOnlyList<string> GetSanitizedColumns(string line)
    {
        string sanitizedLine = line
            .Replace("wsdbm:", string.Empty)
            .Replace("rev:", string.Empty)
            .Replace(" .", string.Empty);
        return sanitizedLine.Split('\t');
    }
}