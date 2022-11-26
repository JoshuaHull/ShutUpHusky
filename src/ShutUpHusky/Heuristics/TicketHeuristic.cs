using System.Text.RegularExpressions;
using LibGit2Sharp;
using ShutUpHusky.Utils;

namespace ShutUpHusky.Heuristics;

internal class TicketHeuristic : IHeuristic {
    private readonly IRandomNumberGenerator _randomNumberGenerator;

    public TicketHeuristic(IRandomNumberGenerator randomNumberGenerator) {
        _randomNumberGenerator = randomNumberGenerator;
    }

    public ICollection<HeuristicResult> Analyse(IRepository repo) {
        var ticketNumberMatch = Regex.Match(repo.Head.FriendlyName, "[a-zA-Z]+\\-[0-9]+");

        if (ticketNumberMatch.Success)
            return new HeuristicResult[] {
                new() {
                    Priority = Constants.TicketPriority,
                    Value = $"feat({ticketNumberMatch.Value.ToLowerInvariant()})",
                    After = ": ",
                },
            };

        return new HeuristicResult[] {
            new() {
                Priority = Constants.TicketPriority,
                Value = $"feat(rand-{(Math.Floor(_randomNumberGenerator.Next() * 9999))})",
                After = ": ",
            },
        };
    }
}
