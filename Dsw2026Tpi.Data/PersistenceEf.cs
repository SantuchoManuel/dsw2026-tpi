using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Dsw2026Tpi.Data;

public class PersistenceEf: IPersistence
{
    private readonly Dsw2026TpiDbContext _context;

    public PersistenceEf(Dsw2026TpiDbContext context)
    {
        _context = context;
    }

    public async Task<T> Add<T>(T entity) where T : EntityBase
    {
        await _context.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    
    public async Task AddRange<T>(IEnumerable<T> entities) where T : EntityBase
    {
        await _context.AddRangeAsync(entities);
        await _context.SaveChangesAsync();
    }

    public async Task<T> Delete<T>(T entity) where T : EntityBase
    {
        var a = entity.Id;
        _context.Remove(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<T?> First<T>(Expression<Func<T, bool>> predicate, params string[] include) where T : EntityBase
    {
        return await Include(_context.Set<T>(), include).FirstOrDefaultAsync(predicate);
    }

    public async Task<IEnumerable<T>?> GetAll<T>(params string[] include) where T : EntityBase
    {
        return await Include(_context.Set<T>(), include).ToListAsync();
    }

    public async Task<T?> GetById<T>(Guid id, params string[] include) where T : EntityBase
    {
        return await Include(_context.Set<T>(), include).FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<T>?> GetFiltered<T>(Expression<Func<T, bool>> predicate, params string[] include) where T : EntityBase
    {
        return await Include(_context.Set<T>(), include).Where(predicate).ToListAsync();
    }

    public async Task<T> Update<T>(T entity) where T : EntityBase
    {
        entity.UpdateTimes();
        _context.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Pagination<T>> Paginate<T, TKey>(int pageSize, int pageIndex, Expression<Func<T, bool>> predicate, Expression<Func<T, TKey>> sortOrder, params string[] includes) where T : EntityBase
    {
        pageSize = Math.Abs(pageSize);
        pageIndex = Math.Abs(pageIndex) == 0 ? 0 : Math.Abs(pageIndex) - 1;

        var filtered = Include(_context.Set<T>(), includes)
                 .Where(predicate)
                 .OrderBy(sortOrder);

        var total = await filtered.CountAsync();

        
        async Task<Pagination<T>> GetPage(int skip, int take)
        {
            var data = await filtered.Skip(skip)
                    .Take(take)
                    .ToListAsync();

            return new Pagination<T>(pageSize, pageIndex, total, data);
        }
        
        //la pagina existe
        if (total > pageSize * pageIndex)
        {
            return await GetPage(pageIndex * pageSize, pageSize);
        }

        //solo hay una pagina
        if (total < pageSize)
        {
            return new Pagination<T>(pageSize, pageIndex, total, await filtered.ToListAsync());
        }

        var targetPageIndex = pageIndex - 1;

        while (true)
        {
            if (total > targetPageIndex * pageSize)
            {
                return await GetPage(targetPageIndex * pageSize, pageSize);
            }

            targetPageIndex--;

            if (targetPageIndex < 0) return new Pagination<T>(pageSize, 0, 0, []);
        }
    }

    private static IQueryable<T> Include<T>(IQueryable<T> query, string[] includes) where T : EntityBase
    {
        var includedQuery = query;

        foreach (var include in includes)
        {
            includedQuery = includedQuery.Include(include);
        }
        return includedQuery;
    }
}
