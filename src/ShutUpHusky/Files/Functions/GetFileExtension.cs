using LibGit2Sharp;

namespace ShutUpHusky.Files;

internal static class GetFileExtensionFunctions {
    public static string GetFileExtension(this string filePath) {
        var ext = Path.GetExtension(filePath);

        return ext.Length switch {
            0 => string.Empty,
            _ => ext[1..],
        };
    }

    public static string GetFileExtension(this StatusEntry entry) =>
        entry.FilePath.GetFileExtension();
}
