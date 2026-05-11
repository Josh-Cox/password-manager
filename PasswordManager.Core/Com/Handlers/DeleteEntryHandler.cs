using PasswordManager.Core.Com.Commands;
using PasswordManager.Core.Com.Interfaces;
using PasswordManager.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Core.Com.Handlers
{
    public class DeleteEntryHandler : ICommandHandler<DeleteEntryCommand>
    {
        private readonly VaultApplication _app;

        public DeleteEntryHandler(VaultApplication app)
        {
            _app = app;
        }

        public async Task HandleAsync(DeleteEntryCommand command)
        {
            await _app.DeleteEntryAsync(command.UserID, command.Entry);
        }
    }
}
