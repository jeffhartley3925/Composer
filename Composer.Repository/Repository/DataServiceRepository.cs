using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Data.Services.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Browser;

namespace Composer.Repository
{
    public class DataServiceRepository<T> : IDataServiceRepository<T>
    {
        private bool _dirty;
        public bool Dirty
        {
            get { return _dirty; }
            set
            {
                _dirty = value;
                var htmlDoc = HtmlPage.Document;
                var htmlEl = htmlDoc.GetElementById("plugin");
                if (htmlEl != null)
                {
                    HtmlPage.Window.Invoke("setDirty", _dirty.ToString().ToLower());
                }
            }
        }

        public DataServiceContext Context;

        public DataServiceRepository()
            : this(new DataServiceContext(new Uri(DataServiceUri.GetUri())))
        {
        }

        public DataServiceRepository(DataServiceContext context)
        {
            Context = context;
            Dirty = false;
        }

        private static string ResolveEntitySet(Type type)
        {
            var entitySetAttribute = (EntitySetAttribute)type.GetCustomAttributes(typeof(EntitySetAttribute), true).FirstOrDefault();

            if (entitySetAttribute != null)
                return entitySetAttribute.EntitySet;

            return null;
        }

        public U Create<U>() where U : new()
        {
            var collection = new DataServiceCollection<U>(Context);
            var entity = new U();
            collection.Add(entity);
            Dirty = true;
            return entity;
        }

        public void Update<U>(U entity)
        {
            Dirty = true;
            Context.UpdateObject(entity);
        }

        public void Delete<U>(U entity)
        {
            Uri uri;
            if (Context.TryGetUri(entity, out uri))
            {
                Dirty = true;
                Context.DeleteObject(entity);
            }
        }

        public void Attach<U>(U entity)
        {
            var collection = new DataServiceCollection<U>(Context);
            collection.Load(entity);
        }

        public IQueryable<U> Retrieve<U>(Expression<Func<U, bool>> predicate, params Expression<Func<U, object>>[] eagerProperties)
        {
            var entitySet = ResolveEntitySet(typeof(U));
            var query = Context.CreateQuery<U>(entitySet);
            query = eagerProperties.Aggregate(query, (current, e) => current.Expand(e));
            return query.Where(predicate);
        }

        public IQueryable<U> Retrieve<U>(Expression<Func<U, bool>> predicate)
        {
            var entitySet = ResolveEntitySet(typeof(U));
            var query = Context.CreateQuery<U>(entitySet);
            return query.Where(predicate);
        }

        public IQueryable<U> RetrieveAll<U>(params Expression<Func<U, object>>[] eagerProperties)
        {
            var entitySet = ResolveEntitySet(typeof(U));
            var query = Context.CreateQuery<U>(entitySet);

            return eagerProperties.Aggregate(query, (current, e) => current.Expand(e));
        }

        public object CallbackObject { get; set; }

        public void SaveChanges()
        {
        }

        public void SaveChanges(object obj)
        {
            CallbackObject = obj;
            Context.BeginSaveChanges(SaveChangesOptions.ContinueOnError, OnChangesSaved, Context);
        }

        public void DetectChanges()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> ManagedObjects
        {
            get { throw new NotImplementedException(); }
        }

        private void OnChangesSaved(IAsyncResult result)
        {
            System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                Context = result.AsyncState as DataServiceContext;
                try
                {
                    var dataServiceContext = Context;
                    if (dataServiceContext != null) dataServiceContext.EndSaveChanges(result);
                    Dirty = false;
                    Resume();
                }
                catch (Exception ex)
                {
                    throw new Exception("", ex.InnerException);
                }
            });
        }

        private void Resume()
        {
            //TODO: must to be a better way. what is it?
            var type = CallbackObject.GetType();
            var castMethod = GetType().GetMethod("Cast").MakeGenericMethod(type);
            var callbackObject = castMethod.Invoke(null, new[] { CallbackObject });
            var callbackMethod = callbackObject.GetType().GetMethod("OnSaveChangesComplete");
            if (callbackMethod != null)
            {
                callbackMethod.Invoke(callbackObject, null);
            }
        }

        public static U Cast<U>(object obj)
        {
            return (U)obj;
        }
    }
}