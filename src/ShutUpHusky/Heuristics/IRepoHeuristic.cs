using LibGit2Sharp;

namespace ShutUpHusky.Heuristics;

internal interface IRepoHeuristic {
    HeuristicResult Analyse(IRepository repo);
}
