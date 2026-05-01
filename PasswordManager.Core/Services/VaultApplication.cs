using PasswordManager.Core.Models;
using System.Collections.ObjectModel;

namespace PasswordManager.Core.Services
{
    public class VaultApplication
    {
        private readonly VaultService _vaultService;
        private VaultSession? _session;

        public VaultApplication(VaultService vaultService)
        {
            _vaultService = vaultService;
        }

        public async Task LoadVaultAsync(string masterPassword)
        {
            _session = await _vaultService.LoadAsync(masterPassword);
        }

        public ObservableCollection<PasswordEntry> GetEntries()
        {
            if (_session == null)
                throw new InvalidOperationException("Vault not loaded.");

            return _session.Entries;
        }

        public PasswordEntry GetEntry(int index)
        {
            if (_session == null)
                throw new InvalidOperationException("Vault not loaded.");

            return _session.Entries[index];
        }

        public async Task AddEntryAsync(PasswordEntry entry)
        {
            if (_session == null)
                throw new InvalidOperationException("Vault not loaded.");

            _session.Entries.Add(entry);
            await _vaultService.SaveAsync(_session);
        }

        public async Task DeleteEntryAsync(PasswordEntry entry)
        {
            if (_session == null)
                throw new InvalidOperationException("Vault not loaded.");

            _session.Entries.Remove(entry);
            await _vaultService.SaveAsync(_session);
        }

        public string GeneratePassword(int length)
        {
            return PasswordService.Generate(length);
        }
    }
}
