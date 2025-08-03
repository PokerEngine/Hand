using Domain;
using Domain.Service.Evaluator;
using Domain.ValueObject;
using System.Diagnostics;

namespace Infrastructure.Service.Evaluator;

internal static class PokerStoveClient
{
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

    public static Combo Evaluate(Game game, CardSet holeCards, CardSet boardCards)
    {
        var process = PrepareProcess(game, boardCards, holeCards);
        var (output, error) = RunProcess(process);
        return ParseResponse(output, error);
    }

    private static Process PrepareProcess(Game game, CardSet holeCards, CardSet boardCards)
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

        return new Process
        {
            StartInfo =
            {
                FileName = "/usr/local/lib/pokerstove/build/bin/ps-recognize",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
    }

    private static (string, string) RunProcess(Process process)
    {
        process.Start();

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();

        process.WaitForExit();

        return (output, error);
    }

    private static string GetGameRepresentation(Game game)
    {
        if (!GameMapping.TryGetValue(game, out var gameType))
        {
            throw new NotPerformedError($"Game {game} is not supported");
        }

        return gameType;
    }

    private static Combo ParseResponse(string output, string error)
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

public class PokerStoveEvaluator : IEvaluator
{
    public Combo Evaluate(Game game, CardSet holeCards, CardSet boardCards)
    {
        return PokerStoveClient.Evaluate(game, holeCards, boardCards);
    }
}
