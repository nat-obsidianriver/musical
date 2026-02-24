namespace Musical.Web.Infrastructure;

/// <summary>
/// Delegating handler that reads the JWT from session and attaches it
/// as an Authorization: Bearer header on every outgoing API request.
/// </summary>
public class JwtTokenHandler(IHttpContextAccessor accessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = accessor.HttpContext?.Session.GetString("jwt");
        if (token is not null)
            request.Headers.Authorization = new("Bearer", token);

        return base.SendAsync(request, cancellationToken);
    }
}
