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

public class AuthApplicationTests
{
    private readonly Mock<IAuthRepository> _authRepoMock;
    private readonly IConfiguration _configuration;
    private readonly AuthApplication _authApp;

    public AuthApplicationTests()
    {
        _authRepoMock = new Mock<IAuthRepository>();

        var inMemorySettings = new Dictionary<string, string> {
            {"Jwt:Key", "MinhaChaveSecretaSuperSegura1234567890"},
            {"Jwt:Issuer", "Loccar"},
            {"Jwt:Audience", "Loccar"}
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _authApp = new AuthApplication(_configuration, _authRepoMock.Object);
    }

    #region Register Tests

    [Fact]
    public async Task Register_ShouldReturn201_WhenUserDoesNotExist()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "teste@email.com",
            Username = "Teste",
            Password = "123456"
        };

        _authRepoMock.Setup(r => r.FindUserByEmail(request.Email))
                     .ReturnsAsync((User)null);

        _authRepoMock.Setup(r => r.RegisterUser(It.IsAny<User>()))
                     .Returns(Task.CompletedTask);

        // Act
        var result = await _authApp.Register(request);

        // Assert
        result.Code.Should().Be("201");
        result.Message.Should().Be("Usuário cadastrado com sucesso!");
        result.Data.Username.Should().Be("Teste");
        result.Data.Email.Should().Be("teste@email.com");
    }

    [Fact]
    public async Task Register_ShouldReturn400_WhenUserAlreadyExists()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "teste@email.com",
            Username = "Teste",
            Password = "123456"
        };

        _authRepoMock.Setup(r => r.FindUserByEmail(request.Email))
                     .ReturnsAsync(new User { Email = request.Email });

        // Act
        var result = await _authApp.Register(request);

        // Assert
        result.Code.Should().Be("400");
        result.Message.Should().Be("Já existe um usuário com esse email");
        result.Data.Should().BeNull();
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_ShouldReturn200_WithToken_WhenCredentialsAreCorrect()
    {
        // Arrange
        var password = "123456";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        _authRepoMock.Setup(r => r.FindUserByEmail("teste@email.com"))
                     .ReturnsAsync(new User { Id = 1, Username = "Teste", Email = "teste@email.com", PasswordHash = hashedPassword });

        var request = new LoginRequest
        {
            Email = "teste@email.com",
            Password = password
        };

        // Act
        var result = await _authApp.Login(request);

        // Assert
        result.Code.Should().Be("200");
        result.Message.Should().Be("Usuário logado com sucesso");
        result.Data.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_ShouldReturn401_WhenUserDoesNotExist()
    {
        // Arrange
        _authRepoMock.Setup(r => r.FindUserByEmail("teste@email.com"))
                     .ReturnsAsync((User)null);

        var request = new LoginRequest
        {
            Email = "teste@email.com",
            Password = "123456"
        };

        // Act
        var result = await _authApp.Login(request);

        // Assert
        result.Code.Should().Be("401");
        result.Message.Should().Be("Usuário não autorizado");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Login_ShouldReturn401_WhenPasswordIsIncorrect()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("senha_correta");

        _authRepoMock.Setup(r => r.FindUserByEmail("teste@email.com"))
                     .ReturnsAsync(new User { Id = 1, Username = "Teste", Email = "teste@email.com", PasswordHash = hashedPassword });

        var request = new LoginRequest
        {
            Email = "teste@email.com",
            Password = "senha_errada"
        };

        // Act
        var result = await _authApp.Login(request);

        // Assert
        result.Code.Should().Be("401");
        result.Message.Should().Be("Usuário não autorizado");
        result.Data.Should().BeNull();
    }

    #endregion
}
