using Microsoft.EntityFrameworkCore;
using Reservae.Data;
using Reservae.Models.Common;
using Reservae.Models.Interfaces;
using Reservae.Models.DTOs;
using Reservae.Repository.Extensions;
namespace Reservae.Repository;

public class BaseRepository<T>(ApplicationDbContext context) : IBaseRepository<T> where T : AuditableEntity
{
    protected readonly ApplicationDbContext Context = context;
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public virtual async Task<T?> GetByIdAsync(int id)
        => await DbSet.FirstOrDefaultAsync(e => e.Id == id);
    public async Task<PagedResponseDto<T>> GetPagedAsync(int page, int pageSize)
    => await DbSet.OrderBy(e => e.CreatedAt).ToPagedAsync(page, pageSize);

    public async Task<List<T>> GetAllAsync()
        => await DbSet.ToListAsync();

    public async Task<T> AddAsync(T entity)
    {
        await DbSet.AddAsync(entity);
        await Context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        DbSet.Update(entity);
        await Context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Registro com id {id} não encontrado.");
        DbSet.Remove(entity);
        await Context.SaveChangesAsync();
    }


}