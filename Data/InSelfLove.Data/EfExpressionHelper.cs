namespace InSelfLove.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata;
    using Microsoft.EntityFrameworkCore.Storage;

    internal static class EfExpressionHelper
    {
        // Get value buffer's indexer method. new ValueBuffer()[0,1,2...]
        private static readonly MethodInfo ValueBufferGetValueMethod =
            typeof(ValueBuffer).GetRuntimeProperties().Single(p => p.GetIndexParameters().Any()).GetMethod;

        // EF.Property method returns an object's property by type & name
        // Example:
        // var blogs = context.Blogs.Where(b =>
        //             EF.Property<DateTime>(b, "LastUpdated") > DateTime.Now.AddDays(-5));
        private static readonly MethodInfo EfPropertyMethod =
            typeof(EF).GetTypeInfo().GetDeclaredMethod(nameof(EF.Property));

        public static Expression<Func<TEntity, bool>> BuildByIdPredicate<TEntity>(
            DbContext dbContext,
            object[] id)
            where TEntity : class
        {
            // Validate
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            // Get entity type
            var entityType = typeof(TEntity);

            // Create an expression parameter out of the entity type
            var entityTypeParameterExpression = Expression.Parameter(entityType, "e");

            // Find all key properties (entity primary key can be non-primitive)
            var entityTypeKeyProperties = dbContext.Model.FindEntityType(entityType).FindPrimaryKey().Properties;

            // Predicate is a statement that returns either true or false
            var predicate = BuildPredicate(entityTypeKeyProperties, new ValueBuffer(id), entityTypeParameterExpression);

            // Create lambda expression with our entity type as parameter & our predicate as body
            return Expression.Lambda<Func<TEntity, bool>>(predicate, entityTypeParameterExpression);
        }

        private static BinaryExpression BuildPredicate(
            IReadOnlyList<IProperty> keyProperties,
            ValueBuffer keyValues,
            ParameterExpression entityTypeParameterExpression)
        {
            var keyValuesConstantExpression = Expression.Constant(keyValues);

            BinaryExpression predicate = null;
            for (var i = 0; i < keyProperties.Count; i++)
            {
                // Current primary key
                var keyProperty = keyProperties[i];

                // Make EF.Property method generic with type <property.ClrType>
                MethodInfo propertyMethodForPropType = EfPropertyMethod.MakeGenericMethod(keyProperty.ClrType);

                ConstantExpression keyPropertyName = Expression.Constant(keyProperty.Name, typeof(string));

                // Create call expression using EF.Property method on our entity type
                // Evaluates to - Property(e, "Id")
                var leftExpression = Expression.Call(
                        propertyMethodForPropType,
                        entityTypeParameterExpression,
                        keyPropertyName);

                // On our keyValues buffer instance, call its Indexer[0, 1, 2...], with i
                // i.e. get current primary key's value
                var currentKeyPropertyValue =
                    Expression.Call(keyValuesConstantExpression, ValueBufferGetValueMethod, Expression.Constant(i));

                // Convert key value to key property type
                var rightExpression = Expression.Convert(currentKeyPropertyValue, keyProperty.ClrType);

                // Create equality comparison between the key property & key value provided by client
                var equalsExpression = Expression.Equal(leftExpression, rightExpression);

                // Build expression for each primary key property
                predicate = predicate == null ? equalsExpression : Expression.AndAlso(predicate, equalsExpression);
            }

            return predicate;
        }
    }
}
