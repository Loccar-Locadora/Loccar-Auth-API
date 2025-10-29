using System.Collections.Generic;
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
    public class AuthApplicationParameterizedTests
    {
        private readonly Mock<IAuthRepository> _authRepoMock;
        private readonly IConfiguration _configuration;
        private readonly AuthApplication _authApp;

        public AuthApplicationParameterizedTests()
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

            var httpClient = new HttpClient(new FakeHttpMessageHandler(
                new HttpResponseMessage(System.Net.HttpStatusCode.Created)));

            _authApp = new AuthApplication(_configuration, _authRepoMock.Object, httpClient);
        }

        #region Parameterized Register Tests

        public static IEnumerable<object[]> ValidRegisterRequests()
        {
            yield return new object[] 
            { 
                new RegisterRequest 
                { 
                    Email = "test@email.com", 
                    Username = "TestUser", 
                    Password = "123456", 
                    Cnh = "12345678901", 
                    CellPhone = "61999999999" 
                },
                "201",
                "Usuário cadastrado com sucesso!"
            };
            
            yield return new object[] 
            { 
                new RegisterRequest 
                { 
                    Email = "user2@domain.com", 
                    Username = "User2", 
                    Password = "password123", 
                    Cnh = "98765432109", 
                    CellPhone = "11888888888" 
                },
                "201",
                "Usuário cadastrado com sucesso!"
            };
            
            yield return new object[] 
            { 
                new RegisterRequest 
                { 
                    Email = "admin@company.org", 
                    Username = "AdminUser", 
                    Password = "admin@2024", 
                    Cnh = "11111111111", 
                    CellPhone = "21777777777" 
                },
                "201", 
                "Usuário cadastrado com sucesso!"
            };
        }

        [Theory]
        [MemberData(nameof(ValidRegisterRequests))]
        public async Task Register_ShouldReturnExpectedResult_WithValidData(
            RegisterRequest request, string expectedCode, string expectedMessage)
        {
            // Arrange
            _authRepoMock.Setup(r => r.FindUserByEmail(request.Email))
                         .ReturnsAsync((User)null);
            _authRepoMock.Setup(r => r.RegisterUser(It.IsAny<User>()))
                         .Returns(Task.CompletedTask);

            // Act
            var result = await _authApp.Register(request);

            // Assert
            result.Code.Should().Be(expectedCode);
            result.Message.Should().Be(expectedMessage);
            result.Data.Should().NotBeNull();
            result.Data.Username.Should().Be(request.Username);
            result.Data.Email.Should().Be(request.Email);
            result.Data.Cnh.Should().Be(request.Cnh);
            result.Data.Cellphone.Should().Be(request.CellPhone);
        }

        public static IEnumerable<object[]> InvalidRegisterRequests()
        {
            yield return new object[] 
            {
                "test@email.com",
                "400",
                "Já existe um usuário com esse email"
            };
            
            yield return new object[] 
            {
                "existing@user.com",
                "400", 
                "Já existe um usuário com esse email"
            };
            
            yield return new object[] 
            {
                "admin@domain.com",
                "400",
                "Já existe um usuário com esse email"
            };
        }

        [Theory]
        [MemberData(nameof(InvalidRegisterRequests))]
        public async Task Register_ShouldReturnError_WhenUserAlreadyExists(
            string email, string expectedCode, string expectedMessage)
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = email,
                Username = "TestUser",
                Password = "123456",
                Cnh = "12345678901",
                CellPhone = "61999999999"
            };

            _authRepoMock.Setup(r => r.FindUserByEmail(email))
                         .ReturnsAsync(new User { Email = email });

            // Act
            var result = await _authApp.Register(request);

            // Assert
            result.Code.Should().Be(expectedCode);
            result.Message.Should().Be(expectedMessage);
            result.Data.Should().BeNull();
        }

        #endregion

        #region Parameterized Login Tests

        public static IEnumerable<object[]> ValidLoginCredentials()
        {
            var password1 = "123456";
            var password2 = "password123";
            var password3 = "admin@2024";

            yield return new object[]
            {
                new LoginRequest { Email = "test@email.com", Password = password1 },
                new User 
                { 
                    Id = 1, 
                    Username = "TestUser", 
                    Email = "test@email.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password1),
                    Roles = new List<Role> { new Role { Id = 1, Name = "User" } }
                },
                "200",
                "Usuário logado com sucesso"
            };

            yield return new object[]
            {
                new LoginRequest { Email = "user2@domain.com", Password = password2 },
                new User 
                { 
                    Id = 2, 
                    Username = "User2", 
                    Email = "user2@domain.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password2),
                    Roles = new List<Role> { new Role { Id = 2, Name = "Admin" } }
                },
                "200",
                "Usuário logado com sucesso"
            };

            yield return new object[]
            {
                new LoginRequest { Email = "admin@company.org", Password = password3 },
                new User 
                { 
                    Id = 3, 
                    Username = "AdminUser", 
                    Email = "admin@company.org", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password3),
                    Roles = new List<Role> { new Role { Id = 1, Name = "SuperAdmin" } }
                },
                "200",
                "Usuário logado com sucesso"
            };
        }

        [Theory]
        [MemberData(nameof(ValidLoginCredentials))]
        public async Task Login_ShouldReturnToken_WithValidCredentials(
            LoginRequest request, User user, string expectedCode, string expectedMessage)
        {
            // Arrange
            _authRepoMock.Setup(r => r.FindUserByEmail(request.Email))
                         .ReturnsAsync(user);

            // Act
            var result = await _authApp.Login(request);

            // Assert
            result.Code.Should().Be(expectedCode);
            result.Message.Should().Be(expectedMessage);
            result.Data.Should().NotBeNullOrEmpty();
        }

        public static IEnumerable<object[]> InvalidLoginCredentials()
        {
            yield return new object[]
            {
                new LoginRequest { Email = "nonexistent@email.com", Password = "123456" },
                (User)null,
                "401",
                "Usuário não autorizado"
            };

            yield return new object[]
            {
                new LoginRequest { Email = "test@email.com", Password = "wrongpassword" },
                new User 
                { 
                    Id = 1, 
                    Username = "TestUser", 
                    Email = "test@email.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword"),
                    Roles = new List<Role> { new Role { Id = 1, Name = "User" } }
                },
                "401",
                "Usuário não autorizado"
            };

            yield return new object[]
            {
                new LoginRequest { Email = "admin@domain.com", Password = "admin123" },
                new User 
                { 
                    Id = 2, 
                    Username = "Admin", 
                    Email = "admin@domain.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("differentpassword"),
                    Roles = new List<Role> { new Role { Id = 2, Name = "Admin" } }
                },
                "401",
                "Usuário não autorizado"
            };
        }

        [Theory]
        [MemberData(nameof(InvalidLoginCredentials))]
        public async Task Login_ShouldReturnUnauthorized_WithInvalidCredentials(
            LoginRequest request, User user, string expectedCode, string expectedMessage)
        {
            // Arrange
            _authRepoMock.Setup(r => r.FindUserByEmail(request.Email))
                         .ReturnsAsync(user);

            // Act
            var result = await _authApp.Login(request);

            // Assert
            result.Code.Should().Be(expectedCode);
            result.Message.Should().Be(expectedMessage);
            result.Data.Should().BeNull();
        }

        #endregion

        #region Edge Cases Tests

        public static IEnumerable<object[]> EdgeCaseEmails()
        {
            yield return new object[] { "" };
            yield return new object[] { " " };
            yield return new object[] { "invalid-email" };
            yield return new object[] { "@domain.com" };
            yield return new object[] { "test@" };
        }

        [Theory]
        [MemberData(nameof(EdgeCaseEmails))]
        public async Task Login_ShouldHandleEdgeCaseEmails(string email)
        {
            // Arrange
            var request = new LoginRequest { Email = email, Password = "123456" };
            _authRepoMock.Setup(r => r.FindUserByEmail(email))
                         .ReturnsAsync((User)null);

            // Act
            var result = await _authApp.Login(request);

            // Assert
            result.Code.Should().Be("401");
            result.Message.Should().Be("Usuário não autorizado");
            result.Data.Should().BeNull();
        }

        public static IEnumerable<object[]> EdgeCasePasswords()
        {
            yield return new object[] { "" };
            yield return new object[] { " " };
            yield return new object[] { "a" };
            yield return new object[] { new string('a', 1000) }; // Very long password
        }

        [Theory]
        [MemberData(nameof(EdgeCasePasswords))]
        public async Task Login_ShouldHandleEdgeCasePasswords(string password)
        {
            // Arrange
            var validUser = new User
            {
                Id = 1,
                Username = "TestUser",
                Email = "test@email.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword"),
                Roles = new List<Role> { new Role { Id = 1, Name = "User" } }
            };

            var request = new LoginRequest { Email = "test@email.com", Password = password };
            _authRepoMock.Setup(r => r.FindUserByEmail("test@email.com"))
                         .ReturnsAsync(validUser);

            // Act
            var result = await _authApp.Login(request);

            // Assert
            result.Code.Should().Be("401");
            result.Message.Should().Be("Usuário não autorizado");
            result.Data.Should().BeNull();
        }

        #endregion
    }
}