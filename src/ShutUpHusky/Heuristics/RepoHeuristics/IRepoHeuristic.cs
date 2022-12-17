using LibGit2Sharp;

namespace ShutUpHusky.Heuristics.RepoHeuristics;

internal interface IRepoHeuristic {
    HeuristicResult? Analyse(IRepository repo);
}
