using System.Linq.Expressions;

namespace TutorialProj.Repositories.Interfaces;

/// <summary>
/// A generic repository interface for common data access operations.
/// By keeping parameters like include, orderBy, and filter optional,
/// we allow the service layer to control query shaping without exposing IQueryable directly.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public interface IRepository<T> where T : class
{
    // Read Operations
    
    // FindAllAsync supports filtering, eager loading (include), ordering, and pagination.
    Task<IEnumerable<T>> FindAllAsync(
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IQueryable<T>>? include = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        int? skip = null,
        int? take = null,
        bool asNoTracking = true);

    Task<T?> FindByIdAsync(object id);

    Task<T?> FindOneAsync(
        Expression<Func<T, bool>> filter,
        Func<IQueryable<T>, IQueryable<T>>? include = null,
        bool asNoTracking = true);

    Task<bool> ExistsAsync(Expression<Func<T, bool>> filter);
    
    Task<int> CountAsync(Expression<Func<T, bool>>? filter = null);

    // Write Operations
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void Delete(T entity);
    Task SaveAsync();
}
