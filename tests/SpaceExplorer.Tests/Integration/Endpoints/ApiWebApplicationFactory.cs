using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using SpaceExplorer.API;
using SpaceExplorer.Application.Common.Pagination;
using SpaceExplorer.Application.ExternalServices;
using SpaceExplorer.Application.Features.Images.Dtos;

namespace SpaceExplorer.Tests.Integration.Endpoints;

public class ApiWebApplicationFactory : WebApplicationFactory<ApiMarker>
{
    static ApiWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("Jwt__Secret", "test-super-secret-key-that-is-32-characters!!");
        Environment.SetEnvironmentVariable("UseInMemoryDatabase", "true");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<INasaService>();
            var nasaMock = Substitute.For<INasaService>();
            nasaMock.SearchAsync(Arg.Any<NasaSearchRequest>(), Arg.Any<CancellationToken>())
                .Returns(new PagedResult<NasaImageDto>(
                [
                    new NasaImageDto("nasa1", "Test Image", "A test image", "https://test.url/image.jpg", null, null, null)
                ], 1, 1, 100));
            services.AddSingleton(nasaMock);

            services.RemoveAll<IAiService>();
            var aiMock = Substitute.For<IAiService>();
            aiMock.EnrichImageAsync(Arg.Any<EnrichItemRequest>(), Arg.Any<CancellationToken>())
                .Returns(new EnrichItemResponse("AI generated description", ["Curiosity 1", "Curiosity 2"]));
            aiMock.SuggestTagsAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
                .Returns(["space", "nasa", "mars", "planet", "exploration"]);
            services.AddSingleton(aiMock);
        });
    }
}
