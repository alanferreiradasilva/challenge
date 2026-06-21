namespace SpaceExplorer.API.Endpoints;

public abstract class BaseEndpoints : IEndpointMapper
{
    public abstract Task Map(WebApplication app);
}
