using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace SpaceExplorer.Tests.Integration.Endpoints;

public class CollectionEndpointsTest : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CollectionEndpointsTest(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> GetTokenAsync()
    {
        var registerPayload = new { name = "User Coll", email = $"coll{Guid.NewGuid()}@test.com", password = "Password123!" };
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerPayload);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        return body.GetProperty("token").GetString()!;
    }

    [Fact]
    public async Task CreateCollection_ShouldReturnCreated()
    {
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = new { name = "Mars Photos", description = "My Mars collection" };
        var response = await _client.PostAsJsonAsync("/api/collections", payload);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("name").GetString().Should().Be("Mars Photos");
        body.GetProperty("description").GetString().Should().Be("My Mars collection");
        body.GetProperty("id").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetCollections_ShouldReturnOk()
    {
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/collections");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCollectionById_ShouldReturnCollection()
    {
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createPayload = new { name = "Specific Collection", description = "Specific" };
        var createResponse = await _client.PostAsJsonAsync("/api/collections", createPayload);
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var id = created.GetProperty("id").GetString()!;

        var response = await _client.GetAsync($"/api/collections/{id}");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("id").GetString().Should().Be(id);
        body.GetProperty("name").GetString().Should().Be("Specific Collection");
    }

    [Fact]
    public async Task GetCollectionById_WithNonExistentId_ShouldReturnNotFound()
    {
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync($"/api/collections/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCollection_ShouldReturnUpdated()
    {
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createPayload = new { name = "Original Name", description = "Original" };
        var createResponse = await _client.PostAsJsonAsync("/api/collections", createPayload);
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var id = created.GetProperty("id").GetString()!;

        var updatePayload = new { name = "Updated Name", description = "Updated" };
        var response = await _client.PutAsJsonAsync($"/api/collections/{id}", updatePayload);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("name").GetString().Should().Be("Updated Name");
        body.GetProperty("description").GetString().Should().Be("Updated");
    }

    [Fact]
    public async Task DeleteCollection_ShouldReturnNoContent()
    {
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createPayload = new { name = "To Delete", description = "" };
        var createResponse = await _client.PostAsJsonAsync("/api/collections", createPayload);
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var id = created.GetProperty("id").GetString()!;

        var response = await _client.DeleteAsync($"/api/collections/{id}");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteCollection_WithNonExistentId_ShouldReturnNoContent()
    {
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.DeleteAsync($"/api/collections/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task AuthRequired_WithoutToken_ShouldReturnUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/collections");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }
}
