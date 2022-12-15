using LibGit2Sharp;

namespace ShutUpHusky.Heuristics.FileHeuristics;

internal interface IFileHeuristic : IHeuristic {
    ICollection<HeuristicResult> Analyse(IRepository repo);
}
