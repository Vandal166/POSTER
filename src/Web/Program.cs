//#define USE_SEEDING // comment this line to disable seeding

using Application;
using Infrastructure;
using Serilog;
using Web;
using Web.Endpoints;
using Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    .AddApplication(builder.Configuration)
    .AddInfrastructure(builder.Configuration)
    .AddWebServices(builder.Configuration);

builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});

builder.Services.AddRazorPages()
    .AddRazorRuntimeCompilation();

builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseStaticFiles(); // Serve static files like CSS, JS, images, etc.
app.UseRouting(); // Enable routing for endpoints
app.UseStatusCodePagesWithReExecute("/Error"); // Custom error pages for status codes

app.UseAuthentication(); // Authentication - login, token validation, etc.
app.UseAuthorization(); // Authorization - access control based on policies/roles

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();


app.MapBlobStorageEndpoints(); // used for uploading/downloading (videos/images)

app.MapHub<MessageNotificationHub>("/messageHub");
app.MapHub<ConversationNotificationHub>("/conversationHub");

#if USE_SEEDING
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
    await seeder.SeedAsync(
        userCount:        50,
        postsPerUser:     20,
        commentsPerPost:  10, CancellationToken.None
    );
}
#endif

app.Run();