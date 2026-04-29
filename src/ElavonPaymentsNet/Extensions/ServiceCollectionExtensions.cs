using ElavonPaymentsNet.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ElavonPaymentsNet.Extensions;

/// <summary>
/// Extension methods for registering <see cref="ElavonPaymentsClient"/> with the
/// .NET dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="ElavonPaymentsClient"/> as a singleton, using
    /// <c>IHttpClientFactory</c> for HTTP client lifecycle management.
    /// </summary>
    public static IServiceCollection AddElavonPayments(
        this IServiceCollection services,
        Action<ElavonPaymentsClientOptionsBuilder> configure)
    {
        var builder = new ElavonPaymentsClientOptionsBuilder();
        configure(builder);
        var builtOptions = builder.Build();

        services
            .AddHttpClient(nameof(ElavonPaymentsClient), client =>
            {
                client.BaseAddress = new Uri(builtOptions.ApiBaseUrl);
                client.Timeout = builtOptions.Timeout;
            })
            .AddHttpMessageHandler(() => new ElavonResilienceHandler(builtOptions.MaxRetryAttempts));

        services.AddSingleton(builtOptions);
        services.AddSingleton<ElavonPaymentsClient>(sp =>
        {
            var httpClient = sp.GetRequiredService<IHttpClientFactory>()
                .CreateClient(nameof(ElavonPaymentsClient));
            return new ElavonPaymentsClient(builtOptions, httpClient);
        });

        return services;
    }
}

/// <summary>
/// Mutable builder used only within <see cref="ServiceCollectionExtensions.AddElavonPayments"/>.
/// Keeps <see cref="ElavonPaymentsClientOptions"/> immutable at the public surface.
/// </summary>
public sealed class ElavonPaymentsClientOptionsBuilder
{
    public string IntegrationKey { get; set; } = string.Empty;
    public string IntegrationPassword { get; set; } = string.Empty;
    public ElavonEnvironment Environment { get; set; } = ElavonEnvironment.Sandbox;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public int MaxRetryAttempts { get; set; } = 3;

    internal ElavonPaymentsClientOptions Build() => new()
    {
        IntegrationKey = IntegrationKey,
        IntegrationPassword = IntegrationPassword,
        Environment = Environment,
        Timeout = Timeout,
        MaxRetryAttempts = MaxRetryAttempts
    };
}
