using PasswordManager.Core.Com.Commands;
using PasswordManager.Core.Com.Interfaces;
using PasswordManager.Core.Services;

namespace PasswordManager.Core.Com.Handlers
{
    public class DeleteAccountHandler : ICommandHandler<DeleteAccountCommand>
    {
        private readonly IVaultStore _vaultStore;
        private readonly ApiAccountService _accountService;

        public DeleteAccountHandler(IVaultStore vaultStore, ApiAccountService accountService)
        {
            _vaultStore = vaultStore;
            _accountService = accountService;
        }

        public async Task HandleAsync(DeleteAccountCommand command)
        {
            await _vaultStore.DeleteAsync(command.UserId);
            await _accountService.DeleteAccountAsync(command.UserId);
        }
    }
}
