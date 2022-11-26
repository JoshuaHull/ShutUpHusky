namespace ShutUpHusky.Utils;

internal static class FilePathExtensions {
    public static string GetFileName(this string filePath) {
        var fullFileName = filePath.Split("/").Last();

        if (fullFileName.StartsWith("."))
            return fullFileName;

        return fullFileName.Split(".").FirstOrDefault() ?? string.Empty;
    }

    public static string GetFileExtension(this string filePath) {
        var fullFileName = filePath.Split("/").Last();

        if (fullFileName.StartsWith("."))
            fullFileName = fullFileName[1..];

        return fullFileName.Split(".").LastOrDefault() ?? string.Empty;
    }
}
