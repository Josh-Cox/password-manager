using PasswordManager.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Core.Commands
{
    public class GeneratePasswordHandler : IQueryHandler<GeneratePasswordQuery, string>
    {
        private readonly VaultApplication _app;

        public GeneratePasswordHandler(VaultApplication app)
        {
            _app = app;
        }

        public Task<string> HandleAsync(GeneratePasswordQuery query)
        {
            var result = _app.GeneratePassword(query.Length);
            return Task.FromResult(result);
        }
    }
}
