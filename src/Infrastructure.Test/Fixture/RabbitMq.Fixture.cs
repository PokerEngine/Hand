using Infrastructure.IntegrationEvent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Test.Fixture;

public class RabbitMqFixture : IDisposable
{
    public IOptions<RabbitMqIntegrationEventBusOptions> Options { get; }

    public RabbitMqFixture()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Test.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var opt = configuration.GetSection(RabbitMqIntegrationEventBusOptions.SectionName).Get<RabbitMqIntegrationEventBusOptions>();
        Options = Microsoft.Extensions.Options.Options.Create(opt!);

        using var httpClient = new HttpClient
        {
            BaseAddress = GetApiUri()
        };
        httpClient.DefaultRequestHeaders.Authorization = GetAuthHeader();

        var response = httpClient
            .PutAsync($"/api/vhosts/{Uri.EscapeDataString(Options.Value.VirtualHost)}",
                new StringContent("", Encoding.UTF8, "application/json"))
            .GetAwaiter().GetResult();

        response.EnsureSuccessStatusCode();

        var permissions = new
        {
            configure = ".*",
            write = ".*",
            read = ".*"
        };

        var permJson = new StringContent(
            JsonSerializer.Serialize(permissions),
            Encoding.UTF8,
            "application/json"
        );

        var permResponse = httpClient
            .PutAsync(
                $"/api/permissions/{Uri.EscapeDataString(Options.Value.VirtualHost)}/{Options.Value.Username}",
                permJson
            )
            .GetAwaiter().GetResult();

        permResponse.EnsureSuccessStatusCode();
    }

    public void Dispose()
    {
        using var httpClient = new HttpClient
        {
            BaseAddress = GetApiUri()
        };
        httpClient.DefaultRequestHeaders.Authorization = GetAuthHeader();

        var response = httpClient.DeleteAsync($"/api/vhosts/{Uri.EscapeDataString(Options.Value.VirtualHost)}").GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
    }

    private Uri GetApiUri()
    {
        return new Uri($"http://{Options.Value.Host}:15672"); // Management port differs
    }

    private AuthenticationHeaderValue GetAuthHeader()
    {
        var token = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{Options.Value.Username}:{Options.Value.Password}"));
        return new AuthenticationHeaderValue("Basic", token);
    }
}
