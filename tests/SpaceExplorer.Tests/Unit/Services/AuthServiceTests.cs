using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SpaceExplorer.Application.Features.Auth;
using SpaceExplorer.Application.Features.Auth.Dtos;
using SpaceExplorer.Infrastructure.Data;
using SpaceExplorer.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;

namespace SpaceExplorer.Tests.Unit.Services;

public class AuthServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);

        _userRepository = new UserRepository(_context);

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "super-secret-test-key-minimum-32-characters-long"
            })
            .Build();

        _service = new AuthService(_userRepository, _configuration, _context);
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldReturnAuthResponse()
    {
        // Arrange
        var request = new RegisterRequest("Alan Smith", "alan@test.com", "Password123!");

        // Act
        var result = await _service.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(request.Email);
        result.Name.Should().Be(request.Name);
        result.Token.Should().NotBeNullOrEmpty();
        result.UserId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var request = new RegisterRequest("Alan", "duplicate@test.com", "Password123!");
        await _service.RegisterAsync(request);

        // Act
        var act = async () => await _service.RegisterAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already registered*");
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnAuthResponse()
    {
        // Arrange
        var register = new RegisterRequest("Alan", "login@test.com", "Password123!");
        await _service.RegisterAsync(register);

        var login = new LoginRequest("login@test.com", "Password123!");

        // Act
        var result = await _service.LoginAsync(login);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.Email.Should().Be(login.Email);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        await _service.RegisterAsync(new RegisterRequest("Alan", "wrong@test.com", "CorrectPass!"));
        var login = new LoginRequest("wrong@test.com", "WrongPassword");

        // Act
        var act = async () => await _service.LoginAsync(login);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid credentials.");
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var login = new LoginRequest("nonexistent@test.com", "Password123!");

        // Act
        var act = async () => await _service.LoginAsync(login);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    public void Dispose() => _context.Dispose();
}
