using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;

namespace PasswordManager.UI.Services;

public class AuthenticatedHttpMessageHandler : DelegatingHandler
{
    private readonly AuthService _auth;

    public AuthenticatedHttpMessageHandler(AuthService auth)
    {
        _auth = auth;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var accessToken = await _auth.GetAccessTokenAsync();

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);
        }

        System.Diagnostics.Debug.WriteLine(
            $"API request {request.Method} {request.RequestUri?.PathAndQuery}; bearer token attached: {!string.IsNullOrWhiteSpace(accessToken)}");

        WriteAccessTokenDiagnostics(accessToken);

        return await base.SendAsync(request, cancellationToken);
    }

    private static void WriteAccessTokenDiagnostics(string? accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            return;

        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);

            System.Diagnostics.Debug.WriteLine(
                "Access token claims: "
                + $"iss={jwt.Issuer}; "
                + $"aud={string.Join(",", jwt.Audiences)}; "
                + $"scp={jwt.Claims.FirstOrDefault(c => c.Type == "scp")?.Value}; "
                + $"oid={jwt.Claims.FirstOrDefault(c => c.Type == "oid")?.Value}; "
                + $"sub={jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unable to read access token diagnostics: {ex.Message}");
        }
    }
}
