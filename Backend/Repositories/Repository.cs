using Microsoft.EntityFrameworkCore.Storage;

public interface IRepository<T>
    where T : class
{
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    void Remove(T entity);
    // Task<IDbContextTransaction> BeginTransactionAsync();
    // Task CommitTransactionAsync();
    // Task RollbackTransactionAsync();
    // Task SaveChangesAsync();
    Task<T?> GetAsync<TKey>(TKey key) where TKey : notnull;
    Task<T?> GetByNameAsync(string name);
}

public class Repository<T> : IRepository<T>
    where T : class
{
    protected readonly AppDbContext _context;

    public Repository(AppDbContext context)
    {
        _context = context;
    }

    // public async Task<IDbContextTransaction> BeginTransactionAsync()
    // {
    //     return await _context.Database.BeginTransactionAsync();
    // }

    // // public async Task CommitTransactionAsync()
    // // {
    // //     await _context.Database.CommitTransactionAsync();
    // // }

    // // public async Task RollbackTransactionAsync()
    // // {
    // //     await _context.Database.RollbackTransactionAsync();
    // // }

    public async Task AddAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync();
    }

    public void Remove(T entity)
    {
        _context.Set<T>().Remove(entity);
        _context.SaveChanges();
    }

    public async Task<T?> GetAsync<TKey>(TKey key)
        where TKey : notnull
    {
        return await _context.Set<T>().FindAsync(key);
    }

    public async Task<T?> GetByNameAsync(string name)
    {
        return await _context.Set<T>().FindAsync(name);
    }

    // public async Task SaveChangesAsync()
    // {
    //     await _context.SaveChangesAsync();
    // }
}
