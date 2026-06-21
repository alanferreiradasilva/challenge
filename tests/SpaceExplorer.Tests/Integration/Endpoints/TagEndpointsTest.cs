using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace SpaceExplorer.Tests.Integration.Endpoints;

public class TagEndpointsTest : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public TagEndpointsTest(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<(string ItemId, string Token)> CreateItemAsync()
    {
        var registerPayload = new { name = "User Tag", email = $"tag{Guid.NewGuid()}@test.com", password = "Password123!" };
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerPayload);
        var registerBody = await registerResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var token = registerBody.GetProperty("token").GetString()!;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createCollPayload = new { name = "Tag Collection", description = "" };
        var createCollResponse = await _client.PostAsJsonAsync("/api/collections", createCollPayload);
        var coll = await createCollResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var collectionId = coll.GetProperty("id").GetString()!;

        var addItemPayload = new
        {
            nasaImageId = "tag-nasa-1",
            nasaImageUrl = "https://test.url/tag.jpg",
            title = "Tag Test Image",
            description = "Test",
            earthDate = (string?)null
        };
        var addItemResponse = await _client.PostAsJsonAsync($"/api/collections/{collectionId}/items", addItemPayload);
        var item = await addItemResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var itemId = item.GetProperty("id").GetString()!;

        return (itemId, token);
    }

    [Fact]
    public async Task GetItemTags_WhenEmpty_ShouldReturnEmptyList()
    {
        var (itemId, token) = await CreateItemAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync($"/api/items/{itemId}/tags");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task AddTag_ShouldReturnCreated()
    {
        var (itemId, token) = await CreateItemAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = new { name = "mars" };
        var response = await _client.PostAsJsonAsync($"/api/items/{itemId}/tags", payload);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("name").GetString().Should().Be("mars");
        body.GetProperty("id").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AddTag_And_GetTags_ShouldReturnTag()
    {
        var (itemId, token) = await CreateItemAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = new { name = "space" };
        await _client.PostAsJsonAsync($"/api/items/{itemId}/tags", payload);

        var response = await _client.GetAsync($"/api/items/{itemId}/tags");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetArrayLength().Should().Be(1);
        body[0].GetProperty("name").GetString().Should().Be("space");
    }

    [Fact]
    public async Task RemoveTag_ShouldReturnNoContent()
    {
        var (itemId, token) = await CreateItemAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = new { name = "temp-tag" };
        var addResponse = await _client.PostAsJsonAsync($"/api/items/{itemId}/tags", payload);
        var added = await addResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var tagId = added.GetProperty("id").GetString()!;

        var response = await _client.DeleteAsync($"/api/items/{itemId}/tags/{tagId}");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task SuggestTags_ShouldReturnSuggestions()
    {
        var (itemId, token) = await CreateItemAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync($"/api/items/{itemId}/tags/suggestions");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("suggestions").GetArrayLength().Should().Be(5);
    }
}
