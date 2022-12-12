using System.Text.RegularExpressions;

namespace ShutUpHusky.Files;

internal static class CamelCaseToKebabCaseFunctions {
    public static string CamelCaseToKebabCase(this string fileName) =>
        Regex.Replace(fileName, "(?<=[a-z])([A-Z])", "-$1", RegexOptions.Compiled).ToLowerInvariant();
}
