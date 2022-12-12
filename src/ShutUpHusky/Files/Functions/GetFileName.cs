using LibGit2Sharp;

namespace ShutUpHusky.Files;

internal static class GetFileNameFunctions {
    public static string GetFileName(this string filePath) {
        var fullFileName = Path.GetFileName(filePath);

        if (fullFileName.StartsWith(".") || !fullFileName.Contains("."))
            return fullFileName;

        var fileNameParts = fullFileName.Split(".");
        return string.Join("-", fileNameParts[..(fileNameParts.Length - 1)]);
    }

    public static string GetFileName(this StatusEntry entry) =>
        entry.FilePath.GetFileName();
}
