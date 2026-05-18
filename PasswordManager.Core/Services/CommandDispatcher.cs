using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PasswordManager.Core.Com.Commands;
using PasswordManager.Core.Com.Handlers;
using PasswordManager.Core.Com.Interfaces;
using PasswordManager.Core.Com.Queries;

namespace PasswordManager.Core.Services
{
    public class CommandDispatcher
    {
        // command and query dicts
        private readonly Dictionary<Type, Func<ICommand, Task>> _commandHandlers;
        private readonly Dictionary<Type, Func<object, Task<object>>> _queryHandlers;

        public CommandDispatcher(
            AddEntryHandler addHandler,
            DeleteEntryHandler deleteHandler,
            LoadVaultHandler loadVaultHandler,
            CreateVaultHandler createVaultHandler,
            GeneratePasswordHandler generateHandler,
            GetEntriesHandler getEntriesHandler,
            VaultExistsHandler vaultExistsHandler)
        {
            _commandHandlers = new()
            {
                {
                    typeof(AddEntryCommand),
                    command => addHandler.HandleAsync((AddEntryCommand)command)
                },
                {
                    typeof(DeleteEntryCommand),
                    command => deleteHandler.HandleAsync((DeleteEntryCommand)command)
                },
                {
                    typeof(LoadVaultCommand),
                    command => loadVaultHandler.HandleAsync((LoadVaultCommand)command)
                },
                {
                    typeof(CreateVaultCommand),
                    command => createVaultHandler.HandleAsync((CreateVaultCommand)command)
                },

            };

            _queryHandlers = new()
            {
                {
                    typeof(GeneratePasswordQuery),
                    async query =>
                    {
                        var result = await generateHandler.HandleAsync((GeneratePasswordQuery)query);
                        return (object)result;
                    }
                },
                {
                    typeof(GetEntriesQuery),
                    async query =>
                    {
                        var result = await getEntriesHandler.HandleAsync((GetEntriesQuery)query);
                        return (object)result;
                    }
                },
                {
                    typeof(VaultExistsQuery),
                    async query =>
                    {
                        var result = await vaultExistsHandler.HandleAsync((VaultExistsQuery)query);
                        return (object)result;
                    }
                }
            };
        }

        // COMMANDS (no return value)
        public async Task DispatchAsync(ICommand command)
        {
            var type = command.GetType();

            if (_commandHandlers.TryGetValue(type, out var handler))
            {
                await handler(command);
                return;
            }

            throw new NotSupportedException($"No handler for {type.Name}");
        }

        // QUERIES (return value)
        public async Task<TResult> DispatchAsync<TResult>(IQuery<TResult> query)
        {
            var type = query.GetType();

            if (_queryHandlers.TryGetValue(type, out var handler))
            {
                var result = await handler(query);
                return (TResult)result;
            }

            throw new NotSupportedException($"No handler for {type.Name}");
        }
    }
}