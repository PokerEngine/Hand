using Domain.Service.Evaluator;
using Domain.ValueObject;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Infrastructure.Service.Evaluator;

public class PokerStoveEvaluator(
    IOptions<PokerStoveEvaluatorOptions> options,
    ILogger<PokerStoveEvaluator> logger
) : IEvaluator
{
    private static readonly Dictionary<Game, string> GameMapping = new()
    {
        { Game.NoLimitHoldem, "h" },
        { Game.PotLimitOmaha, "O" },
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

        logger.LogDebug("Call {Path} with {arguments}", options.Value.Path, arguments);

        return new Process
        {
            StartInfo =
            {
                FileName = options.Value.Path,
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
            logger.LogDebug("Got output {Output}", output);
        }

        if (error != "")
        {
            logger.LogError("Got error {Error}", error);
        }

        return (output, error);
    }

    private string GetGameRepresentation(Game game)
    {
        if (!GameMapping.TryGetValue(game, out var gameType))
        {
            throw new ArgumentException("Game is not supported", nameof(game));
        }

        return gameType;
    }

    private Combo ParseResponse(string output, string error)
    {
        if (error != "")
        {
            throw new FormatException($"Error is caught: {error}");
        }

        var parts = output.Split(':').Select(x => x.Trim()).ToArray();
        if (parts.Length != 3)
        {
            throw new FormatException($"Invalid response: {output}");
        }

        if (!ComboTypeMapping.TryGetValue(parts[0], out var comboType))
        {
            throw new FormatException($"Invalid combo: {output}");
        }

        if (!Int32.TryParse(parts[2], out var comboWeight))
        {
            throw new FormatException($"Invalid weight: {output}");
        }

        return new Combo(type: comboType, weight: comboWeight);
    }
}

public class PokerStoveEvaluatorOptions
{
    public const string SectionName = "PokerStove";

    public required string Path { get; init; }
}
