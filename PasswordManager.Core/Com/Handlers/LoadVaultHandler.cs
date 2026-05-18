using PasswordManager.Core.Com.Commands;
using PasswordManager.Core.Com.Interfaces;
using PasswordManager.Core.Services;

namespace PasswordManager.Core.Com.Handlers
{
    public class LoadVaultHandler : ICommandHandler<LoadVaultCommand>
    {
        private readonly VaultApplication _app;

        public LoadVaultHandler(VaultApplication app)
        {
            _app = app;
        }

        public async Task HandleAsync(LoadVaultCommand command)
        {
            var ok = await _app.LoadVaultAsync(command.UserID, command.MasterPassword);

            if (!ok)
                throw new InvalidMasterPasswordException();
        }
    }
}
