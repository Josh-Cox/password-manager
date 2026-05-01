using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Core.Commands
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
