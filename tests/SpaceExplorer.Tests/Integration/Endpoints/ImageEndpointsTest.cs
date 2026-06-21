using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace SpaceExplorer.Tests.Integration.Endpoints;

public class ImageEndpointsTest : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ImageEndpointsTest(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> GetTokenAsync()
    {
        var registerPayload = new { name = "User Img", email = $"img{Guid.NewGuid()}@test.com", password = "Password123!" };
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerPayload);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        return body.GetProperty("token").GetString()!;
    }

    private async Task<(string CollectionId, string Token)> CreateCollectionAsync()
    {
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var createPayload = new { name = "Image Collection", description = "" };
        var createResponse = await _client.PostAsJsonAsync("/api/collections", createPayload);
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var collectionId = created.GetProperty("id").GetString()!;
        return (collectionId, token);
    }

    [Fact]
    public async Task SearchNasaImages_ShouldReturnResults()
    {
        var response = await _client.GetAsync("/api/images/search?query=mars&page=1");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("items").GetArrayLength().Should().Be(1);
        body.GetProperty("totalCount").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task AddItemToCollection_ShouldReturnCreated()
    {
        var (collectionId, token) = await CreateCollectionAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = new
        {
            nasaImageId = "nasa123",
            nasaImageUrl = "https://test.url/img.jpg",
            title = "Test Image",
            description = "A test image",
            earthDate = "2024-01-15"
        };

        var response = await _client.PostAsJsonAsync($"/api/collections/{collectionId}/items", payload);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("nasaImageId").GetString().Should().Be("nasa123");
        body.GetProperty("title").GetString().Should().Be("Test Image");
    }

    [Fact]
    public async Task GetCollectionItems_ShouldReturnItems()
    {
        var (collectionId, token) = await CreateCollectionAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = new
        {
            nasaImageId = "nasa456",
            nasaImageUrl = "https://test.url/img2.jpg",
            title = "Image 2",
            description = "Desc",
            earthDate = "2024-02-20"
        };
        await _client.PostAsJsonAsync($"/api/collections/{collectionId}/items", payload);

        var response = await _client.GetAsync($"/api/collections/{collectionId}/items");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task RemoveItemFromCollection_ShouldReturnNoContent()
    {
        var (collectionId, token) = await CreateCollectionAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = new
        {
            nasaImageId = "nasa789",
            nasaImageUrl = "https://test.url/img3.jpg",
            title = "To Remove",
            description = "",
            earthDate = (string?)null
        };
        var addResponse = await _client.PostAsJsonAsync($"/api/collections/{collectionId}/items", payload);
        var added = await addResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var itemId = added.GetProperty("id").GetString()!;

        var response = await _client.DeleteAsync($"/api/collections/{collectionId}/items/{itemId}");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task EnrichItem_ShouldReturnEnriched()
    {
        var (collectionId, token) = await CreateCollectionAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = new
        {
            nasaImageId = "nasa101",
            nasaImageUrl = "https://test.url/img4.jpg",
            title = "Enrich Me",
            description = "Desc",
            earthDate = (string?)null
        };
        var addResponse = await _client.PostAsJsonAsync($"/api/collections/{collectionId}/items", payload);
        var added = await addResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var itemId = added.GetProperty("id").GetString()!;

        var response = await _client.PostAsync($"/api/items/{itemId}/enrich", null);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("aiDescription").GetString().Should().Be("AI generated description");
    }

    [Fact]
    public async Task GetTimeline_ShouldReturnOk()
    {
        var (collectionId, token) = await CreateCollectionAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = new
        {
            nasaImageId = "nasa202",
            nasaImageUrl = "https://test.url/img5.jpg",
            title = "Timeline Item",
            description = "",
            earthDate = "2024-06-01"
        };
        await _client.PostAsJsonAsync($"/api/collections/{collectionId}/items", payload);

        var response = await _client.GetAsync("/api/timeline");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }
}
