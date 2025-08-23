# POSTER

A simple .NET 9 web application inspired by Twitter. Users can register, log in, and interact through posts, comments, conversations, and real-time notifications.

---

## Features

- **User Accounts**
  - Register and log in using Keycloak authentication.
  - Edit profile picture.
  - Follow other users and see who follows you.

- **Posts**
  - Create posts with optional images or videos.
  - Track views for each post.
  - Delete own posts.
  - Comment on posts (comments behave similarly to posts).
  - Search posts by content criteria.

- **Conversations**
  - Create conversations by providing:
    - Conversation name  
    - (Optional) profile picture  
    - Selected users to add
  - Conversation creator can:
    - Add/remove users
    - Update name/profile picture
    - Delete the conversation
  - Participants can send messages in that conversation and can leave conversations.

- **Notifications via SignalR**
  - New conversation created
  - Removed from a conversation
  - Updates on a conversation you are part of
  - New post by a followed user

---

## Technical Overview

- Built with **.NET 9**
- Presentation layer implemented with **Razor Pages**
- **Docker Compose** configured with:
  - PostgreSQL as the main database (users, posts, etc.)
  - Keycloak as the identity provider
  - Separate Keycloak database for storing information (passwords, emails)
  - Azurite for blob storage of post images and videos
- **JWT-based authentication** stored in cookies, with session revocation support via Keycloak admin panel
- **HTMX** used for improved UX and dynamic form updates
- Option to **seed the database** with fake data using Bogus
- Project follows **Clean Architecture** principles

---

## Requirements

- [.NET SDK 9.0.304](https://dotnet.microsoft.com/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Azure Storage Explorer](https://azure.microsoft.com/en-us/products/storage/storage-explorer/) (for local blob containers)
- [Playwright](https://playwright.dev/dotnet/) (for functional tests)

---

## Setup and Run

1. Clone the repository:
   ```bash
   git clone https://github.com/Vandal166/POSTER.git
   ```

2. Run Docker Compose:
   ```bash
   docker compose up -d
   ```

3. Navigate to the web project:
   ```bash
   cd src/Web
   ```

4. Apply EF migrations:
   ```bash
   dotnet ef database update
   ```

5. Configure **Keycloak**:

   - Access the Keycloak admin panel.  
     If the container fails with `connection refused`, re-run it (sometimes `web.keycloak` starts before `keycloak.db` initializes).
   - Log in using:
     ```
     Username: admin
     Password: admin
     ```
   - Go to **Manage Realms → Create realm**.  
     Name it `PosterRealm` and import the configuration from `/keycloak/realm-export.json`.

6. Configure client secrets in Keycloak:

   - For client `poster-admin`:  
     Go to **Credentials → Regenerate Client Secret**.  
     Copy the secret into `application.Development.json` under `AdminClientSecret` section.

   - For client `poster-frontend`:  
     Do the same and copy into `ClientSecret` section.

7. Set up storage containers with [Storage Explorer](https://azure.microsoft.com/en-us/products/storage/storage-explorer)

   - Navigate to **Emulator & Attached → Emulator (Default Ports) → Blob Containers**
   - Create two containers: `images` and `videos`.

8. Run the web application:
   ```bash
   dotnet run
   ```

---

## Tests

Located under the `/tests` directory:

- **Domain.UnitTests** → Tests for entities and their business logic.  
- **Application.UnitTests** → Tests for business logic of various services.  
- **Application.IntegrationTests** → Verifies services integrate correctly with the database.  
- **Web.FunctionalTests** → Tests registration, login, and protected pages.

Before running functional tests, configure [Playwright](https://github.com/microsoft/playwright):

```powershell
cd .\tests\Web.FunctionalTests\
powershell bin/Debug/net9.0/playwright.ps1 install
```

---

## Notes

- If authentication fails for a container, try switching ports in Docker Compose and update connection strings.
- Images/icons used in the app:  
  [Default Profile Picture avatar](https://www.flaticon.com/free-icons/profile-image)
  [Default conversation icon](https://www.flaticon.com/free-icons/comment)
  [Freepik](http://www.freepik.com/)
