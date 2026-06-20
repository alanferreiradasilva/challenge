using SpaceExplorer.Application.Common.Pagination;

namespace SpaceExplorer.Application.Common.Interfaces;

public interface IGenericService<TDto, TCreateDto, TUpdateDto, TKey>
{
    Task<TDto?> GetByIdAsync(TKey id, CancellationToken ct = default);
    Task<PagedResult<TDto>> GetAllAsync(PagedRequest request, CancellationToken ct = default);
    Task<TDto> CreateAsync(TCreateDto request, CancellationToken ct = default);
    Task<TDto> UpdateAsync(TKey id, TUpdateDto request, CancellationToken ct = default);
    Task DeleteAsync(TKey id, CancellationToken ct = default);
}
