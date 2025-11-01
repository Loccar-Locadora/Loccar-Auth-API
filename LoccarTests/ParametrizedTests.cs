using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using LoccarApplication;
using LoccarDomain.Common;
using LoccarDomain.Login;
using LoccarDomain.Register;
using LoccarInfra.Interfaces;
using LoccarInfra.ORM.model;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace LoccarTests
{
    public class ParametrizedTests
    {
        private readonly Mock<IAuthRepository> _authRepoMock;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly AuthApplication _authApp;

        public ParametrizedTests()
        {
            _authRepoMock = new Mock<IAuthRepository>();

            var inMemorySettings = new Dictionary<string, string?>
            {
                { "Jwt:Key", "MinhaChaveSecretaSuperSegura1234567890" },
                { "Jwt:Issuer", "Loccar" },
                { "Jwt:Audience", "Loccar" },
                { "LoccarApi:BaseUrl", "http://fake-api" },
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _httpClient = MockHttpClientFactory.CreateSuccessClient();
            _authApp = new AuthApplication(_configuration, _authRepoMock.Object, _httpClient);
        }

        public static IEnumerable<object[]> ValidRegisterData()
        {
            yield return new object[]
            {
                "test1@email.com",
                "User1",
                "password123",
                "12345678901",
                "61999999999",
                "201",
                "Usuario cadastrado com sucesso!",
            };

            yield return new object[]
            {
                "test2@email.com",
                "User2",
                "mypassword",
                "98765432109",
                "11888888888",
                "201",
                "Usuario cadastrado com sucesso!",
            };

            yield return new object[]
            {
                "admin@company.com",
                "AdminUser",
                "adminpass",
                "11111111111",
                "21777777777",
                "201",
                "Usuario cadastrado com sucesso!",
            };
        }

        [Theory]
        [InlineData("user1@email.com", "password123", "200", "Usuario logado com sucesso")]
        [InlineData("user2@email.com", "mypassword", "200", "Usuario logado com sucesso")]
        [InlineData("admin@email.com", "adminpass", "200", "Usuario logado com sucesso")]
        public async Task LoginShouldReturnExpectedResultWithValidCredentialsAsync(
            string email,
            string password,
            string expectedCode,
            string expectedMessage)
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "TestUser",
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Roles = new List<Role> { new Role { Id = 1, Name = "User" } },
            };

            _authRepoMock.Setup(r => r.FindUserByEmail(email))
                         .ReturnsAsync(user);

            var request = new LoginRequest { Email = email, Password = password };

            // Act
            var result = await _authApp.LoginAsync(request);

            // Assert
            result.Code.Should().Be(expectedCode);
            result.Message.Should().Be(expectedMessage);
            result.Data.Should().NotBeNullOrEmpty();
        }

        [Theory]
        [InlineData("nonexistent@email.com", "anypassword")]
        [InlineData("user@email.com", "wrongpassword")]
        [InlineData("", "password")]
        [InlineData("user@email.com", "")]
        public async Task LoginShouldReturn401WithInvalidCredentialsAsync(string email, string password)
        {
            // Arrange
            if (!string.IsNullOrEmpty(email) && password == "wrongpassword")
            {
                var user = new User
                {
                    Id = 1,
                    Username = "TestUser",
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword"),
                    Roles = new List<Role> { new Role { Id = 1, Name = "User" } },
                };
                _authRepoMock.Setup(r => r.FindUserByEmail(email)).ReturnsAsync(user);
            }
            else
            {
                _authRepoMock.Setup(r => r.FindUserByEmail(It.IsAny<string>())).ReturnsAsync((User?)null);
            }

            var request = new LoginRequest { Email = email, Password = password };

            // Act
            var result = await _authApp.LoginAsync(request);

            // Assert
            result.Code.Should().Be("401");
            result.Message.Should().Be("Usuario nao autorizado");
            result.Data.Should().BeNull();
        }

        [Theory]
        [MemberData(nameof(ValidRegisterData))]
        public async Task RegisterShouldReturnSuccessWithValidDataAsync(
            string email,
            string username,
            string password,
            string driverLicense,
            string cellPhone,
            string expectedCode,
            string expectedMessage)
        {
            // Arrange
            _authRepoMock.Setup(r => r.FindUserByEmail(email)).ReturnsAsync((User?)null);
            _authRepoMock.Setup(r => r.RegisterUser(It.IsAny<User>())).Returns(Task.CompletedTask);

            var request = new RegisterRequest
            {
                Email = email,
                Username = username,
                Password = password,
                DriverLicense = driverLicense,
                CellPhone = cellPhone,
            };

            // Act
            var result = await _authApp.RegisterAsync(request);

            // Assert
            result.Code.Should().Be(expectedCode);
            result.Message.Should().Be(expectedMessage);
            result.Data.Should().NotBeNull();
            result.Data!.Email.Should().Be(email);
            result.Data.Username.Should().Be(username);
        }

        [Theory]
        [InlineData("existing1@email.com")]
        [InlineData("existing2@email.com")]
        [InlineData("duplicate@email.com")]
        public async Task RegisterShouldReturn400WhenUserAlreadyExistsAsync(string email)
        {
            // Arrange
            _authRepoMock.Setup(r => r.FindUserByEmail(email))
                         .ReturnsAsync(new User { Email = email });

            var request = new RegisterRequest
            {
                Email = email,
                Username = "TestUser",
                Password = "123456",
                DriverLicense = "12345678901",
                CellPhone = "61999999999",
            };

            // Act
            var result = await _authApp.RegisterAsync(request);

            // Assert
            result.Code.Should().Be("400");
            result.Message.Should().Be("Ja existe um usuario com esse email");
            result.Data.Should().BeNull();
        }

        [Theory]
        [InlineData("", "123456")]
        [InlineData(" ", "123456")]
        [InlineData("invalid-email", "123456")]
        [InlineData("test@", "123456")]
        [InlineData("@email.com", "123456")]
        public async Task LoginShouldHandleInvalidEmailsAsync(string email, string password)
        {
            // Arrange
            _authRepoMock.Setup(r => r.FindUserByEmail(It.IsAny<string>())).ReturnsAsync((User?)null);

            var request = new LoginRequest { Email = email, Password = password };

            // Act
            var result = await _authApp.LoginAsync(request);

            // Assert
            result.Code.Should().Be("401");
            result.Message.Should().Be("Usuario nao autorizado");
        }

        [Theory]
        [InlineData("test@email.com", "")]
        [InlineData("test@email.com", " ")]
        [InlineData("test@email.com", "a")]
        public async Task LoginShouldHandleInvalidPasswordsAsync(string email, string password)
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "TestUser",
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword"),
                Roles = new List<Role> { new Role { Id = 1, Name = "User" } },
            };

            _authRepoMock.Setup(r => r.FindUserByEmail(email)).ReturnsAsync(user);

            var request = new LoginRequest { Email = email, Password = password };

            // Act
            var result = await _authApp.LoginAsync(request);

            // Assert
            result.Code.Should().Be("401");
            result.Message.Should().Be("Usuario nao autorizado");
        }
    }
}
