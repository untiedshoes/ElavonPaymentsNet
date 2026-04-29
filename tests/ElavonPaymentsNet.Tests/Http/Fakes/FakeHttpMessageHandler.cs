using System.Net.Http;

namespace ElavonPaymentsNet.Tests.Http.Fakes;

/// <summary>
/// A fake <see cref="HttpMessageHandler"/> that delegates response creation to a caller-supplied function.
/// Use this to test code that depends on <see cref="HttpClient"/> without hitting a real network.
/// </summary>
internal sealed class FakeHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond)
    : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return respond(request);
    }
}
