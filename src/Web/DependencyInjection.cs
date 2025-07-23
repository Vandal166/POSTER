using System.Security.Claims;
using Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Web;

public static class DependencyInjection
{
    public static IServiceCollection AddWebServices(this IServiceCollection services, IConfiguration configuration)
    {
        IConfigurationSection keycloakSettings = configuration.GetSection("Keycloak");
        services.AddAuthorizationBuilder();
        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                //options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme; // Redirects to Keycloak login panel when unauthorized
            })
            .AddCookie(options => 
            {
                options.LoginPath = "/Login";      // redirect here if unauthenticated
                options.LogoutPath = "/Logout";
                
                options.AccessDeniedPath = "/Shared/AccessDenied"; // redirect here if authenticated but not authorized
                options.Events.OnValidatePrincipal = async context =>
                {
                    var lastValidated = context.Properties.IssuedUtc;

                    // every 5 seconds we check if the user is still valid
                    if (lastValidated == null || DateTimeOffset.UtcNow - lastValidated > TimeSpan.FromSeconds(5))
                    {
                        var userId = context.Principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ??
                                     context.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        
                        if (!string.IsNullOrEmpty(userId)) // if the userid is not empty meaning Authenticated
                        {
                            // we call Keycloak admin API to get the latest user state
                            var updatedUser = await context.HttpContext.RequestServices
                                .GetRequiredService<IKeycloakService>()
                                .GetUserAsync(userId);

                            if (updatedUser == null || updatedUser.Enabled == false) // if the user is not found/disable then a change occurred on Keycloak side
                            {
                                // signing them out immediately
                                context.RejectPrincipal();
                                await context.HttpContext.SignOutAsync(); //TODO with message
                                return;
                            }

                            // if the user is found and enabled, we instead update the claims principal cuz they might have changed on Keycloak side
                            var newPrincipal = ClaimsPrincipalFactory.BuildClaims(updatedUser);

                            // replacing the old claims principal
                            context.ReplacePrincipal(newPrincipal);
                            context.ShouldRenew = true;
                        }
                    }
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    // if user is authenticated but missing profileCompleted claim, redirect to CompleteProfile
                    if (context.HttpContext.User.Identity?.IsAuthenticated == true &&
                        !context.HttpContext.User.HasClaim(c =>
                            c.Type == "profileCompleted" &&
                            c.Value.Equals("true", StringComparison.OrdinalIgnoreCase)))
                    {
                        context.Response.Redirect("/CompleteProfile");
                    }
                    else
                    {
                        // Default behavior (redirect to AccessDeniedPath)
                        context.Response.Redirect(context.Options.AccessDeniedPath);
                    }

                    return Task.CompletedTask;
                };
            })
            .AddOpenIdConnect(authenticationScheme: OpenIdConnectDefaults.AuthenticationScheme,
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
                });


        services.AddAuthorizationBuilder()
            .SetDefaultPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim("profileCompleted", "true")
                .Build())
            .AddPolicy("ProfileCompleted", policy =>
                policy.RequireClaim("profileCompleted", "true"))
            .AddPolicy("ProfileNotCompleted", policy =>
                policy.RequireClaim("profileCompleted", "false"));

        
        // named HttpClient for talking to Keycloak’s Token & Admin APIs
        services.AddHttpClient("Keycloak", client =>
                client.BaseAddress = new Uri(keycloakSettings["Authority"]!.TrimEnd('/') + "/"))
            .ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (_,__,___,____) => true
                });
        
        return services;
    }
}