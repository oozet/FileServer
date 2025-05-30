using Microsoft.EntityFrameworkCore.Storage;

public interface IRepository<T>
    where T : class
{
    Task AddAsync(T entity);
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    Task SaveChangesAsync();
    Task<T?> GetAsync<TKey>(TKey key)
        where TKey : notnull;
}

public class Repository<T> : IRepository<T>
    where T : class
{
    protected readonly AppDbContext _context;

    public Repository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        await _context.Database.CommitTransactionAsync();
    }

    public async Task RollbackTransactionAsync()
    {
        await _context.Database.RollbackTransactionAsync();
    }

    public async Task AddAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
    }

    public void Update(T entity)
    {
        _context.Set<T>().Update(entity);
    }

    public async Task<T?> GetAsync<TKey>(TKey key)
        where TKey : notnull
    {
        return await _context.Set<T>().FindAsync(key);
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public async Task<T?> GetByIdAsync(string id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public async Task<T?> GetByNameAsync(string name)
    {
        return await _context.Set<T>().FindAsync(name);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
