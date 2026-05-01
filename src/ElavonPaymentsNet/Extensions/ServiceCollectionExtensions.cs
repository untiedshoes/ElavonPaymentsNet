using ElavonPaymentsNet.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
            // Pipeline (outermost first): Logging → Authentication → Resilience
            .AddHttpMessageHandler(sp => new ElavonLoggingHandler(sp.GetService<ILoggerFactory>()))
            .AddHttpMessageHandler(() => new ElavonAuthenticationHandler(
                builtOptions.IntegrationKey, builtOptions.IntegrationPassword))
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
    /// <summary>The integration key (vendor name) issued by Elavon.</summary>
    public string IntegrationKey { get; set; } = string.Empty;
    /// <summary>The integration password issued by Elavon.</summary>
    public string IntegrationPassword { get; set; } = string.Empty;
    /// <summary>The target environment. Defaults to <see cref="ElavonEnvironment.Sandbox"/>.</summary>
    public ElavonEnvironment Environment { get; set; } = ElavonEnvironment.Sandbox;
    /// <summary>HTTP request timeout. Defaults to 30 seconds.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    /// <summary>Maximum number of retry attempts for eligible GET requests. Defaults to 3.</summary>
    public int MaxRetryAttempts { get; set; } = 3;

    internal ElavonPaymentsClientOptions Build()
    {
        if (string.IsNullOrWhiteSpace(IntegrationKey))
            throw new ArgumentException("IntegrationKey is required.", nameof(IntegrationKey));
        if (string.IsNullOrWhiteSpace(IntegrationPassword))
            throw new ArgumentException("IntegrationPassword is required.", nameof(IntegrationPassword));

        return new()
        {
            IntegrationKey = IntegrationKey,
            IntegrationPassword = IntegrationPassword,
            Environment = Environment,
            Timeout = Timeout,
            MaxRetryAttempts = MaxRetryAttempts
        };
    }
}
