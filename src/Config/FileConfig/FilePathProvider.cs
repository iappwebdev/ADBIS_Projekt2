namespace WatDiv.Config.FileConfig;

internal sealed class FilePathProvider
{
    public static DirectoryInfo TryGetSolutionDirectoryInfo()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory != null && !directory.GetFiles("*.sln").Any())
        {
            directory = directory.Parent;
        }

        return directory;
    }
}