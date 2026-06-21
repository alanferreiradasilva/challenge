using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace SpaceExplorer.Tests.Integration.Endpoints;

public class AuthEndpointsTest : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AuthEndpointsTest(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnToken()
    {
        var payload = new { name = "John Doe", email = "john@test.com", password = "Password123!" };

        var response = await _client.PostAsJsonAsync("/api/auth/register", payload);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("token").GetString().Should().NotBeNullOrEmpty();
        body.GetProperty("email").GetString().Should().Be("john@test.com");
        body.GetProperty("name").GetString().Should().Be("John Doe");
        body.GetProperty("userId").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnError()
    {
        var payload = new { name = "Jane", email = "duplicate@test.com", password = "Password123!" };
        await _client.PostAsJsonAsync("/api/auth/register", payload);

        var response = await _client.PostAsJsonAsync("/api/auth/register", payload);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        var registerPayload = new { name = "Alice", email = "alice@test.com", password = "Password123!" };
        await _client.PostAsJsonAsync("/api/auth/register", registerPayload);

        var loginPayload = new { email = "alice@test.com", password = "Password123!" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginPayload);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("token").GetString().Should().NotBeNullOrEmpty();
        body.GetProperty("email").GetString().Should().Be("alice@test.com");
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldReturnError()
    {
        var registerPayload = new { name = "Bob", email = "bob@test.com", password = "CorrectPass1!" };
        await _client.PostAsJsonAsync("/api/auth/register", registerPayload);

        var loginPayload = new { email = "bob@test.com", password = "WrongPassword" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginPayload);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ShouldReturnError()
    {
        var loginPayload = new { email = "nonexistent@test.com", password = "Password123!" };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginPayload);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.InternalServerError);
    }
}
