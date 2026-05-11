using PasswordManager.Core.Com.Interfaces;
using PasswordManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace PasswordManager.Core.Com.Queries
{
    public class GetEntriesQuery : IQuery<ObservableCollection<PasswordEntry>>
    {
        public GetEntriesQuery()
        {
        }
    }
}
