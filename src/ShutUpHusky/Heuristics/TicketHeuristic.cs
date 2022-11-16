using System.Text.RegularExpressions;
using LibGit2Sharp;

namespace ShutUpHusky.Heuristics;
 
public class TicketHeuristic : IHeuristic {
    public HeuristicResult Analyse(IRepository repo) {
        var ticketNumberMatch = Regex.Match(repo.Head.FriendlyName, "[a-zA-Z]+\\-[0-9]+");

        if (ticketNumberMatch.Success)
            return new() {
                Priority = 10,
                Value = $"feat({ticketNumberMatch.Value.ToLowerInvariant()})",
            };

        return new() {
            Priority = 10,
            Value = $"feat(rand-{(Math.Floor(new Random().NextDouble() * 9999))})",
        };
    }
}
