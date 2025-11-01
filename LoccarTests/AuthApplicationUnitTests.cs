using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using LoccarApplication;
using LoccarDomain.Common;
using LoccarDomain.Login;
using LoccarDomain.Register;
using LoccarDomain.User;
using LoccarInfra.Interfaces;
using LoccarInfra.ORM.model;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace LoccarTests
{
    public class AuthApplicationUnitTests
    {
        private readonly Mock<IAuthRepository> _authRepoMock;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly AuthApplication _authApp;

        public AuthApplicationUnitTests()
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

        [Fact]
        public async Task LoginShouldReturn200WhenCredentialsAreValidAsync()
        {
            // Arrange
            var password = "123456";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User
            {
                Id = 1,
                Username = "TestUser",
                Email = "test@email.com",
                PasswordHash = hashedPassword,
                Roles = new List<Role> { new Role { Id = 1, Name = "User" } },
            };

            _authRepoMock.Setup(r => r.FindUserByEmail("test@email.com"))
                         .ReturnsAsync(user);

            var request = new LoginRequest
            {
                Email = "test@email.com",
                Password = password,
            };

            // Act
            var result = await _authApp.LoginAsync(request);

            // Assert
            result.Code.Should().Be("200");
            result.Message.Should().Be("Usuario logado com sucesso");
            result.Data.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task LoginShouldReturn401WhenUserNotFoundAsync()
        {
            // Arrange
            _authRepoMock.Setup(r => r.FindUserByEmail(It.IsAny<string>()))
                         .ReturnsAsync((User?)null);

            var request = new LoginRequest
            {
                Email = "test@email.com",
                Password = "123456",
            };

            // Act
            var result = await _authApp.LoginAsync(request);

            // Assert
            result.Code.Should().Be("401");
            result.Message.Should().Be("Usuario nao autorizado");
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task LoginShouldReturn401WhenPasswordIsWrongAsync()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "TestUser",
                Email = "test@email.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword"),
                Roles = new List<Role> { new Role { Id = 1, Name = "User" } },
            };

            _authRepoMock.Setup(r => r.FindUserByEmail("test@email.com"))
                         .ReturnsAsync(user);

            var request = new LoginRequest
            {
                Email = "test@email.com",
                Password = "wrongpassword",
            };

            // Act
            var result = await _authApp.LoginAsync(request);

            // Assert
            result.Code.Should().Be("401");
            result.Message.Should().Be("Usuario nao autorizado");
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task RegisterShouldReturn201WhenUserDoesNotExistAsync()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "newuser@email.com",
                Username = "NewUser",
                Password = "123456",
                DriverLicense = "12345678901",
                CellPhone = "61999999999",
            };

            _authRepoMock.Setup(r => r.FindUserByEmail(request.Email))
                         .ReturnsAsync((User?)null);
            _authRepoMock.Setup(r => r.RegisterUser(It.IsAny<User>()))
                         .Returns(Task.CompletedTask);

            // Act
            var result = await _authApp.RegisterAsync(request);

            // Assert
            result.Code.Should().Be("201");
            result.Message.Should().Be("Usuario cadastrado com sucesso!");
            result.Data.Should().NotBeNull();
            result.Data!.Email.Should().Be(request.Email);
            result.Data.Username.Should().Be(request.Username);
        }

        [Fact]
        public async Task RegisterShouldReturn400WhenUserAlreadyExistsAsync()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "existing@email.com",
                Username = "ExistingUser",
                Password = "123456",
                DriverLicense = "12345678901",
                CellPhone = "61999999999",
            };

            _authRepoMock.Setup(r => r.FindUserByEmail(request.Email))
                         .ReturnsAsync(new User { Email = request.Email });

            // Act
            var result = await _authApp.RegisterAsync(request);

            // Assert
            result.Code.Should().Be("400");
            result.Message.Should().Be("Ja existe um usuario com esse email");
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task RegisterShouldReturn400WhenHttpClientFailsAsync()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "test@email.com",
                Username = "TestUser",
                Password = "123456",
                DriverLicense = "12345678901",
                CellPhone = "61999999999",
            };

            _authRepoMock.Setup(r => r.FindUserByEmail(request.Email))
                         .ReturnsAsync((User?)null);
            _authRepoMock.Setup(r => r.RegisterUser(It.IsAny<User>()))
                         .Returns(Task.CompletedTask);

            using var httpClient = MockHttpClientFactory.CreateErrorClient();
            var authApp = new AuthApplication(_configuration, _authRepoMock.Object, httpClient);

            // Act
            var result = await authApp.RegisterAsync(request);

            // Assert
            result.Code.Should().Be("400");
            result.Message.Should().Be("Erro ao cadastrar locatario");
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task RegisterShouldCallRepositoryWhenUserIsValidAsync()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "test@email.com",
                Username = "TestUser",
                Password = "123456",
                DriverLicense = "12345678901",
                CellPhone = "61999999999",
            };

            _authRepoMock.Setup(r => r.FindUserByEmail(request.Email))
                         .ReturnsAsync((User?)null);
            _authRepoMock.Setup(r => r.RegisterUser(It.IsAny<User>()))
                         .Returns(Task.CompletedTask);

            // Act
            await _authApp.RegisterAsync(request);

            // Assert
            _authRepoMock.Verify(r => r.FindUserByEmail(request.Email), Times.Once);
            _authRepoMock.Verify(r => r.RegisterUser(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task LoginShouldCallRepositoryOnlyOnceAsync()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "TestUser",
                Email = "test@email.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Roles = new List<Role> { new Role { Id = 1, Name = "User" } },
            };

            _authRepoMock.Setup(r => r.FindUserByEmail("test@email.com"))
                         .ReturnsAsync(user);

            var request = new LoginRequest
            {
                Email = "test@email.com",
                Password = "123456",
            };

            // Act
            await _authApp.LoginAsync(request);

            // Assert
            _authRepoMock.Verify(r => r.FindUserByEmail("test@email.com"), Times.Once);
        }

        [Fact]
        public void ConstructorShouldInitializeCorrectly()
        {
            // Assert
            _authApp.Should().NotBeNull();
        }
    }
}
