using FluentAssertions;
using NSubstitute;
using SpaceExplorer.Application.Common.Pagination;
using SpaceExplorer.Application.Features.Collections;
using SpaceExplorer.Application.Features.Collections.Dtos;
using SpaceExplorer.Infrastructure.Repositories;

namespace SpaceExplorer.Tests.Unit.Services;

public class CollectionServiceTests
{
    private readonly ICollectionRepository _repository = Substitute.For<ICollectionRepository>();
    private readonly CollectionService _service;

    public CollectionServiceTests()
    {
        _service = new CollectionService(_repository);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedCollection()
    {
        // Arrange
        var request = new CreateCollectionRequest("Mars Photos", "My Mars collection");
        var expectedDto = new CollectionDto(Guid.NewGuid(), request.Name, request.Description, Guid.NewGuid(), DateTime.UtcNow, 0);
        _repository.CreateAsync(Arg.Any<CollectionDto>(), Arg.Any<CancellationToken>())
                   .Returns(expectedDto);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.Description.Should().Be(request.Description);
        await _repository.Received(1).CreateAsync(Arg.Any<CollectionDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnUserCollections()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var collections = new List<CollectionDto>
        {
            new(Guid.NewGuid(), "Collection 1", null, userId, DateTime.UtcNow, 0),
            new(Guid.NewGuid(), "Collection 2", "Desc", userId, DateTime.UtcNow, 3)
        };
        _repository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
                   .Returns(collections);

        // Act
        var result = await _service.GetByUserIdAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(c => c.UserId.Should().Be(userId));
    }

    [Fact]
    public async Task UpdateAsync_WhenCollectionNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>())
                   .Returns((CollectionDto?)null);

        // Act
        var act = async () => await _service.UpdateAsync(id, new UpdateCollectionRequest("New Name", null));

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetAllAsync_ShouldDelegateToRepository()
    {
        // Arrange
        var request = new PagedRequest(1, 10);
        var pagedResult = new PagedResult<CollectionDto>([], 0, 1, 10);
        _repository.GetAllAsync(request, Arg.Any<CancellationToken>())
                   .Returns(pagedResult);

        // Act
        var result = await _service.GetAllAsync(request);

        // Assert
        result.Should().NotBeNull();
        await _repository.Received(1).GetAllAsync(request, Arg.Any<CancellationToken>());
    }
}
