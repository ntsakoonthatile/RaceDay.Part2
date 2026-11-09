# RaceDay – Programming 2B POE Part 3

## Project overview

RaceDay is a sporting event management system built using ASP.NET Core MVC and an existing ASP.NET Core Web API. I extended the Part 2 API with web pages for Organisers and Participants in Part 3.

Organisers can manage events and categories, view event enrolments, update enrolment status and record results. Participants can browse upcoming events, enter an event, see their entries and results, and update their profiles.

## Technologies

- C# and .NET 8
- ASP.NET Core MVC and ASP.NET Core Web API
- Entity Framework Core with SQL Server Express
- HTML, CSS and Bootstrap
- xUnit and Swagger
- Docker and Docker Compose
- GitHub Actions
- Local image uploads through the API (lecturer-approved alternative to Azure Blob Storage)

## Project folders

- `RaceDay.Api/` – controllers, DTOs, models, EF Core migrations, image upload endpoint and SQL Server access
- `RaceDay.Mvc/` – MVC controllers, Razor views, models, API client and CSS
- `RaceDay.Tests/` – automated API tests
- `.github/workflows/part3-ci.yml` – build, test and Docker image checks
- `compose.yaml` – starts the MVC website in Docker

Open `RaceDay.Part2.sln` in Visual Studio 2022. The solution includes the Part 3 MVC project.

## Requirements

Install .NET 8 SDK, Visual Studio 2022 (ASP.NET workload), SQL Server Express, and Docker Desktop if demonstrating the container. A local SQL Server database named `RaceDayPart2DB` was used during development.

The API's `ConnectionStrings:DefaultConnection` must match the SQL Server instance. Apply the project's existing EF Core migrations when setting up a new database, after checking whether the schema already exists.

## Run locally

1. Restore the solution's NuGet packages and build it in Visual Studio.
2. Start `RaceDay.Api` using its HTTPS launch profile. Swagger is normally at `https://localhost:7041/swagger/index.html`.
3. Start `RaceDay.Mvc`. Its `appsettings.json` uses `https://localhost:7041/` as the local API address.
4. Register or log in as an Organiser or Participant and test the available pages.

The API must stay running while the MVC website is used.

## Image storage

For Part 3, local image storage was used as the lecturer-approved alternative to Azure. Users upload event banners or profile pictures in MVC. The MVC project sends the files to `POST /api/images/upload` in the API. The API validates the file and saves it in `RaceDay.Api/wwwroot/uploads`, then returns a URL which is stored on the corresponding Event or User record.

Uploads support JPG, PNG and WebP up to 5 MB. Existing uploaded images are runtime data and are not included in this source repository. Upload fresh test images when running the project after cloning.

## Docker

The Docker configuration runs the MVC project in a container. The Part 2 API and SQL Server Express run separately on the Docker host computer.

From the solution folder, run the API in PowerShell:

```powershell
$env:Uploads__PublicBaseUrl = "http://localhost:5041"
dotnet run --project .\RaceDay.Api\RaceDay.Api.csproj --no-launch-profile --urls "http://0.0.0.0:5041"
```

In a second terminal in the same folder:

```powershell
docker compose up --build
```

Open `http://localhost:8080`. The MVC container contacts the API via `http://host.docker.internal:5041/`.

**Important for a virtual machine:** `host.docker.internal` refers to the Docker host, not automatically to a separate VM. If the API is running in a VM but Docker is running on a different laptop, adjust the API address and VM networking before testing. Stop the container with `docker compose down`.

## Testing

Run the tests using:

```powershell
dotnet test RaceDay.Tests/RaceDay.Tests.csproj
```

Manual checks should include login, role-specific navigation, events and categories, enrolments, results, profile changes, image uploading and the Docker website. The presence of the workflow does not guarantee a green CI run; check GitHub Actions for the latest result.

## GitHub Actions

The Part 3 workflow restores and builds the API, MVC and tests, runs the automated test project and builds the MVC Docker image. The workflow is at `.github/workflows/part3-ci.yml`. Check its run in the Actions tab.

## Video presentation

Part 3 demonstration video: **add your unlisted YouTube URL here before submitting.**

The presentation should show the MVC structure, Organiser and Participant features, image uploads into the API's local folder, Docker and GitHub Actions.

## AI assistance disclosure

I used ChatGPT to help with understanding requirements, troubleshooting, code examples and documentation. I reviewed and adapted the code during development and carried out testing as part of the project work.

## Conclusion

RaceDay Part 3 shows how an MVC website can communicate with an existing API to manage sporting events. It also includes image uploading, a container setup and automated build and test checks.
