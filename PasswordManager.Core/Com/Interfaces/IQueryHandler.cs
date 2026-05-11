using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager.Core.Com.Interfaces
{
    public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
    {
        Task<TResult> HandleAsync(TQuery query);
    }
}
