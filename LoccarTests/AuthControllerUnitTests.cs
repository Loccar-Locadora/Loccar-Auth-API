using System.Threading.Tasks;
using FluentAssertions;
using LoccarApplication.Interfaces;
using LoccarAuth.Controllers;
using LoccarDomain.Common;
using LoccarDomain.Login;
using LoccarDomain.Register;
using LoccarDomain.User;
using Moq;
using Xunit;

namespace LoccarTests
{
    public class AuthControllerUnitTests
    {
        private readonly Mock<IAuthApplication> _authApplicationMock;
        private readonly AuthController _controller;

        public AuthControllerUnitTests()
        {
            _authApplicationMock = new Mock<IAuthApplication>();
            _controller = new AuthController(_authApplicationMock.Object);
        }

        #region Login Tests

        [Fact]
        public async Task Login_ShouldReturnSuccess_WhenApplicationReturnsSuccess()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "test@email.com",
                Password = "123456"
            };

            var expectedResult = new BaseReturn<string>
            {
                Code = "200",
                Message = "Usuario logado com sucesso",
                Data = "fake-jwt-token"
            };

            _authApplicationMock.Setup(app => app.Login(request))
                               .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.Login(request);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            _authApplicationMock.Verify(app => app.Login(request), Times.Once);
        }

        [Fact]
        public async Task Login_ShouldReturnError_WhenApplicationReturnsError()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "test@email.com",
                Password = "wrongpassword"
            };

            var expectedResult = new BaseReturn<string>
            {
                Code = "401",
                Message = "Usuario nao autorizado",
                Data = null
            };

            _authApplicationMock.Setup(app => app.Login(request))
                               .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.Login(request);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            _authApplicationMock.Verify(app => app.Login(request), Times.Once);
        }

        #endregion

        #region Register Tests

        [Fact]
        public async Task Register_ShouldReturnSuccess_WhenApplicationReturnsSuccess()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "test@email.com",
                Username = "TestUser",
                Password = "123456",
                DriverLicense = "12345678901",
                CellPhone = "61999999999"
            };

            var expectedUserData = new UserData
            {
                Email = request.Email,
                Username = request.Username,
                DriverLicense = request.DriverLicense,
                Cellphone = request.CellPhone
            };

            var expectedResult = new BaseReturn<UserData>
            {
                Code = "201",
                Message = "Usuario cadastrado com sucesso!",
                Data = expectedUserData
            };

            _authApplicationMock.Setup(app => app.Register(request))
                               .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.Register(request);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            _authApplicationMock.Verify(app => app.Register(request), Times.Once);
        }

        [Fact]
        public async Task Register_ShouldReturnError_WhenApplicationReturnsError()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "existing@email.com",
                Username = "TestUser",
                Password = "123456",
                DriverLicense = "12345678901",
                CellPhone = "61999999999"
            };

            var expectedResult = new BaseReturn<UserData>
            {
                Code = "400",
                Message = "Ja existe um usuario com esse email",
                Data = null
            };

            _authApplicationMock.Setup(app => app.Register(request))
                               .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.Register(request);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            _authApplicationMock.Verify(app => app.Register(request), Times.Once);
        }

        #endregion

        #region Basic Functionality Tests

        [Fact]
        public void Constructor_ShouldInitializeCorrectly()
        {
            // Assert
            _controller.Should().NotBeNull();
        }

        [Fact]
        public async Task Login_ShouldPassRequestThrough()
        {
            // Arrange
            var request = new LoginRequest { Email = "test@email.com", Password = "123456" };
            _authApplicationMock.Setup(app => app.Login(It.IsAny<LoginRequest>()))
                               .ReturnsAsync(new BaseReturn<string> { Code = "200" });

            // Act
            await _controller.Login(request);

            // Assert
            _authApplicationMock.Verify(app => app.Login(request), Times.Once);
        }

        [Fact]
        public async Task Register_ShouldPassRequestThrough()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "test@email.com",
                Username = "TestUser",
                Password = "123456",
                DriverLicense = "12345678901",
                CellPhone = "61999999999"
            };
            _authApplicationMock.Setup(app => app.Register(It.IsAny<RegisterRequest>()))
                               .ReturnsAsync(new BaseReturn<UserData> { Code = "201" });

            // Act
            await _controller.Register(request);

            // Assert
            _authApplicationMock.Verify(app => app.Register(request), Times.Once);
        }

        #endregion
    }
}
