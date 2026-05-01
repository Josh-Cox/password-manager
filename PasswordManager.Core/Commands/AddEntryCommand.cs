using System;
using System.Collections.Generic;
using System.Text;
using PasswordManager.Core.Models;

namespace PasswordManager.Core.Commands
{
    public class AddEntryCommand : ICommand
    {
        public PasswordEntry Entry { get; }

        public AddEntryCommand(PasswordEntry entry)
        {
            Entry = entry;
        }
    }
}
