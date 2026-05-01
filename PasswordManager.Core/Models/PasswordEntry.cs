using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Core.Models
{
    public class PasswordEntry
    {
        public string Site { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
