using PasswordManager.Core.Com.Interfaces;

namespace PasswordManager.Core.Com.Queries
{
    public class VaultExistsQuery : IQuery<bool>
    {
        public string UserId { get; }

        public VaultExistsQuery(string userId)
        {
            UserId = userId;
        }
    }
}
