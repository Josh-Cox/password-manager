using System;
using System.Collections.Generic;
using System.Text;
using PasswordManager.Core.Models;

namespace PasswordManager.Core.Commands
{
    public class AddEntryCommand : ICommand
    {       
        public string UserID { get; }
        public PasswordEntry Entry { get; }

        public AddEntryCommand(string userID, PasswordEntry entry)
        {
            UserID = userID;
            Entry = entry;
        }
    }
}
