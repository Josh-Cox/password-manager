using System;
using System.Collections.Generic;
using System.Text;
using PasswordManager.Core.Com.Interfaces;
using PasswordManager.Core.Models;

namespace PasswordManager.Core.Com.Commands
{
    public class LoadVaultCommand : ICommand
    {
        public string UserID { get; }
        public string MasterPassword { get; }

        public LoadVaultCommand(string userID, string masterPassword)
        {
            UserID = userID;
            MasterPassword = masterPassword;
        }
    }
}
