namespace SpaceExplorer.API.Endpoints;

public static class EndpointsRegister
{
    public static void RegisterEndpoints(this WebApplication app)
    {
        var mappers = typeof(Program).Assembly
                        .GetTypes()
                        .Where(t => typeof(IEndpointMapper).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                        .Select(Activator.CreateInstance)
                        .Cast<IEndpointMapper>();

        foreach (var mapper in mappers)
            mapper.Map(app);
    }
}
