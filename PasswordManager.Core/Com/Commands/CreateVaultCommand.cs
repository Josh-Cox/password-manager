using PasswordManager.Core.Com.Interfaces;

namespace PasswordManager.Core.Com.Commands
{
    public class CreateVaultCommand : ICommand
    {
        public string UserID { get; }
        public string MasterPassword { get; }

        public CreateVaultCommand(string userID, string masterPassword)
        {
            UserID = userID;
            MasterPassword = masterPassword;
        }
    }
}
