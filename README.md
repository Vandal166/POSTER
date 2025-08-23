steps taken from zero:
1. clone repo
2. run compose file
3. navigate to src/Web
4. dotnet ef database update
5. go to keycloak admin panel, if the container is down due to an connection refused
try to re-run it, sometimes the web.keycloak that depends on keycloak.db will be run before the database is initialized
6. log in using admin admin
7. go to Manage realms -> Create realm -> name it PosterRealm and import the configuration file from /keycloak/realm-export.json -> Create.
8. Configure admin client secrets in Keycloak:
From the PosterRealm -> Clients -> 'poster-admin':
Into Credentials tab:
Regenerate 'Client secret' -> copy to the application.Developement.json 'AdminClientSecret' section.
For Client 'Poster-frontend':
Into Credentials tab:
Regenerate 'Client secret' -> copy to the application.Developement.json 'ClientSecret' section.
9. using Storage Explorer(link):
navigate to Emulator & Attached -> Emulator(Default Ports) -> Blob Containers and create 'images' and 'videos' containers
10. Run the web app and done!


<a href="https://www.flaticon.com/free-icons/profile-image" title="profile-image icons">Profile-image icons created by Md Tanvirul Haque - Flaticon</a>

Docker Desktop installed:
www...
if auth fails for an container then try switching ports in docker compose and connection string

For Web.FunctionalTests:
Configrue Playwright:
cd .\tests\Web.FunctionalTests\
powershell bin/Debug/net9.0/playwright.ps1 install


Using:
.NET 9.0.304
