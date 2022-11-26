namespace ShutUpHusky.Utils;

internal static class PriorityExtensions {
    public static double ToPriority(this int step, int minPriority, int maxPriority, int steps) {
        if (steps <= 1)
            return maxPriority;

        return maxPriority - step * ((maxPriority - minPriority) / (steps - 1d));
    }
}
