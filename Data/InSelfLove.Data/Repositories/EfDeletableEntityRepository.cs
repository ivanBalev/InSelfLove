namespace InSelfLove.Data.Repositories
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using InSelfLove.Data.Common.Models;
    using InSelfLove.Data.Common.Repositories;
    using Microsoft.EntityFrameworkCore;

    public class EfDeletableEntityRepository<TEntity> : EfRepository<TEntity>, IDeletableEntityRepository<TEntity>
        where TEntity : class, IDeletableEntity
    {
        public EfDeletableEntityRepository(MySqlDbContext context)
            : base(context)
        {
        }

        public override IQueryable<TEntity> All() => base.All().Where(x => !x.IsDeleted);

        public override IQueryable<TEntity> AllAsNoTracking() => base.AllAsNoTracking().Where(x => !x.IsDeleted);

        public IQueryable<TEntity> AllWithDeleted() => base.All().IgnoreQueryFilters();

        public IQueryable<TEntity> AllAsNoTrackingWithDeleted() => base.AllAsNoTracking().IgnoreQueryFilters();

        // Not used and removed in later templage versions but still cool
        public Task<TEntity> GetByIdWithDeletedAsync(params object[] id)
        {
            // Get expression that compares then given entity type's primary key field(s) to given id values
            // (primary key can be composite i.e. consisting of more than one field in the entity)
            // Dynamically finds the primary key (no way for us to know what it is at compile time)
            // and creates an expression comparing each of its fields to the supplied id entries
            var getByIdPredicate = EfExpressionHelper.BuildByIdPredicate<TEntity>(this.Context, id);

            // Execute expression (x => x.id == id) for each primary key field
            return this.AllWithDeleted().FirstOrDefaultAsync(getByIdPredicate);
        }

        public void HardDelete(TEntity entity) => base.Delete(entity);

        public void Undelete(TEntity entity)
        {
            entity.IsDeleted = false;
            entity.DeletedOn = null;
            this.Update(entity);
        }

        public override void Delete(TEntity entity)
        {
            entity.IsDeleted = true;
            entity.DeletedOn = DateTime.UtcNow;
            this.Update(entity);
        }
    }
}
