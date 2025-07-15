namespace Web.Endpoints;

public static class RootEndpoint
{
    public static IEndpointRouteBuilder MapRootEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", () =>
            {
                var rng = Random.Shared;
                return Enumerable.Range(1, 5).Select(i => new 
                {
                    Date         = DateOnly.FromDateTime(DateTime.Now.AddDays(i)),
                    TemperatureC = rng.Next(-20, 55),
                    Summary      = new[] 
                            { "Freezing","Bracing","Chilly","Cool","Mild","Warm","Balmy","Hot","Sweltering","Scorching" }
                        [rng.Next(10)]
                });
            }).WithName("GetWeatherForecast");
        
        app.MapGet("/requireAuth", () =>
        {
            var rng = Random.Shared;
            var summaries = new[] { "Authorized", "Nice", "Work" };
            return Enumerable.Range(1, 5).Select(i => new
            {
                Date         = DateOnly.FromDateTime(DateTime.Now.AddDays(i)),
                TemperatureC = rng.Next(-20, 55),
                Summary      = summaries[rng.Next(summaries.Length)]
            });
        })
        .RequireAuthorization()
        .WithName("GetWeatherForecastWithAuth");

        return app;
    }
}
        