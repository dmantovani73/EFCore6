// https://www.thereformedprogrammer.net/ef-core-in-depth-soft-deleting-data-with-global-query-filters/

using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;
using System.Reflection;

internal static class SoftDeleteQueryExtension
{
    public static void AddSoftDeleteQueryFilter(this IMutableEntityType entityData)
    {
        var methodToCall = typeof(SoftDeleteQueryExtension)
            .GetMethod(nameof(GetSoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)?
            .MakeGenericMethod(entityData.ClrType);

        if (methodToCall == null) return;

        var filter = methodToCall.Invoke(null, new object[] { });
        if (filter == null) return;

        entityData.SetQueryFilter((LambdaExpression)filter);
        var property = entityData.FindProperty(nameof(ISoftDelete.IsDeleted));
        if (property == null) return;

        entityData.AddIndex(property);
    }

    private static LambdaExpression GetSoftDeleteFilter<TEntity>()
        where TEntity : class, ISoftDelete
    {
        Expression<Func<TEntity, bool>> filter = x => !x.IsDeleted;
        
        return filter;
    }
}