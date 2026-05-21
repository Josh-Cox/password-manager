using PasswordManager.Core.Com.Commands;
using PasswordManager.Core.Com.Interfaces;
using PasswordManager.Core.Services;

namespace PasswordManager.Core.Com.Handlers
{
    public class ClearSessionHandler : ICommandHandler<ClearSessionCommand>
    {
        private readonly VaultApplication _app;

        public ClearSessionHandler(VaultApplication app)
        {
            _app = app;
        }

        public Task HandleAsync(ClearSessionCommand command)
        {
            _app.ClearSession();
            return Task.CompletedTask;
        }
    }
}
