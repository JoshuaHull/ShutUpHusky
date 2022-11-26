using System.Text.RegularExpressions;
using LibGit2Sharp;

namespace ShutUpHusky.Heuristics;

internal class TypeAndScopeHeuristic : IHeuristic {
    public ICollection<HeuristicResult> Analyse(IRepository repo) {
        var ticketNumberMatch = Regex.Match(repo.Head.FriendlyName, "[a-zA-Z]+\\-[0-9]+");

        if (ticketNumberMatch.Success)
            return new HeuristicResult[] {
                new() {
                    Priority = Constants.TypeAndScopePriority,
                    Value = $"feat({ticketNumberMatch.Value.ToLowerInvariant()})",
                    After = ": ",
                },
            };

        return new HeuristicResult[] {
            new() {
                Priority = Constants.TypeAndScopePriority,
                Value = $"feat",
                After = ": ",
            },
        };
    }
}
