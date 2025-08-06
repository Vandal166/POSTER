## 🔐 Required Secrets (Local Development)

This project uses [dotnet user-secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) to store sensitive values locally.

To get started, run the following:

```bash
cd src/Web
dotnet user-secrets init

dotnet user-secrets set "JwtSettings:Secret" "long-secret-key"
dotnet user-secrets set "JwtSettings:Issuer" "your-app"
dotnet user-secrets set "JwtSettings:Audience" "your-audience"

dotnet user-secrets set "ConnectionStrings:Poster_DB" "Host=localhost;Port=5432;Database=Poster_DB;Username=postgres;Password=secret"


<a href="https://www.flaticon.com/free-icons/profile-image" title="profile-image icons">Profile-image icons created by Md Tanvirul Haque - Flaticon</a>