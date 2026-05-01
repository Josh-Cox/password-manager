using System;
using System.Collections.Generic;
using System.Text;
using PasswordManager.Core.Models;

namespace PasswordManager.Core.Commands
{
    public class DeleteEntryCommand : ICommand
    {
        public PasswordEntry Entry { get; }

        public DeleteEntryCommand(PasswordEntry entry)
        {
            Entry = entry;
        }
    }
}
