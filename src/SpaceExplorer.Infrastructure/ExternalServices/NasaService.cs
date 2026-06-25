using System.Net.Http.Json;
using System.Text.Json;
using SpaceExplorer.Application.Common.Pagination;
using SpaceExplorer.Application.ExternalServices;
using SpaceExplorer.Application.Features.Images.Dtos;

namespace SpaceExplorer.Infrastructure.ExternalServices;

public class NasaService(HttpClient httpClient) : INasaService
{
    private const string BaseUrl = "https://images-api.nasa.gov";

    public async Task<PagedResult<NasaImageDto>> SearchAsync(NasaSearchRequest request, CancellationToken ct = default)
    {
        var searchTerms = new List<string>();

        if (!string.IsNullOrWhiteSpace(request.Query))
            searchTerms.Add(request.Query);
        if (!string.IsNullOrWhiteSpace(request.Rover))
            searchTerms.Add($"\"{Uri.EscapeDataString(request.Rover)}\" rover");
        if (!string.IsNullOrWhiteSpace(request.Camera))
            searchTerms.Add($"\"{Uri.EscapeDataString(request.Camera)}\" camera");
        if (!string.IsNullOrWhiteSpace(request.Mission))
            searchTerms.Add($"\"{Uri.EscapeDataString(request.Mission)}\" mission");

        var queryParams = new List<string>();

        if (searchTerms.Count > 0)
            queryParams.Add($"q={Uri.EscapeDataString(string.Join(" ", searchTerms))}");
        if (request.StartDate.HasValue)
            queryParams.Add($"year_start={request.StartDate.Value.Year}");
        if (request.EndDate.HasValue)
            queryParams.Add($"year_end={request.EndDate.Value.Year}");

        queryParams.Add($"media_type={request.MediaType ?? "image"}");
        queryParams.Add($"page={request.Page}");

        var url = $"{BaseUrl}/search?{string.Join("&", queryParams)}";
        var response = await httpClient.GetFromJsonAsync<JsonElement>(url, ct);

        var items = response
            .GetProperty("collection")
            .GetProperty("items")
            .EnumerateArray()
            .Select(item =>
            {
                var data = item.GetProperty("data")[0];
                var links = item.TryGetProperty("links", out var l) ? l.EnumerateArray().FirstOrDefault() : default;

                return new NasaImageDto(
                    NasaId: data.TryGetProperty("nasa_id", out var id) ? id.GetString() ?? "" : "",
                    Title: data.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "",
                    Description: data.TryGetProperty("description", out var d) ? d.GetString() : null,
                    ImageUrl: links.ValueKind != JsonValueKind.Undefined && links.TryGetProperty("href", out var href) ? href.GetString() ?? "" : "",
                    Photographer: data.TryGetProperty("photographer", out var p) ? p.GetString() : null,
                    Date: data.TryGetProperty("date_created", out var dc) && DateOnly.TryParse(dc.GetString(), out var date) ? date : null,
                    Location: data.TryGetProperty("location", out var loc) ? loc.GetString() : null
                );
            })
            .ToList();

        var totalHits = response
            .GetProperty("collection")
            .TryGetProperty("metadata", out var meta) &&
            meta.TryGetProperty("total_hits", out var total)
                ? total.GetInt32()
                : items.Count;

        return new PagedResult<NasaImageDto>(items, totalHits, request.Page, 100);
    }
}
