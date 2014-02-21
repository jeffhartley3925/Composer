using System;
using System.Collections;
using System.Data.Services.Client;
using System.Linq.Expressions;
using System.Linq;

namespace Composer.Modules.Composition.Service
{
    public static class SetBasedExtensions
    {

        public static DataServiceQuery<T> IsIn<T>(this DataServiceQuery<T> query, IEnumerable Set, Expression<Func<T, Object>> propertyExpression)
        {

            //The Filter Predicate that contains the Filter criteria
            System.Linq.Expressions.Expression filterPredicate = null;
            //The parameter expression containing the Entity Type
            ParameterExpression param = propertyExpression.Parameters.Single();
            //Get Key Property 
            //The Left Hand Side of the Filter Expression
            System.Linq.Expressions.Expression left = propertyExpression.Body;
            //Build a Dynamic Linq Query for finding an entity whose ID is in the list
            foreach (var id in Set)
            {
                //Build a comparision expression which equats the Id of the ENtity with this value in the IDs list
                // ex : e.Id == 1
                System.Linq.Expressions.Expression comparison = System.Linq.Expressions.Expression.Equal(left, System.Linq.Expressions.Expression.Constant(id));
                //Add this to the complete Filter Expression
                // e.Id == 1 or e.Id == 3
                filterPredicate = (filterPredicate == null) ? comparison : System.Linq.Expressions.Expression.Or(filterPredicate, comparison);
            }

            //Convert the Filter Expression into a Lambda expression of type Func<Lists,bool>
            // which means that this lambda expression takes an instance of type EntityType and returns a Bool
            var filterLambdaExpression = System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(filterPredicate, param);
            return (DataServiceQuery<T>)query.Where<T>(filterLambdaExpression);

        }
    }
}
