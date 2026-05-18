using PasswordManager.Core.Com.Commands;
using PasswordManager.Core.Com.Interfaces;
using PasswordManager.Core.Services;

namespace PasswordManager.Core.Com.Handlers
{
    public class CreateVaultHandler : ICommandHandler<CreateVaultCommand>
    {
        private const int MinMasterPasswordLength = 12;
        private const int MaxMasterPasswordLength = 128;

        private readonly VaultApplication _app;

        public CreateVaultHandler(VaultApplication app)
        {
            _app = app;
        }

        public async Task HandleAsync(CreateVaultCommand command)
        {
            Validate(command.MasterPassword);
            await _app.CreateVaultAsync(command.UserID, command.MasterPassword);
        }

        private static void Validate(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Master password cannot be empty.");

            if (password.Length < MinMasterPasswordLength)
                throw new ArgumentException(
                    $"Master password must be at least {MinMasterPasswordLength} characters.");

            if (password.Length > MaxMasterPasswordLength)
                throw new ArgumentException(
                    $"Master password cannot exceed {MaxMasterPasswordLength} characters.");
        }
    }
}
