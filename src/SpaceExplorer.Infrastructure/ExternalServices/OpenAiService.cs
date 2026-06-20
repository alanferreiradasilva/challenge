using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SpaceExplorer.Application.ExternalServices;
using SpaceExplorer.Application.Features.Images.Dtos;

namespace SpaceExplorer.Infrastructure.ExternalServices;

public class OpenAiService(HttpClient httpClient, IConfiguration configuration) : IAiService
{
    private const string ApiUrl = "https://api.openai.com/v1/chat/completions";
    private const string Model = "gpt-4o-mini";

    public async Task<EnrichItemResponse> EnrichImageAsync(EnrichItemRequest request, CancellationToken ct = default)
    {
        var prompt = $$"""
            You are a space science educator. Given this NASA image:
            Title: {{request.Title}}
            Description: {{request.Description ?? "No description available."}}

            Provide:
            1. An engaging 2-3 sentence description for a general audience.
            2. Three interesting curiosities as a JSON array of strings.

            Respond with JSON: { "description": "...", "curiosities": ["...", "...", "..."] }
            """;

        var result = await CallOpenAiAsync(prompt, ct);

        try
        {
            var parsed = JsonSerializer.Deserialize<JsonElement>(result);
            var description = parsed.GetProperty("description").GetString() ?? result;
            var curiosities = parsed.GetProperty("curiosities")
                .EnumerateArray()
                .Select(c => c.GetString() ?? "")
                .ToList();
            return new EnrichItemResponse(description, curiosities);
        }
        catch
        {
            return new EnrichItemResponse(result, []);
        }
    }

    public async Task<IEnumerable<string>> SuggestTagsAsync(string title, string? description, CancellationToken ct = default)
    {
        var prompt = $$"""
            Suggest 5 concise tags (single words or short phrases) for this NASA image:
            Title: {{title}}
            Description: {{description ?? "No description."}}
            
            Respond with a JSON array of strings only: ["tag1", "tag2", "tag3", "tag4", "tag5"]
            """;

        var result = await CallOpenAiAsync(prompt, ct);

        try
        {
            return JsonSerializer.Deserialize<List<string>>(result) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private async Task<string> CallOpenAiAsync(string prompt, CancellationToken ct)
    {
        var apiKey = configuration["OpenAi:ApiKey"]
            ?? throw new InvalidOperationException("OpenAi:ApiKey is not configured.");

        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        var body = new
        {
            model = Model,
            messages = new[] { new { role = "user", content = prompt } },
            temperature = 0.7
        };

        var response = await httpClient.PostAsJsonAsync(ApiUrl, body, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        return json.GetProperty("choices")[0]
                   .GetProperty("message")
                   .GetProperty("content")
                   .GetString() ?? string.Empty;
    }
}
