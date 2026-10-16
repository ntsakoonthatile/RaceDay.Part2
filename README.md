
# RaceDay Part 2

## 1. Project Description

RaceDay is a web-based event management system developed for road running, walking and cycling events in South Africa. The system helps event organisers manage events, categories, participant enrolments and race results. Participants can register for an account, log in, browse available events, enter events by selecting a category, view their enrolments and check their results. Organisers can create, update and delete events, manage event categories, view enrolments and capture participant results. This project implements the backend REST API using ASP.NET Core Web API, Entity Framework Core and Microsoft SQL Server. It also includes authentication, role-based access control, API documentation and automated unit testing.

## 2. Technologies Used

The following technologies and tools were used to develop RaceDay Part 2:

* **C#:** Main programming language.
* **ASP.NET Core Web API:** Used to develop the REST API.
* **Entity Framework Core:** Used to communicate with the database.
* **Microsoft SQL Server:** Used to store application data.
* **SQL Server Express:** Used as the local database server.
* **Swagger:** Used to view and test API endpoints.
* **BCrypt:** Used to hash passwords before storing them.
* **ASP.NET Core Session:** Used to maintain the authenticated user's session and role.
* **xUnit:** Used to write and run unit tests.
* **Visual Studio:** Used to develop and test the application.
* **Git and GitHub:** Used for version control and source code submission.
* **GitHub Actions:** Used to build the project and run automated tests.

## 3. User Roles

RaceDay supports two user roles.

### Organiser

An organiser can:

* Register and log in.
* View and update their profile.
* Create, view, update and delete events.
* Create and manage event categories.
* View enrolments for their events.
* Update enrolment statuses.
* Capture participant race results.

### Participant

A participant can:

* Register and log in.
* View and update their profile.
* Browse available events.
* View event categories.
* Enrol in an event by selecting a category.
* View their own enrolments.
* View their own race results.

The API uses role-based access control to restrict protected operations to authorised users.

## 4. Requirements

Before running the project, make sure the following software is installed:

* Visual Studio 2022 with the ASP.NET and web development workload.
* .NET 8 SDK.
* SQL Server Express.
* SQL Server Management Studio (SSMS).
* Git.
* A web browser for Swagger testing.

A GitHub account is also required to access the repository and view the automated build and test results.

## 5. Database Setup

RaceDay uses Microsoft SQL Server to store user accounts, event types, events, categories, enrolments and results.

The application uses Entity Framework Core migrations to create and update the database structure.

### Steps to set up the database

1. Install and open SQL Server Express.
2. Confirm that the SQL Server instance is available.
3. Open the RaceDay solution in Visual Studio.
4. Open appsettings.json.
5. Check the connection string under ConnectionStrings.
6. Confirm that the server name matches your local SQL Server instance.


10. Open SQL Server Management Studio and refresh the Databases folder.
11. Confirm that the RaceDayPart2DB database and its tables have been created.

**Note:** The connection string must match your own SQL Server setup. If your database already exists and contains data, check the migration status before making database changes.

## 6. How to Open and Run the Project

1. Open the cloned project folder.
2. Locate RaceDay.Part2.sln.
3. Open the solution in Visual Studio.
4. Wait for NuGet packages to restore.
5. Check the database connection string in appsettings.json.
6. Ensure that SQL Server Express is running.
7. Set RaceDay.Api as the startup project.
8. Build the solution by selecting **Build → Build Solution**.
9. Run the application using the HTTPS or HTTP launch profile.
10. Swagger should open in the browser if it is enabled in the current development configuration.

## 8. Testing the API

Swagger provides an interface for viewing and testing the available API endpoints.

The API includes functionality for:

* User registration and login.
* User profile management.
* Event management.
* Event category management.
* Participant enrolments.
* Enrolment status management.
* Race result management.

To test the API:

1. Run the application.
2. Open Swagger in the browser.
3. Register a user with the Organiser or Participant role.
4. Log in using the registered account.
5. Test the endpoints available to that role.
6. Confirm that protected endpoints reject unauthenticated requests.
7. Confirm that users cannot perform actions restricted to the other role.

Use valid request data when testing the endpoints.

## 9. Running Unit Tests

The solution includes an xUnit test project called RaceDay.Tests.

To run the tests in Visual Studio:

1. Open the solution.
2. Select **Test → Test Explorer**.
3. Click **Run All Tests**.
4. Check the results and investigate any failed tests.

## Video Presentation
https://youtu.be/has0DX77ZJQ

RaceDay Part 2 provides a backend API for managing road running, walking and cycling events. It supports two user roles and includes event management, category management, participant enrolments and race results.The project uses Entity Framework Core and SQL Server for data storage, Swagger for API testing, xUnit for automated testing and GitHub Actions for continuous integration. These technologies help make the system easier to develop, test and maintain.

##  AI Assistance Disclosure
Was used when I was stuck

AI assistance was used to help understand the requirements, troubleshoot errors and support parts of the development process. The code and project functionality should be reviewed and tested by the student before submission.
