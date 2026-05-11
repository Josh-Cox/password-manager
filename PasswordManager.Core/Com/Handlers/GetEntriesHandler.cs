using PasswordManager.Core.Com.Interfaces;
using PasswordManager.Core.Com.Queries;
using PasswordManager.Core.Models;
using PasswordManager.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace PasswordManager.Core.Com.Handlers
{
    public class GetEntriesHandler : IQueryHandler<GetEntriesQuery, ObservableCollection<PasswordEntry>>
    {
        private readonly VaultApplication _app;

        public GetEntriesHandler(VaultApplication app)
        {
            _app = app;
        }

        public Task<ObservableCollection<PasswordEntry>> HandleAsync(GetEntriesQuery query)
        {
            var result = _app.GetEntries();
            return Task.FromResult(result);
        }
    }
}
