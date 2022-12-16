using LibGit2Sharp;

namespace ShutUpHusky.Heuristics.FileHeuristics;

internal interface IFileHeuristic : IHeuristic {
    HeuristicResult[] Analyse(IRepository repo);
}
