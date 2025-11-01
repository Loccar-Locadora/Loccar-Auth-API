using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using LoccarInfra.ORM.model;
using LoccarInfra.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LoccarTests
{
    public sealed class AuthRepositoryIntegrationTests : IDisposable
    {
        private readonly DataBaseContext _context;
        private readonly AuthRepository _repository;
        private bool _disposed;

        public AuthRepositoryIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<DataBaseContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            _context = new DataBaseContext(options);
            _repository = new AuthRepository(_context);
        }

        [Fact]
        public async Task FindUserByEmailShouldReturnUserWhenUserExistsAsync()
        {
            // Arrange
            var user = new User
            {
                Email = "test@email.com",
                Username = "TestUser",
                PasswordHash = "hashedpassword",
                IsActive = true,
                Roles = new List<Role>(),
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.FindUserByEmail("test@email.com");

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be("test@email.com");
            result.Username.Should().Be("TestUser");
        }

        [Fact]
        public async Task FindUserByEmailShouldReturnNullWhenUserDoesNotExistAsync()
        {
            // Act
            var result = await _repository.FindUserByEmail("nonexistent@email.com");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task FindUserByEmailShouldReturnNullWhenEmailIsEmptyAsync()
        {
            // Act
            var result = await _repository.FindUserByEmail(string.Empty);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task RegisterUserShouldAddUserToDatabaseAsync()
        {
            // Arrange
            var user = new User
            {
                Email = "newuser@email.com",
                Username = "NewUser",
                PasswordHash = "hashedpassword",
                Roles = new List<Role>(),
            };

            // Act
            await _repository.RegisterUser(user);

            // Assert
            var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "newuser@email.com");
            savedUser.Should().NotBeNull();
            savedUser!.Email.Should().Be("newuser@email.com");
            savedUser.Username.Should().Be("NewUser");
            savedUser.IsActive.Should().Be(true);
        }

        [Fact]
        public async Task RegisterUserShouldGenerateIdAsync()
        {
            // Arrange
            var user = new User
            {
                Email = "newuser@email.com",
                Username = "NewUser",
                PasswordHash = "hashedpassword",
            };

            // Act
            await _repository.RegisterUser(user);

            // Assert
            var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "newuser@email.com");
            savedUser.Should().NotBeNull();
            savedUser!.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CompleteWorkflowRegisterThenFindShouldWorkAsync()
        {
            // Arrange
            var user = new User
            {
                Email = "workflow@email.com",
                Username = "WorkflowUser",
                PasswordHash = "workflowhash",
            };

            // Act - Register
            await _repository.RegisterUser(user);

            // Act - Find
            var foundUser = await _repository.FindUserByEmail("workflow@email.com");

            // Assert
            foundUser.Should().NotBeNull();
            foundUser!.Email.Should().Be("workflow@email.com");
            foundUser.Username.Should().Be("WorkflowUser");
            foundUser.PasswordHash.Should().Be("workflowhash");
            foundUser.Id.Should().BeGreaterThan(0);
            foundUser.IsActive.Should().Be(true);
        }

        [Fact]
        public async Task RegisterMultipleUsersShouldWorkAsync()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Email = "user1@email.com", Username = "User1", PasswordHash = "hash1" },
                new User { Email = "user2@email.com", Username = "User2", PasswordHash = "hash2" },
                new User { Email = "user3@email.com", Username = "User3", PasswordHash = "hash3" },
            };

            // Act
            foreach (var user in users)
            {
                await _repository.RegisterUser(user);
            }

            // Assert
            var savedUsers = await _context.Users.ToListAsync();
            savedUsers.Should().HaveCount(3);
            savedUsers.Should().OnlyContain(u => u.IsActive == true);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _context?.Dispose();
                _disposed = true;
            }
        }
    }
}
