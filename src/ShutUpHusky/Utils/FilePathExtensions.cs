namespace ShutUpHusky.Utils;

public static class FilePathExtensions {
    public static string GetFileName(this string filePath) {
        var fullFileName = filePath.Split("/").Last();

        if (fullFileName.StartsWith("."))
            return fullFileName;

        return fullFileName.Split(".").First();
    }
}
