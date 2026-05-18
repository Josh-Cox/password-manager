namespace PasswordManager.Core.Services
{
    public class VaultNotFoundException : Exception
    {
        public VaultNotFoundException()
            : base("No vault found for this user.") { }
    }
}
