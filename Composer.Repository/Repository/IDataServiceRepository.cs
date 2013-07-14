using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Composer.Repository.DataService;

namespace Composer.Repository
{
    public interface IDataServiceRepository<T>
    {
        U Create<U>() where U : new();
        void Update<U>(U entity);
        void Delete<U>(U entity);
        IQueryable<U> RetrieveAll<U>(params Expression<Func<U, object>>[] eagerProperties);
        IQueryable<U> Retrieve<U>(Expression<Func<U, bool>> predicate, params Expression<Func<U, object>>[] eagerProperties);
        IQueryable<U> Retrieve<U>(Expression<Func<U, bool>> predicate);
        void Attach<U>(U entity);
        void SaveChanges(object obj);
        void DetectChanges();
        IEnumerable<T> ManagedObjects { get; }

    }
}