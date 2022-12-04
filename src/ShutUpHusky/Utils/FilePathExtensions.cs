namespace ShutUpHusky.Utils;

internal static class FilePathExtensions {
    public static string GetFileName(this string filePath) {
        var fullFileName = Path.GetFileName(filePath);

        if (fullFileName.StartsWith("."))
            return fullFileName;

        return fullFileName.Split(".").FirstOrDefault() ?? string.Empty;
    }

    public static string GetFileExtension(this string filePath) {
        var ext = Path.GetExtension(filePath);

        return ext.Length switch {
            0 => string.Empty,
            _ => ext[1..],
        };
    }
}
