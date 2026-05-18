namespace PasswordManager.Core.Services
{
    public class InvalidMasterPasswordException : Exception
    {
        public InvalidMasterPasswordException()
            : base("The master password is incorrect.") { }
    }
}
