using LibGit2Sharp;

namespace ShutUpHusky.Heuristics;

internal interface IFileHeuristic : IHeuristic {
    HeuristicResult Analyse(IRepository repo, StatusEntry statusEntry);
}
