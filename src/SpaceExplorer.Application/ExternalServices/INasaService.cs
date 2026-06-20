using SpaceExplorer.Application.Common.Pagination;
using SpaceExplorer.Application.Features.Images.Dtos;

namespace SpaceExplorer.Application.ExternalServices;

public interface INasaService
{
    Task<PagedResult<NasaImageDto>> SearchAsync(NasaSearchRequest request, CancellationToken ct = default);
}
