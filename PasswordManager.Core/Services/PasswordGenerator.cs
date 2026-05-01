using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace PasswordManager.Core.Services
{
    public static class PasswordGenerator
    {
        private const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
        private const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Numbers = "0123456789";
        private const string Symbols = "!@#$%^&*()-_=+[]{};:,.<>?";

        public static string Generate(int length)
        {
            string allChars = Lowercase + Uppercase + Numbers + Symbols;

            StringBuilder password = new StringBuilder();

            byte[] randomBytes = new byte[length];

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            foreach (byte b in randomBytes)
            {
                int index = b % allChars.Length;
                password.Append(allChars[index]);
            }

            return password.ToString();
        }
    }
}
