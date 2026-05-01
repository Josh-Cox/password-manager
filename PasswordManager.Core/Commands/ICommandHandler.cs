using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Core.Commands
{
    public interface ICommandHandler<TCommand> where TCommand : ICommand
    {
        Task HandleAsync(TCommand command);
    }
}