using Reservae.Models.Common;
using Reservae.Models.DTOs;

namespace Reservae.Models.Interfaces;

public interface IBaseRepository<T> where T : AuditableEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<PagedResponseDto<T>> GetPagedAsync(int page, int pageSize);
    Task<List<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}