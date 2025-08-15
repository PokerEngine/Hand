using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Test.Fixture;

public class RabbitMqFixture : IDisposable
{
    public IConfiguration Configuration { get; }
    private Uri _apiUrl;
    private string _authHeader;
    private string _virtualHost;

    public RabbitMqFixture()
    {
        Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Test.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var host = Configuration.GetValue<string>("RabbitMQ:Host") ??
                   throw new ArgumentException("RabbitMQ:Host is not configured");
        var username = Configuration.GetValue<string>("RabbitMQ:Username") ??
                       throw new ArgumentException("RabbitMQ:Username is not configured");
        var password = Configuration.GetValue<string>("RabbitMQ:Password") ??
                       throw new ArgumentException("RabbitMQ:Password is not configured");
        var virtualHost = Configuration.GetValue<string>("RabbitMQ:VirtualHost") ??
                          throw new ArgumentException("RabbitMQ:VirtualHost is not configured");

        _apiUrl = new Uri($"http://{host}:15672"); // Management port differs
        _authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
        _virtualHost = virtualHost;

        using var httpClient = new HttpClient
        {
            BaseAddress = _apiUrl
        };
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _authHeader);

        var response = httpClient
            .PutAsync($"/api/vhosts/{Uri.EscapeDataString(virtualHost)}",
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
                $"/api/permissions/{Uri.EscapeDataString(virtualHost)}/{username}",
                permJson
            )
            .GetAwaiter().GetResult();

        permResponse.EnsureSuccessStatusCode();
    }

    public void Dispose()
    {
        using var httpClient = new HttpClient
        {
            BaseAddress = _apiUrl
        };
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _authHeader);

        var response = httpClient.DeleteAsync($"/api/vhosts/{Uri.EscapeDataString(_virtualHost)}").GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
    }
}
