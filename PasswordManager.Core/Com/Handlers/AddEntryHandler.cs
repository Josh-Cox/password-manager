using PasswordManager.Core.Com.Commands;
using PasswordManager.Core.Com.Interfaces;
using PasswordManager.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Core.Com.Handlers
{
    public class AddEntryHandler : ICommandHandler<AddEntryCommand>
    {
        private readonly VaultApplication _app;

        public AddEntryHandler(VaultApplication app)
        {
            _app = app;
        }

        public async Task HandleAsync(AddEntryCommand command)
        {
            await _app.AddEntryAsync(command.UserID, command.Entry);
        }
    }
}
