using Domain.Service.Evaluator;
using Infrastructure.Service.Evaluator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Infrastructure.Test.Service.Evaluator;

public class BaseEvaluatorTest
{
    protected readonly IEvaluator Evaluator = CreateEvaluator();

    private static IEvaluator CreateEvaluator()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var opt = configuration.GetSection(PokerStoveEvaluatorOptions.SectionName).Get<PokerStoveEvaluatorOptions>();
        var options = Microsoft.Extensions.Options.Options.Create(opt!);
        var logger = NullLogger<PokerStoveEvaluator>.Instance;
        return new PokerStoveEvaluator(options, logger);
    }
}
