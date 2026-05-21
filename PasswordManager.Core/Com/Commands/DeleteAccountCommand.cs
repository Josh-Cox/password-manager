using PasswordManager.Core.Com.Interfaces;

namespace PasswordManager.Core.Com.Commands
{
    public class DeleteAccountCommand : ICommand
    {
        public string UserId { get; }
        public DeleteAccountCommand(string userId) { UserId = userId; }
    }
}
