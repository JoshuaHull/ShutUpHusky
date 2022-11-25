using LibGit2Sharp;

namespace ShutUpHusky.Heuristics;

public interface IHeuristic {
    ICollection<HeuristicResult> Analyse(IRepository repo);
}
