using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Core.Com.Interfaces
{
    public interface ICommandHandler<TCommand> where TCommand : ICommand
    {
        Task HandleAsync(TCommand command);
    }
}