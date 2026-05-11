using PasswordManager.Core.Com.Commands;
using PasswordManager.Core.Com.Interfaces;
using PasswordManager.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

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
            await _app.LoadVaultAsync(command.UserID, command.MasterPassword);
        }
    }
}
