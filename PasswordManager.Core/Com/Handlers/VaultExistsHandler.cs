using PasswordManager.Core.Com.Interfaces;
using PasswordManager.Core.Com.Queries;
using PasswordManager.Core.Services;

namespace PasswordManager.Core.Com.Handlers
{
    public class VaultExistsHandler : IQueryHandler<VaultExistsQuery, bool>
    {
        private readonly VaultApplication _app;

        public VaultExistsHandler(VaultApplication app)
        {
            _app = app;
        }

        public Task<bool> HandleAsync(VaultExistsQuery query) =>
            _app.VaultExistsAsync(query.UserId);
    }
}
