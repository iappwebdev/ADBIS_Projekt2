namespace WatDiv.Sanitizing;

internal interface ISanitizer
{
    IReadOnlyList<string> GetSanitizedColumns(string line);
}