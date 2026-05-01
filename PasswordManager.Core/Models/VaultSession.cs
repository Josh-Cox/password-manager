using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace PasswordManager.Core.Models
{
    public class VaultSession
    {
        public ObservableCollection<PasswordEntry> Entries { get; set; } = new();
        public byte[] Salt { get; set; } = Array.Empty<byte>();
        public string MasterPassword { get; set; } = "";
    }
}
