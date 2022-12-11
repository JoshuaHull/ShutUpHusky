namespace ShutUpHusky.Analysis.Statistics;

internal class TokenScoreboard {
    private readonly Dictionary<string, int> _tokenScores = new();
    private readonly List<OneLineOfTokens> _lines = new();
    private readonly TokenScoreboardOptions _options;

    public TokenScoreboard(TokenScoreboardOptions options) {
        _options = options;
    }

    public void AddAll(IEnumerable<OneLineOfTokens> lines) {
        foreach (var line in lines)
            Add(line);
    }

    public void Add(OneLineOfTokens line) {
        if (!line.Tokens.Any())
            return;

        if (_options.IgnoreLinesStartingWithToken.Contains(line.Tokens[0]))
            return;

        var usefulTokens = line
            .Tokens
            .Where(token => !_options.IgnoredTokens.Contains(token));

        if (!usefulTokens.Any())
            return;

        _lines.Add(line);

        foreach (var token in usefulTokens)
            if (_tokenScores.ContainsKey(token))
                _tokenScores[token] = _tokenScores[token] + 1;
            else
                _tokenScores.Add(token, 1);
    }

    public IEnumerable<OneLineOfTokens> HighestScoringLines() {
        if (!_tokenScores.Any())
            return Array.Empty<OneLineOfTokens>();

        var highestScore = _tokenScores
            .Select(kv => kv.Value)
            .OrderByDescending(_ => _)
            .First();

        var weightedFrequencies = _tokenScores
            .ToDictionary(ts => ts.Key, ts => ts.Value / (double)highestScore);

        return _lines
            .OrderByDescending(line =>
                line
                    .Tokens
                    .Sum(token => weightedFrequencies.TryGetValue(token, out var frequency) ? frequency : 0)
            );
    }
}
