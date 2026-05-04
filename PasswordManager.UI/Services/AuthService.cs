using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Identity.Client;


namespace PasswordManager.UI.Services
{
    public class AuthService
    {
        private readonly IPublicClientApplication _app;
        private readonly string[] _scopes;

        public AuthService(string clientId, string tenantId, string scope)
        {
            var authority =
    $"https://{tenantId}.ciamlogin.com/{tenantId}/PasswordManagerSignIn";

            _app = PublicClientApplicationBuilder
                .Create(clientId)
                .WithAuthority(authority)
                .WithRedirectUri("http://localhost")
                .Build();

            _scopes = new[] { scope };
        }

        public async Task<AuthenticationResult> LoginAsync()
        {
            return await _app
                .AcquireTokenInteractive(_scopes)
                .ExecuteAsync();
        }
    }

}
