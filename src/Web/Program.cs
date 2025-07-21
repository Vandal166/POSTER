using System.Security.Claims;
using System.Text;
using Application;
using Application.Contracts;
using Infrastructure;
using Infrastructure.Seeding;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Web.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => // adding swagger logging in/logging out thing
{
    /*// 1) Define the Bearer auth scheme
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter valid JWT token.\n\nExample: \"eyJhbGciOi…\""
    });

    // 2) Require the scheme globally (so protected endpoints automatically ask)
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference 
                { 
                    Type = ReferenceType.SecurityScheme, 
                    Id = "Bearer" 
                }
            },
            new string[] {}
        }
    });*/
});

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services
    .AddScoped<IDataSeeder, DataSeeder>();

builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});

//var jwtConfig = builder.Configuration.GetSection("JwtSettings");
//var secretKey = Encoding.UTF8.GetBytes(jwtConfig["Secret"]!);
var keycloakSettings = builder.Configuration.GetSection("Keycloak");
builder.Services.AddAuthorizationBuilder();
builder.Services
    .AddAuthentication
    (options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme =
                OpenIdConnectDefaults.AuthenticationScheme; // Redirects to Keycloak when unauthorized
            /*options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;*/
        }
    )
    .AddCookie(options => {
            options.LoginPath = "/Login";      // redirect here if unauthenticated
            options.LogoutPath = "/Logout";
        })
    .AddOpenIdConnect
    (
        authenticationScheme: OpenIdConnectDefaults.AuthenticationScheme,
        options =>
        {
            options.ClientId = "Poster-frontend";
            options.ClientSecret = keycloakSettings["ClientSecret"];
            options.Authority = keycloakSettings["Authority"];
            options.Scope.Add("PosterAPI.all");
            options.ResponseType = OpenIdConnectResponseType.Code;
            options.SaveTokens = true;
            options.SignInScheme = "Cookies";
            options.SignOutScheme = "Cookies";
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters.NameClaimType = ClaimTypes.Name;
            options.MapInboundClaims = false;
            options.CallbackPath = "/signin-oidc";
            options.SignedOutCallbackPath = "/signout-callback-oidc";

            options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "sub"); // mapping keycloaks 'sub'(Guid) claim to ClaimTypes.NameIdentifier
            /*// Map Keycloak’s realm_access.roles JSON array to ClaimTypes.Role
            options.ClaimActions.MapJsonKey
            (
                ClaimTypes.Role,
                "realm_access.roles",
                "role"
            );*/
        }
    );
    /*.AddJwtBearer(options =>
    {
        options.Authority = keycloakSettings["Authority"];
        options.Audience  = keycloakSettings["Audience"];
        
        // needs to be false so that the middleware can talk to our Keycloak server locally, which is not using HTTPS.
        options.RequireHttpsMetadata = false; // see https://learn.microsoft.com/en-us/dotnet/aspire/authentication/keycloak-integration
        options.SaveToken            = true;
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtConfig["Keycloak:Authority"],
            ValidAudience            = jwtConfig["Keycloak:Audience"],
            RoleClaimType = ClaimTypes.Role
            //IssuerSigningKey         = new SymmetricSecurityKey(secretKey)
        };
        
    }
    );*/

/*
builder.Services.AddAuthorizationBuilder();
var authBuilder = builder.Services.AddAuthentication("Keycloak");


authBuilder.AddOpenIdConnect(
        authenticationScheme: "Keycloak", 
        options =>
    {
        options.ClientId = "Poster-frontend";
        options.ClientSecret = "qoL5X0Z5AC5YYodwaARJYPygpz4Rk8VH"; //TODO dotnet secrets
        options.Authority = keycloakSettings["Authority"];
        options.Scope.Add("PosterAPI.all");
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.SaveTokens = true;
        options.SignInScheme = JwtBearerDefaults.AuthenticationScheme;
        options.SignOutScheme = JwtBearerDefaults.AuthenticationScheme;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters.NameClaimType = ClaimTypes.Name;
        options.MapInboundClaims = false;
        options.CallbackPath = "/signin-oidc";
        options.SignedOutCallbackPath = "/signout-callback-oidc";
        
        // Map Keycloak’s realm_access.roles JSON array to ClaimTypes.Role
        options.ClaimActions.MapJsonKey(
            ClaimTypes.Role,
            "realm_access.roles",
            "role");
    }).AddCookie("Cookies");*/

//builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthorizationHandler>();
/*builder.Services.AddHttpClient<HttpClient>
    (client => client.BaseAddress = new Uri("http://localhost:8080/")).AddHttpMessageHandler<AuthorizationHandler>();*/
// 2️⃣ A named HttpClient for talking to Keycloak’s Token & Admin APIs
builder.Services.AddHttpClient("Keycloak", client =>
        client.BaseAddress = new Uri(keycloakSettings["Authority"].TrimEnd('/') + "/"))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new HttpClientHandler { ServerCertificateCustomValidationCallback = (_,__,___,____) => true });

/*builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
    .AddPolicy("CanCreatePost", policy => policy.RequireRole("Admin", "Author"));*/

builder.Services.AddRazorPages()
    .AddRazorRuntimeCompilation(); // Enable runtime compilation of Razor pages in development

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

app.UseAuthentication(); // Authentication - login, token validation, etc.
app.UseAuthorization(); // Authorization - access control based on policies/roles

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();

app.MapRootEndpoint();
app.MapAuthEndpoints();
app.MapPostEndpoints()
    .MapPostLikeEndpoints()
    .MapPostCommentsEndpoints();

/*
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
    await seeder.SeedAsync(
        userCount:        50,
        postsPerUser:     20,
        commentsPerPost:  10, CancellationToken.None
    );
}*/

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}