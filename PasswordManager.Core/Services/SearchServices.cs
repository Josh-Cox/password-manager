using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using PasswordManager.Core.Models;

namespace PasswordManager.Core.Services
{
    public class SearchServices
    {
        public List<PasswordEntry> Search(ObservableCollection<PasswordEntry> entries, string query)
        {
            query = query.ToLower();

            return entries
                .Where(e =>
                    (e.Site ?? "").ToLower().Contains(query) ||
                    (e.Username ?? "").ToLower().Contains(query))
                .ToList();
        }
    }
}
