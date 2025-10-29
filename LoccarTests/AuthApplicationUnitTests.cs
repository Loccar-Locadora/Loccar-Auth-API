using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
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
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

        public AuthApplicationUnitTests()
        {
            _authRepoMock = new Mock<IAuthRepository>();

            var inMemorySettings = new Dictionary<string, string> 
            {
                {"Jwt:Key", "MinhaChaveSecretaSuperSegura1234567890"},
                {"Jwt:Issuer", "Loccar"},  
                {"Jwt:Audience", "Loccar"},
                {"LoccarApi:BaseUrl", "http://fake-api"}
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        }

        #region JWT Token Generation Tests

        [Fact]
        public async Task Login_ShouldGenerateValidJwtToken_WhenCredentialsAreCorrect()
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
                Roles = new List<Role> { new Role { Id = 1, Name = "User" } }
            };

            _authRepoMock.Setup(r => r.FindUserByEmail("test@email.com"))
                         .ReturnsAsync(user);

            var httpClient = new HttpClient(new FakeHttpMessageHandler(
                new HttpResponseMessage(HttpStatusCode.OK)));
            var authApp = new AuthApplication(_configuration, _authRepoMock.Object, httpClient);

            var request = new LoginRequest
            {
                Email = "test@email.com",
                Password = password
            };

            // Act
            var result = await authApp.Login(request);

            // Assert
            result.Code.Should().Be("200");
            result.Data.Should().NotBeNullOrEmpty();

            // Validate JWT token structure
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(result.Data);

            token.Should().NotBeNull();
            token.Claims.Should().Contain(c => c.Type == "name" && c.Value == "TestUser");
            token.Claims.Should().Contain(c => c.Type == "id" && c.Value == "1");
            token.Issuer.Should().Be("Loccar");
            token.Audiences.Should().Contain("Loccar");
        }

        [Fact]
        public async Task Login_ShouldSetCorrectTokenExpiration()
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
                Roles = new List<Role> { new Role { Id = 1, Name = "User" } }
            };

            _authRepoMock.Setup(r => r.FindUserByEmail("test@email.com"))
                         .ReturnsAsync(user);

            var httpClient = new HttpClient(new FakeHttpMessageHandler(
                new HttpResponseMessage(HttpStatusCode.OK)));
            var authApp = new AuthApplication(_configuration, _authRepoMock.Object, httpClient);

            var request = new LoginRequest
            {
                Email = "test@email.com",
                Password = password
            };

            var beforeLogin = DateTime.UtcNow;

            // Act
            var result = await authApp.Login(request);

            // Assert
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(result.Data);

            var expectedExpiration = beforeLogin.AddHours(1);
            token.ValidTo.Should().BeCloseTo(expectedExpiration, TimeSpan.FromMinutes(1));
        }

        #endregion

        #region Password Hashing Tests

        [Fact]
        public async Task Register_ShouldHashPasswordCorrectly()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "test@email.com",
                Username = "TestUser",
                Password = "plainTextPassword",
                Cnh = "12345678901",
                CellPhone = "61999999999"
            };

            User capturedUser = null;
            _authRepoMock.Setup(r => r.FindUserByEmail(request.Email))
                         .ReturnsAsync((User)null);
            _authRepoMock.Setup(r => r.RegisterUser(It.IsAny<User>()))
                         .Callback<User>(user => capturedUser = user)
                         .Returns(Task.CompletedTask);

            var httpClient = new HttpClient(new FakeHttpMessageHandler(
                new HttpResponseMessage(HttpStatusCode.Created)));
            var authApp = new AuthApplication(_configuration, _authRepoMock.Object, httpClient);

            // Act
            await authApp.Register(request);

            // Assert
            capturedUser.Should().NotBeNull();
            capturedUser.PasswordHash.Should().NotBe("plainTextPassword");
            BCrypt.Net.BCrypt.Verify("plainTextPassword", capturedUser.PasswordHash).Should().BeTrue();
        }

        [Fact]
        public async Task Login_ShouldVerifyPasswordCorrectly()
        {
            // Arrange
            var plainTextPassword = "mySecretPassword";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainTextPassword);
            var user = new User 
            { 
                Id = 1, 
                Username = "TestUser", 
                Email = "test@email.com", 
                PasswordHash = hashedPassword,
                Roles = new List<Role> { new Role { Id = 1, Name = "User" } }
            };

            _authRepoMock.Setup(r => r.FindUserByEmail("test@email.com"))
                         .ReturnsAsync(user);

            var httpClient = new HttpClient(new FakeHttpMessageHandler(
                new HttpResponseMessage(HttpStatusCode.OK)));
            var authApp = new AuthApplication(_configuration, _authRepoMock.Object, httpClient);

            var correctRequest = new LoginRequest
            {
                Email = "test@email.com",
                Password = plainTextPassword
            };

            var incorrectRequest = new LoginRequest
            {
                Email = "test@email.com",
                Password = "wrongPassword"
            };

            // Act & Assert - Correct password
            var correctResult = await authApp.Login(correctRequest);
            correctResult.Code.Should().Be("200");

            // Act & Assert - Incorrect password
            var incorrectResult = await authApp.Login(incorrectRequest);
            incorrectResult.Code.Should().Be("401");
        }

        #endregion

        #region Repository Interaction Tests

        [Fact]
        public async Task Register_ShouldCallRepositoryWithCorrectUserData()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "test@email.com",
                Username = "TestUser",
                Password = "123456",
                Cnh = "12345678901",
                CellPhone = "61999999999"
            };

            User capturedUser = null;
            _authRepoMock.Setup(r => r.FindUserByEmail(request.Email))
                         .ReturnsAsync((User)null);
            _authRepoMock.Setup(r => r.RegisterUser(It.IsAny<User>()))
                         .Callback<User>(user => capturedUser = user)
                         .Returns(Task.CompletedTask);

            var httpClient = new HttpClient(new FakeHttpMessageHandler(
                new HttpResponseMessage(HttpStatusCode.Created)));
            var authApp = new AuthApplication(_configuration, _authRepoMock.Object, httpClient);

            // Act
            await authApp.Register(request);

            // Assert
            _authRepoMock.Verify(r => r.FindUserByEmail(request.Email), Times.Once);
            _authRepoMock.Verify(r => r.RegisterUser(It.IsAny<User>()), Times.Once);

            capturedUser.Should().NotBeNull();
            capturedUser.Username.Should().Be(request.Username);
            capturedUser.Email.Should().Be(request.Email);
            capturedUser.Roles.Should().HaveCount(1);
            capturedUser.Roles.First().Id.Should().Be(1);
        }

        [Fact]
        public async Task Login_ShouldCallRepositoryOnlyOnce()
        {
            // Arrange
            var user = new User 
            { 
                Id = 1, 
                Username = "TestUser", 
                Email = "test@email.com", 
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Roles = new List<Role> { new Role { Id = 1, Name = "User" } }
            };

            _authRepoMock.Setup(r => r.FindUserByEmail("test@email.com"))
                         .ReturnsAsync(user);

            var httpClient = new HttpClient(new FakeHttpMessageHandler(
                new HttpResponseMessage(HttpStatusCode.OK)));
            var authApp = new AuthApplication(_configuration, _authRepoMock.Object, httpClient);

            var request = new LoginRequest
            {
                Email = "test@email.com",
                Password = "123456"
            };

            // Act
            await authApp.Login(request);

            // Assert
            _authRepoMock.Verify(r => r.FindUserByEmail("test@email.com"), Times.Once);
        }

        #endregion

        #region HTTP Client Tests

        [Fact]
        public async Task Register_ShouldHandleHttpClientFailure()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "test@email.com",
                Username = "TestUser",
                Password = "123456",
                Cnh = "12345678901",
                CellPhone = "61999999999"
            };

            _authRepoMock.Setup(r => r.FindUserByEmail(request.Email))
                         .ReturnsAsync((User)null);
            _authRepoMock.Setup(r => r.RegisterUser(It.IsAny<User>()))
                         .Returns(Task.CompletedTask);

            // HTTP client returns error
            var httpClient = new HttpClient(new FakeHttpMessageHandler(
                new HttpResponseMessage(HttpStatusCode.BadRequest)));
            var authApp = new AuthApplication(_configuration, _authRepoMock.Object, httpClient);

            // Act
            var result = await authApp.Register(request);

            // Assert
            result.Code.Should().Be("400");
            result.Message.Should().Be("Erro ao cadastrar locatário");
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task Register_ShouldSendCorrectHttpRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "test@email.com",
                Username = "TestUser",
                Password = "123456",
                Cnh = "12345678901",
                CellPhone = "61999999999"
            };

            _authRepoMock.Setup(r => r.FindUserByEmail(request.Email))
                         .ReturnsAsync((User)null);
            _authRepoMock.Setup(r => r.RegisterUser(It.IsAny<User>()))
                         .Returns(Task.CompletedTask);

            var capturedRequest = new List<HttpRequestMessage>();
            var fakeHandler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.Created));
            fakeHandler.OnSendAsync = req => capturedRequest.Add(req);

            var httpClient = new HttpClient(fakeHandler);
            var authApp = new AuthApplication(_configuration, _authRepoMock.Object, httpClient);

            // Act
            await authApp.Register(request);

            // Assert
            capturedRequest.Should().HaveCount(1);
            var httpRequest = capturedRequest.First();
            httpRequest.Method.Should().Be(HttpMethod.Post);
            httpRequest.RequestUri.ToString().Should().Be("http://fake-api/locatario/register");
            httpRequest.Content.Headers.ContentType.MediaType.Should().Be("application/json");
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public async Task Login_ShouldHandleRepositoryException()
        {
            // Arrange
            _authRepoMock.Setup(r => r.FindUserByEmail(It.IsAny<string>()))
                         .ThrowsAsync(new Exception("Database connection failed"));

            var httpClient = new HttpClient(new FakeHttpMessageHandler(
                new HttpResponseMessage(HttpStatusCode.OK)));
            var authApp = new AuthApplication(_configuration, _authRepoMock.Object, httpClient);

            var request = new LoginRequest
            {
                Email = "test@email.com",
                Password = "123456"
            };

            // Act
            var result = await authApp.Login(request);

            // Assert
            result.Code.Should().Be("500");
            result.Message.Should().StartWith("Ocorreu um erro inesperado:");
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task Register_ShouldHandleRepositoryException()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "test@email.com",
                Username = "TestUser",
                Password = "123456",
                Cnh = "12345678901",
                CellPhone = "61999999999"
            };

            _authRepoMock.Setup(r => r.FindUserByEmail(request.Email))
                         .ThrowsAsync(new Exception("Database connection failed"));

            var httpClient = new HttpClient(new FakeHttpMessageHandler(
                new HttpResponseMessage(HttpStatusCode.Created)));
            var authApp = new AuthApplication(_configuration, _authRepoMock.Object, httpClient);

            // Act
            var result = await authApp.Register(request);

            // Assert
            result.Code.Should().Be("500");
            result.Message.Should().StartWith("Ocorreu um erro inesperado:");
            result.Data.Should().BeNull();
        }

        #endregion

        #region Configuration Tests

        [Fact]
        public async Task Login_ShouldUseCorrectJwtConfiguration()
        {
            // Arrange
            var customSettings = new Dictionary<string, string> 
            {
                {"Jwt:Key", "CustomSecretKey1234567890123456789"},
                {"Jwt:Issuer", "CustomIssuer"},  
                {"Jwt:Audience", "CustomAudience"}
            };
            var customConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(customSettings)
                .Build();

            var user = new User 
            { 
                Id = 1, 
                Username = "TestUser", 
                Email = "test@email.com", 
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Roles = new List<Role> { new Role { Id = 1, Name = "User" } }
            };

            _authRepoMock.Setup(r => r.FindUserByEmail("test@email.com"))
                         .ReturnsAsync(user);

            var httpClient = new HttpClient(new FakeHttpMessageHandler(
                new HttpResponseMessage(HttpStatusCode.OK)));
            var authApp = new AuthApplication(customConfig, _authRepoMock.Object, httpClient);

            var request = new LoginRequest
            {
                Email = "test@email.com",
                Password = "123456"
            };

            // Act
            var result = await authApp.Login(request);

            // Assert
            result.Code.Should().Be("200");
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(result.Data);
            
            token.Issuer.Should().Be("CustomIssuer");
            token.Audiences.Should().Contain("CustomAudience");
        }

        #endregion
    }
}