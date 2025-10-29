using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using LoccarApplication.Interfaces;
using LoccarDomain.Common;
using LoccarDomain.Login;
using LoccarDomain.Register;
using LoccarDomain.User;
using Microsoft.AspNetCore.Mvc;
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

        #region Login Controller Tests

        [Fact]
        public async Task Login_ShouldReturnExpectedResult_WhenApplicationReturnsSuccess()
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

        public static IEnumerable<object[]> LoginTestData()
        {
            yield return new object[]
            {
                new LoginRequest { Email = "user1@email.com", Password = "pass1" },
                new BaseReturn<string> { Code = "200", Message = "Success", Data = "token1" }
            };

            yield return new object[]
            {
                new LoginRequest { Email = "user2@email.com", Password = "pass2" },
                new BaseReturn<string> { Code = "401", Message = "Unauthorized", Data = null }
            };

            yield return new object[]
            {
                new LoginRequest { Email = "user3@email.com", Password = "pass3" },
                new BaseReturn<string> { Code = "500", Message = "Internal Error", Data = null }
            };
        }

        [Theory]
        [MemberData(nameof(LoginTestData))]
        public async Task Login_ShouldPassThroughApplicationResult(
            LoginRequest request, BaseReturn<string> expectedResult)
        {
            // Arrange
            _authApplicationMock.Setup(app => app.Login(request))
                               .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.Login(request);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            _authApplicationMock.Verify(app => app.Login(request), Times.Once);
        }

        #endregion

        #region Register Controller Tests

        [Fact]
        public async Task Register_ShouldReturnExpectedResult_WhenApplicationReturnsSuccess()
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

            var expectedUserData = new UserData
            {
                Email = request.Email,
                Username = request.Username,
                Cnh = request.Cnh,
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
                Cnh = "12345678901",
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

        public static IEnumerable<object[]> RegisterTestData()
        {
            yield return new object[]
            {
                new RegisterRequest 
                { 
                    Email = "user1@email.com", 
                    Username = "User1", 
                    Password = "pass1",
                    Cnh = "11111111111",
                    CellPhone = "61111111111"
                },
                new BaseReturn<UserData> 
                { 
                    Code = "201", 
                    Message = "Success", 
                    Data = new UserData 
                    { 
                        Email = "user1@email.com", 
                        Username = "User1",
                        Cnh = "11111111111",
                        Cellphone = "61111111111"
                    } 
                }
            };

            yield return new object[]
            {
                new RegisterRequest 
                { 
                    Email = "existing@email.com", 
                    Username = "User2", 
                    Password = "pass2",
                    Cnh = "22222222222",
                    CellPhone = "62222222222"
                },
                new BaseReturn<UserData> 
                { 
                    Code = "400", 
                    Message = "User exists", 
                    Data = null 
                }
            };

            yield return new object[]
            {
                new RegisterRequest 
                { 
                    Email = "error@email.com", 
                    Username = "User3", 
                    Password = "pass3",
                    Cnh = "33333333333",
                    CellPhone = "63333333333"
                },
                new BaseReturn<UserData> 
                { 
                    Code = "500", 
                    Message = "Internal Error", 
                    Data = null 
                }
            };
        }

        [Theory]
        [MemberData(nameof(RegisterTestData))]
        public async Task Register_ShouldPassThroughApplicationResult(
            RegisterRequest request, BaseReturn<UserData> expectedResult)
        {
            // Arrange
            _authApplicationMock.Setup(app => app.Register(request))
                               .ReturnsAsync(expectedResult);

            // Act  
            var result = await _controller.Register(request);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            _authApplicationMock.Verify(app => app.Register(request), Times.Once);
        }

        #endregion

        #region Controller Specific Tests

        [Fact]
        public void Constructor_ShouldInitializeWithAuthApplication()
        {
            // Arrange & Act
            var controller = new AuthController(_authApplicationMock.Object);

            // Assert
            controller.Should().NotBeNull();
        }

        [Fact]
        public async Task Login_ShouldNotModifyRequest()
        {
            // Arrange
            var originalRequest = new LoginRequest
            {
                Email = "test@email.com",
                Password = "123456"
            };

            var requestCopy = new LoginRequest
            {
                Email = originalRequest.Email,
                Password = originalRequest.Password
            };

            _authApplicationMock.Setup(app => app.Login(It.IsAny<LoginRequest>()))
                               .ReturnsAsync(new BaseReturn<string> { Code = "200" });

            // Act
            await _controller.Login(originalRequest);

            // Assert
            originalRequest.Email.Should().Be(requestCopy.Email);
            originalRequest.Password.Should().Be(requestCopy.Password);
        }

        [Fact]
        public async Task Register_ShouldNotModifyRequest()
        {
            // Arrange
            var originalRequest = new RegisterRequest
            {
                Email = "test@email.com",
                Username = "TestUser",
                Password = "123456",
                Cnh = "12345678901",
                CellPhone = "61999999999"
            };

            var requestCopy = new RegisterRequest
            {
                Email = originalRequest.Email,
                Username = originalRequest.Username,
                Password = originalRequest.Password,
                Cnh = originalRequest.Cnh,
                CellPhone = originalRequest.CellPhone
            };

            _authApplicationMock.Setup(app => app.Register(It.IsAny<RegisterRequest>()))
                               .ReturnsAsync(new BaseReturn<UserData> { Code = "201" });

            // Act
            await _controller.Register(originalRequest);

            // Assert
            originalRequest.Should().BeEquivalentTo(requestCopy);
        }

        #endregion

        #region Exception Handling Tests

        [Fact]
        public async Task Login_ShouldPropagateException_WhenApplicationThrows()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "test@email.com",
                Password = "123456"
            };

            _authApplicationMock.Setup(app => app.Login(request))
                               .ThrowsAsync(new System.Exception("Application error"));

            // Act & Assert
            await Assert.ThrowsAsync<System.Exception>(() => _controller.Login(request));
        }

        [Fact]
        public async Task Register_ShouldPropagateException_WhenApplicationThrows()
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

            _authApplicationMock.Setup(app => app.Register(request))
                               .ThrowsAsync(new System.Exception("Application error"));

            // Act & Assert
            await Assert.ThrowsAsync<System.Exception>(() => _controller.Register(request));
        }

        #endregion
    }
}