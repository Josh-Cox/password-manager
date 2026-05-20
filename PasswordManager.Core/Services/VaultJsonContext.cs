using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using PasswordManager.Core.Models;

namespace PasswordManager.Core.Services
{
    [JsonSerializable(typeof(ObservableCollection<PasswordEntry>))]
    internal partial class VaultJsonContext : JsonSerializerContext { }
}
