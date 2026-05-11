using PasswordManager.Core.Com.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Core.Com.Queries
{
    public class GeneratePasswordQuery : IQuery<string>
    {
        public int Length { get; }

        public GeneratePasswordQuery(int length)
        {
            Length = length;
        }
    }
}
