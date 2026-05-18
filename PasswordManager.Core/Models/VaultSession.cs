using System.Collections.ObjectModel;

namespace PasswordManager.Core.Models
{
    public class VaultSession
    {
        public ObservableCollection<PasswordEntry> Entries { get; set; } = new();
        public byte[] Salt { get; set; } = Array.Empty<byte>();
        public string MasterPassword { get; set; } = "";
        // Derived from MasterPassword + Salt; cached to avoid re-running PBKDF2 on every save
        internal byte[]? CachedKey { get; set; }
    }
}
