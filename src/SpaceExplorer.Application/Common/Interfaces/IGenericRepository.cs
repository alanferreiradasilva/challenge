using SpaceExplorer.Application.Common.Pagination;

namespace SpaceExplorer.Application.Common.Interfaces;

public interface IGenericRepository<TDto, TKey>
{
    Task<TDto?> GetByIdAsync(TKey id, CancellationToken ct = default);
    Task<PagedResult<TDto>> GetAllAsync(PagedRequest request, CancellationToken ct = default);
    Task<TDto> CreateAsync(TDto dto, CancellationToken ct = default);
    Task<TDto> UpdateAsync(TDto dto, CancellationToken ct = default);
    Task DeleteAsync(TKey id, CancellationToken ct = default);
}
