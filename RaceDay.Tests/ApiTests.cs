using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay.Tests
{
    public class ApiTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public ApiTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Register_WithValidDetails_ReturnsCreated()
        {
            var client = _factory.CreateClient();

            var request = new
            {
                firstName = "New",
                lastName = "User",
                email = "newuser@test.com",
                password = "Password123!",
                role = "Participant"
            };

            var response = await client.PostAsJsonAsync(
                "/api/auth/register",
                request);

            Assert.Equal(
                HttpStatusCode.Created,
                response.StatusCode);
        }

        [Fact]
        public async Task Register_WithInvalidRole_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();

            var request = new
            {
                firstName = "Test",
                lastName = "User",
                email = "invalid@test.com",
                password = "Password123!",
                role = "Admin"
            };

            var response = await client.PostAsJsonAsync(
                "/api/auth/register",
                request);

            Assert.Equal(
                HttpStatusCode.BadRequest,
                response.StatusCode);
        }

        [Fact]
        public async Task Login_WithWrongPassword_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var request = new
            {
                email = "organiser@test.com",
                password = "WrongPassword"
            };

            var response = await client.PostAsJsonAsync(
                "/api/auth/login",
                request);

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }

        [Fact]
        public async Task Profile_WithoutLogin_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync(
                "/api/users/me");

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }

        [Fact]
        public async Task Participant_CannotCreateEvent()
        {
            var client = _factory.CreateClient();

            var loginRequest = new
            {
                email = "participant@test.com",
                password = "Password123!"
            };

            var loginResponse = await client.PostAsJsonAsync(
                "/api/auth/login",
                loginRequest);

            Assert.Equal(
                HttpStatusCode.OK,
                loginResponse.StatusCode);

            var eventRequest = new
            {
                eventTypeId = 1,
                name = "Test Event",
                description = "Test event",
                eventDate = "2026-12-01",
                location = "Johannesburg",
                distanceKm = 10,
                bannerImageUrl = (string?)null
            };

            var response = await client.PostAsJsonAsync(
                "/api/events",
                eventRequest);

            Assert.Equal(
                HttpStatusCode.Forbidden,
                response.StatusCode);
        }

        [Fact]
        public async Task Organiser_CanCreateEvent()
        {
            var client = _factory.CreateClient();

            var loginRequest = new
            {
                email = "organiser@test.com",
                password = "Password123!"
            };

            var loginResponse = await client.PostAsJsonAsync(
                "/api/auth/login",
                loginRequest);

            Assert.Equal(
                HttpStatusCode.OK,
                loginResponse.StatusCode);

            var eventRequest = new
            {
                eventTypeId = 1,
                name = "Test Run",
                description = "Testing event creation",
                eventDate = "2026-12-01",
                location = "Johannesburg",
                distanceKm = 10,
                bannerImageUrl = (string?)null
            };

            var response = await client.PostAsJsonAsync(
                "/api/events",
                eventRequest);

            Assert.Equal(
                HttpStatusCode.Created,
                response.StatusCode);
        }
    }
}
