namespace ShutUpHusky.Utils;

public static class ArrayExtensions {
    public static T[] AppendRange<T>(this T[] left, T[] right) {
        var rtn = new T[left.Length + right.Length];
        for (int i = 0; i < left.Length; i += 1)
            rtn[i] = left[i];
        for (int i = 0; i < right.Length; i += 1)
            rtn[i + left.Length] = right[i];
        return rtn;
    }
}
