using Domain;
using Domain.Service.Evaluator;
using Domain.ValueObject;
using System.Diagnostics;

namespace Infrastructure.Service.Evaluator;

public class PokerStoveEvaluator : IEvaluator
{
    private readonly string _path;
    private readonly ILogger<PokerStoveEvaluator> _logger;

    private static readonly Dictionary<Game, string> GameMapping = new()
    {
        { Game.HoldemNoLimit6Max, "h" },
        { Game.HoldemNoLimit9Max, "h" },
        { Game.OmahaPotLimit6Max, "O" },
        { Game.OmahaPotLimit9Max, "O" },
    };
    private static readonly Dictionary<string, ComboType> ComboTypeMapping = new()
    {
        {"high card", ComboType.HighCard},
        {"one pair", ComboType.OnePair},
        {"two pair", ComboType.TwoPair},
        {"trips", ComboType.Trips},
        {"straight", ComboType.Straight},
        {"flush", ComboType.Flush},
        {"full house", ComboType.FullHouse},
        {"quads", ComboType.Quads},
        {"str8 flush", ComboType.StraightFlush},
    };

    public PokerStoveEvaluator(IConfiguration configuration, ILogger<PokerStoveEvaluator> logger)
    {
        _path = configuration.GetValue<string>("PokerStove:Path") ??
                throw new ArgumentException("PokerStove:Path is not configured", nameof(configuration));
        _logger = logger;
    }

    public Combo Evaluate(Game game, CardSet holeCards, CardSet boardCards)
    {
        var process = PrepareProcess(game, boardCards, holeCards);
        var (output, error) = RunProcess(process);
        return ParseResponse(output, error);
    }

    private Process PrepareProcess(Game game, CardSet holeCards, CardSet boardCards)
    {
        var arguments = $"--game {GetGameRepresentation(game)}";
        if (holeCards.Count > 0)
        {
            arguments += $" --hand {holeCards}";
        }
        if (boardCards.Count > 0)
        {
            arguments += $" --board {boardCards}";
        }

        _logger.LogDebug($"Call {_path} with {arguments}", _path, arguments);

        return new Process
        {
            StartInfo =
            {
                FileName = _path,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
    }

    private (string, string) RunProcess(Process process)
    {
        process.Start();

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();

        process.WaitForExit();

        if (output != "")
        {
            _logger.LogDebug($"Got output {output}", output);
        }

        if (error != "")
        {
            _logger.LogDebug($"Got error {error}", error);
        }

        return (output, error);
    }

    private string GetGameRepresentation(Game game)
    {
        if (!GameMapping.TryGetValue(game, out var gameType))
        {
            throw new NotPerformedError($"Game {game} is not supported");
        }

        return gameType;
    }

    private Combo ParseResponse(string output, string error)
    {
        if (error != "")
        {
            throw new NotPerformedError(error);
        }

        var parts = output.Split(':').Select(x => x.Trim()).ToArray();
        if (parts.Length != 3)
        {
            throw new NotPerformedError($"Invalid response: {output}");
        }

        if (!ComboTypeMapping.TryGetValue(parts[0], out var comboType))
        {
            throw new NotPerformedError($"Invalid combo: {output}");
        }

        if (!Int32.TryParse(parts[2], out var comboWeight))
        {
            throw new NotPerformedError($"Invalid weight: {output}");
        }

        return new Combo(type: comboType, weight: comboWeight);
    }
}
