using System.Text.RegularExpressions;

namespace ShutUpHusky.Utils;

internal static class FileNameExtensions {
    public static string CamelCaseToKebabCase(this string fileName) =>
        Regex.Replace(fileName, "(?<=[a-z])([A-Z])", "-$1", RegexOptions.Compiled).ToLowerInvariant();
}
