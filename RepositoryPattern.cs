using System.Linq.Expressions;

internal interface IGenericRepository<TEntity, TId>
    where TEntity : class
{
    Task<IEnumerable<TEntity>> GetAll(Expression<Func<TEntity, bool>>? predicate = null);

    Task<TEntity?> GetById(TId id);

    Task Add(TEntity entity);

    Task Delete(TId id);
}

internal class GenericRepository<TEntity, TId> : IGenericRepository<TEntity, TId>
    where TEntity : class
{
    readonly DbSet<TEntity> _entities;

    public GenericRepository(DbContext context)
    {
        _entities = context.Set<TEntity>();
    }

    public virtual async Task Add(TEntity entity)
    {
        await _entities.AddAsync(entity);
    }

    public virtual async Task Delete(TId id)
    {
        var entity = await GetById(id);
        if (entity is not null) _entities.Remove(entity);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAll(Expression<Func<TEntity, bool>>? predicate = null)
    {
        return await (predicate == null ? _entities : _entities.Where(predicate)).ToListAsync();
    }

    public virtual async Task<TEntity?> GetById(TId id)
    {
        return await _entities.FindAsync(id);
    }
}

internal interface IUnitOfWork : IAsyncDisposable
{ }

internal abstract class BaseUnitOfWork : IUnitOfWork
{
    readonly DbContext _context;

    public BaseUnitOfWork(DbContext context)
    {
        _context = context;
    }

    public async ValueTask DisposeAsync()
    {
        await _context.SaveChangesAsync();
        await _context.DisposeAsync();
    }
}

internal class UniversityUnitOfWork : BaseUnitOfWork
{
    public UniversityUnitOfWork(DbContext context) 
        : base(context)
    {
        Students = new GenericRepository<Student, int>(context);
    }

    public GenericRepository<Student, int> Students { get; set; }
}
