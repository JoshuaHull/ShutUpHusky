using LibGit2Sharp;

namespace ShutUpHusky.Heuristics;

internal interface IHeuristic {
    ICollection<HeuristicResult> Analyse(IRepository repo);
}
