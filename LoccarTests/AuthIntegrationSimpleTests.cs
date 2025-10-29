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
using LoccarInfra.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace LoccarTests
{
    public class AuthIntegrationSimpleTests : IDisposable
    {
        private readonly DataBaseContext _context;
        private readonly IAuthRepository _repository;
        private readonly IConfiguration _configuration;
        private readonly AuthApplication _authApplication;
        private readonly AuthController _controller;

        public AuthIntegrationSimpleTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<DataBaseContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            _context = new DataBaseContext(options);
            _repository = new AuthRepository(_context);

            // Setup configuration
            var inMemorySettings = new Dictionary<string, string> 
            {
                {"Jwt:Key", "TestSecretKey123456789012345678901234567890"},
                {"Jwt:Issuer", "TestIssuer"},
                {"Jwt:Audience", "TestAudience"},
                {"LoccarApi:BaseUrl", "http://localhost:5000/api"}
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            // Setup HTTP client with fake response
            var httpClient = new HttpClient(new FakeHttpMessageHandler(
                new HttpResponseMessage(System.Net.HttpStatusCode.Created)));

            // Create the application layer
            _authApplication = new AuthApplication(_configuration, _repository, httpClient);

            // Create the controller
            _controller = new AuthController(_authApplication);
        }

        #region Integration Register Tests

        [Fact]
        public async Task Register_IntegrationTest_ShouldCreateUserInDatabase()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "integration_test@email.com",
                Username = "IntegrationUser",
                Password = "123456",
                Cnh = "12345678901",
                CellPhone = "61999999999"
            };

            // Act
            var result = await _controller.Register(request);

            // Assert
            result.Should().NotBeNull();
            result.Code.Should().Be("201");
            result.Message.Should().Be("Usuário cadastrado com sucesso!");

            // Verify user was created in database
            var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            userInDb.Should().NotBeNull();
            userInDb.Username.Should().Be(request.Username);
            userInDb.Email.Should().Be(request.Email);
        }

        [Fact]
        public async Task Register_IntegrationTest_ShouldReturn400_WhenUserExists()
        {
            // Arrange - Create user in database first
            var existingUser = new User
            {
                Email = "existing_integration@email.com",
                Username = "ExistingUser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Roles = new List<Role> { new Role { Id = 1, Name = "User" } }
            };

            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();

            var request = new RegisterRequest
            {
                Email = "existing_integration@email.com",
                Username = "AnotherUser", 
                Password = "123456",
                Cnh = "12345678901",
                CellPhone = "61999999999"
            };

            // Act
            var result = await _controller.Register(request);

            // Assert
            result.Should().NotBeNull();
            result.Code.Should().Be("400");
            result.Message.Should().Be("Já existe um usuário com esse email");
            result.Data.Should().BeNull();
        }

        #endregion

        #region Integration Login Tests

        [Fact]
        public async Task Login_IntegrationTest_ShouldReturnToken_WhenCredentialsValid()
        {
            // Arrange - Create user in database
            var password = "login_test_password";
            var user = new User
            {
                Email = "login_integration@email.com",
                Username = "LoginUser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Roles = new List<Role> { new Role { Id = 1, Name = "User" } }
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var request = new LoginRequest
            {
                Email = "login_integration@email.com",
                Password = password
            };

            // Act
            var result = await _controller.Login(request);

            // Assert
            result.Should().NotBeNull();
            result.Code.Should().Be("200");
            result.Message.Should().Be("Usuário logado com sucesso");
            result.Data.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Login_IntegrationTest_ShouldReturn401_WhenUserNotFound()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "nonexistent_integration@email.com",
                Password = "wrongpassword"
            };

            // Act
            var result = await _controller.Login(request);

            // Assert
            result.Should().NotBeNull();
            result.Code.Should().Be("401");
            result.Message.Should().Be("Usuário não autorizado");
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task Login_IntegrationTest_ShouldReturn401_WhenPasswordWrong()
        {
            // Arrange - Create user in database
            var correctPassword = "correct_password";
            var user = new User
            {
                Email = "wrong_password_test@email.com",
                Username = "WrongPasswordUser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(correctPassword),
                Roles = new List<Role> { new Role { Id = 1, Name = "User" } }
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var request = new LoginRequest
            {
                Email = "wrong_password_test@email.com",
                Password = "wrong_password"
            };

            // Act
            var result = await _controller.Login(request);

            // Assert
            result.Should().NotBeNull();
            result.Code.Should().Be("401");
            result.Message.Should().Be("Usuário não autorizado");
            result.Data.Should().BeNull();
        }

        #endregion

        #region Parameterized Integration Tests

        public static IEnumerable<object[]> ValidLoginDataForIntegration()
        {
            yield return new object[]
            {
                "integration1@email.com",
                "User1",
                "password123",
                "200",
                "Usuário logado com sucesso"
            };

            yield return new object[]
            {
                "integration2@email.com", 
                "User2",
                "mySecretPass",
                "200",
                "Usuário logado com sucesso"
            };

            yield return new object[]
            {
                "integration3@domain.com",
                "AdminUser",
                "admin@2024",
                "200",
                "Usuário logado com sucesso"
            };
        }

        [Theory]
        [MemberData(nameof(ValidLoginDataForIntegration))]
        public async Task Login_Integration_ShouldReturnExpectedResults(
            string email, string username, string password, string expectedCode, string expectedMessage)
        {
            // Arrange - Create user in database
            var user = new User
            {
                Email = email,
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Roles = new List<Role> { new Role { Id = 1, Name = "User" } }
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var request = new LoginRequest
            {
                Email = email,
                Password = password
            };

            // Act
            var result = await _controller.Login(request);

            // Assert
            result.Should().NotBeNull();
            result.Code.Should().Be(expectedCode);
            result.Message.Should().Be(expectedMessage);
            
            if (expectedCode == "200")
            {
                result.Data.Should().NotBeNullOrEmpty();
            }
        }

        public static IEnumerable<object[]> InvalidLoginDataForIntegration()
        {
            yield return new object[]
            {
                "nonexistent1@email.com",
                "password123",
                "401",
                "Usuário não autorizado"
            };

            yield return new object[]
            {
                "nonexistent2@domain.com",
                "wrongpass",
                "401", 
                "Usuário não autorizado"
            };

            yield return new object[]
            {
                "fake@email.com",
                "",
                "401",
                "Usuário não autorizado"
            };
        }

        [Theory]
        [MemberData(nameof(InvalidLoginDataForIntegration))]
        public async Task Login_Integration_ShouldReturnUnauthorized_WithInvalidData(
            string email, string password, string expectedCode, string expectedMessage)
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = email,
                Password = password
            };

            // Act
            var result = await _controller.Login(request);

            // Assert
            result.Should().NotBeNull();
            result.Code.Should().Be(expectedCode);
            result.Message.Should().Be(expectedMessage);
            result.Data.Should().BeNull();
        }

        #endregion

        #region End-to-End Workflow Tests

        [Fact]
        public async Task CompleteAuthFlow_IntegrationTest_RegisterThenLogin()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "workflow_test@email.com",
                Username = "WorkflowUser",
                Password = "workflow123",
                Cnh = "12345678901",
                CellPhone = "61999999999"
            };

            // Act 1 - Register
            var registerResult = await _controller.Register(registerRequest);

            // Assert 1
            registerResult.Should().NotBeNull();
            registerResult.Code.Should().Be("201");

            // Act 2 - Login
            var loginRequest = new LoginRequest
            {
                Email = registerRequest.Email,
                Password = registerRequest.Password
            };

            var loginResult = await _controller.Login(loginRequest);

            // Assert 2
            loginResult.Should().NotBeNull();
            loginResult.Code.Should().Be("200");
            loginResult.Message.Should().Be("Usuário logado com sucesso");
            loginResult.Data.Should().NotBeNullOrEmpty();

            // Verify user exists in database
            var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Email == registerRequest.Email);
            userInDb.Should().NotBeNull();
            userInDb.Username.Should().Be(registerRequest.Username);
        }

        #endregion

        #region Database State Tests

        [Fact]
        public async Task Register_IntegrationTest_ShouldNotAffectOtherUsers()
        {
            // Arrange - Create existing user
            var existingUser = new User
            {
                Email = "existing@email.com",
                Username = "Existing",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("existingpass")
            };

            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();

            var newRequest = new RegisterRequest
            {
                Email = "new@email.com",
                Username = "New",
                Password = "newpass",
                Cnh = "12345678901",
                CellPhone = "61999999999"
            };

            // Act
            await _controller.Register(newRequest);

            // Assert
            var users = await _context.Users.OrderBy(u => u.Email).ToListAsync();
            users.Should().HaveCount(2);

            var existing = users.First(u => u.Email == "existing@email.com");
            existing.Username.Should().Be("Existing");
            existing.PasswordHash.Should().NotBe("existingpass"); // Should still be hashed

            var added = users.First(u => u.Email == "new@email.com");
            added.Username.Should().Be("New");
        }

        [Fact]
        public async Task Login_IntegrationTest_ShouldNotModifyDatabase()
        {
            // Arrange
            var user = new User
            {
                Id = 999,
                Email = "test@email.com",
                Username = "TestUser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("testpass"),
                Roles = new List<Role> { new Role { Id = 1, Name = "User" } }
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var originalHash = user.PasswordHash;
            var originalUsername = user.Username;

            var request = new LoginRequest
            {
                Email = "test@email.com",
                Password = "testpass"
            };

            // Act
            await _controller.Login(request);

            // Assert - User data should remain unchanged
            var userAfterLogin = await _context.Users.FindAsync(999);
            userAfterLogin.Should().NotBeNull();
            userAfterLogin.PasswordHash.Should().Be(originalHash);
            userAfterLogin.Username.Should().Be(originalUsername);
        }

        #endregion

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}