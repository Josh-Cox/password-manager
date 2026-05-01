using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Core.Services
{
    public static class PasswordService
    {
        public static string Generate(int length)
        {
            return PasswordGenerator.Generate(length);
        }
    }
}
