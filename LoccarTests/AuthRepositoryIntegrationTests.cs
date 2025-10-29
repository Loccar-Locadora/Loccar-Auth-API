using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LoccarInfra.ORM.model;
using LoccarInfra.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LoccarTests
{
    public class AuthRepositoryIntegrationTests : IDisposable
    {
        private readonly DataBaseContext _context;
        private readonly AuthRepository _repository;

        public AuthRepositoryIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<DataBaseContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            _context = new DataBaseContext(options);
            _repository = new AuthRepository(_context);
        }

        #region FindUserByEmail Tests

        [Fact]
        public async Task FindUserByEmail_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var user = new User
            {
                Email = "test@email.com",
                Username = "TestUser",
                PasswordHash = "hashedpassword",
                Roles = new List<Role> { new Role { Id = 1, Name = "User" } }
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.FindUserByEmail("test@email.com");

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be("test@email.com");
            result.Username.Should().Be("TestUser");
            result.PasswordHash.Should().Be("hashedpassword");
        }

        [Fact]
        public async Task FindUserByEmail_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Act
            var result = await _repository.FindUserByEmail("nonexistent@email.com");

            // Assert
            result.Should().BeNull();
        }

        public static IEnumerable<object[]> EmailTestData()
        {
            yield return new object[] { "user1@domain.com", "User1", "hash1" };
            yield return new object[] { "user2@domain.com", "User2", "hash2" };
            yield return new object[] { "admin@company.org", "Admin", "hash3" };
            yield return new object[] { "test.email+tag@example.co.uk", "TestUser", "hash4" };
        }

        [Theory]
        [MemberData(nameof(EmailTestData))]
        public async Task FindUserByEmail_ShouldFindCorrectUser_WithVariousEmails(
            string email, string username, string passwordHash)
        {
            // Arrange
            var users = new List<User>
            {
                new User { Email = "other1@email.com", Username = "Other1", PasswordHash = "otherhash1" },
                new User { Email = email, Username = username, PasswordHash = passwordHash },
                new User { Email = "other2@email.com", Username = "Other2", PasswordHash = "otherhash2" }
            };

            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.FindUserByEmail(email);

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be(email);
            result.Username.Should().Be(username);
            result.PasswordHash.Should().Be(passwordHash);
        }

        [Fact]
        public async Task FindUserByEmail_ShouldBeCaseInsensitive()
        {
            // Arrange
            var user = new User
            {
                Email = "Test@Email.COM",
                Username = "TestUser",
                PasswordHash = "hashedpassword"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result1 = await _repository.FindUserByEmail("test@email.com");
            var result2 = await _repository.FindUserByEmail("TEST@EMAIL.COM");
            var result3 = await _repository.FindUserByEmail("Test@Email.COM");

            // Assert
            result1.Should().NotBeNull();
            result2.Should().NotBeNull();
            result3.Should().NotBeNull();
            
            result1.Email.Should().Be("Test@Email.COM");
            result2.Email.Should().Be("Test@Email.COM");
            result3.Email.Should().Be("Test@Email.COM");
        }

        #endregion

        #region RegisterUser Tests

        [Fact]
        public async Task RegisterUser_ShouldAddUserToDatabase()
        {
            // Arrange
            var user = new User
            {
                Email = "newuser@email.com",
                Username = "NewUser",
                PasswordHash = "hashedpassword",
                Roles = new List<Role> { new Role { Id = 1, Name = "User" } }
            };

            // Act
            await _repository.RegisterUser(user);

            // Assert
            var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "newuser@email.com");
            savedUser.Should().NotBeNull();
            savedUser.Email.Should().Be("newuser@email.com");
            savedUser.Username.Should().Be("NewUser");
            savedUser.PasswordHash.Should().Be("hashedpassword");
        }

        [Fact]
        public async Task RegisterUser_ShouldGenerateId_WhenUserIsAdded()
        {
            // Arrange
            var user = new User
            {
                Email = "newuser@email.com",
                Username = "NewUser",
                PasswordHash = "hashedpassword"
            };

            // Act
            await _repository.RegisterUser(user);

            // Assert
            var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "newuser@email.com");
            savedUser.Should().NotBeNull();
            savedUser.Id.Should().BeGreaterThan(0);
        }

        public static IEnumerable<object[]> RegisterUserTestData()
        {
            yield return new object[] 
            { 
                new User 
                { 
                    Email = "test1@email.com", 
                    Username = "Test1", 
                    PasswordHash = "hash1",
                    Roles = new List<Role> { new Role { Id = 1, Name = "User" } }
                } 
            };
            
            yield return new object[] 
            { 
                new User 
                { 
                    Email = "test2@domain.org", 
                    Username = "Test2", 
                    PasswordHash = "hash2",
                    Roles = new List<Role> { new Role { Id = 2, Name = "Admin" } }
                } 
            };
            
            yield return new object[] 
            { 
                new User 
                { 
                    Email = "admin@company.com", 
                    Username = "AdminUser", 
                    PasswordHash = "adminhash",
                    Roles = new List<Role> { new Role { Id = 1, Name = "User" }, new Role { Id = 2, Name = "Admin" } }
                } 
            };
        }

        [Theory]
        [MemberData(nameof(RegisterUserTestData))]
        public async Task RegisterUser_ShouldSaveUserCorrectly_WithVariousData(User user)
        {
            // Act
            await _repository.RegisterUser(user);

            // Assert
            var savedUser = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Email == user.Email);
                
            savedUser.Should().NotBeNull();
            savedUser.Email.Should().Be(user.Email);
            savedUser.Username.Should().Be(user.Username);
            savedUser.PasswordHash.Should().Be(user.PasswordHash);
            
            if (user.Roles != null)
            {
                savedUser.Roles.Should().HaveCount(user.Roles.Count);
            }
        }

        [Fact]
        public async Task RegisterUser_ShouldHandleMultipleUsers()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Email = "user1@email.com", Username = "User1", PasswordHash = "hash1" },
                new User { Email = "user2@email.com", Username = "User2", PasswordHash = "hash2" },
                new User { Email = "user3@email.com", Username = "User3", PasswordHash = "hash3" }
            };

            // Act
            foreach (var user in users)
            {
                await _repository.RegisterUser(user);
            }

            // Assert
            var savedUsers = await _context.Users.OrderBy(u => u.Email).ToListAsync();
            savedUsers.Should().HaveCount(3);
            
            for (int i = 0; i < users.Count; i++)
            {
                savedUsers[i].Email.Should().Be(users[i].Email);
                savedUsers[i].Username.Should().Be(users[i].Username);
                savedUsers[i].PasswordHash.Should().Be(users[i].PasswordHash);
            }
        }

        #endregion

        #region Integration Workflow Tests

        [Fact]
        public async Task CompleteWorkflow_ShouldWork_RegisterThenFind()
        {
            // Arrange
            var user = new User
            {
                Email = "workflow@email.com",
                Username = "WorkflowUser",
                PasswordHash = "workflowhash",
                Roles = new List<Role> { new Role { Id = 1, Name = "User" } }
            };

            // Act - Register
            await _repository.RegisterUser(user);

            // Act - Find
            var foundUser = await _repository.FindUserByEmail("workflow@email.com");

            // Assert
            foundUser.Should().NotBeNull();
            foundUser.Email.Should().Be("workflow@email.com");
            foundUser.Username.Should().Be("WorkflowUser");
            foundUser.PasswordHash.Should().Be("workflowhash");
            foundUser.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task RegisterUser_ShouldNotAffectExistingUsers()
        {
            // Arrange
            var existingUser = new User
            {
                Email = "existing@email.com",
                Username = "Existing",
                PasswordHash = "existinghash"
            };

            var newUser = new User
            {
                Email = "new@email.com",
                Username = "New",
                PasswordHash = "newhash"
            };

            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();

            // Act
            await _repository.RegisterUser(newUser);

            // Assert
            var users = await _context.Users.OrderBy(u => u.Email).ToListAsync();
            users.Should().HaveCount(2);

            var existing = users.First(u => u.Email == "existing@email.com");
            existing.Username.Should().Be("Existing");
            existing.PasswordHash.Should().Be("existinghash");

            var added = users.First(u => u.Email == "new@email.com");
            added.Username.Should().Be("New");
            added.PasswordHash.Should().Be("newhash");
        }

        #endregion

        #region Edge Cases and Error Handling

        [Fact]
        public async Task FindUserByEmail_ShouldHandleEmptyString()
        {
            // Act
            var result = await _repository.FindUserByEmail("");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task FindUserByEmail_ShouldHandleWhitespace()
        {
            // Act
            var result = await _repository.FindUserByEmail("   ");

            // Assert
            result.Should().BeNull();
        }

        [Theory]
        [InlineData(null)]
        public async Task FindUserByEmail_ShouldReturnNull_WhenEmailIsNull(string email)
        {
            // Act
            var result = await _repository.FindUserByEmail(email);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task RegisterUser_ShouldHandleUserWithMinimalData()
        {
            // Arrange
            var user = new User
            {
                Email = "minimal@email.com",
                Username = "Minimal",
                PasswordHash = "hash"
                // No roles
            };

            // Act
            await _repository.RegisterUser(user);

            // Assert
            var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "minimal@email.com");
            savedUser.Should().NotBeNull();
            savedUser.Email.Should().Be("minimal@email.com");
            savedUser.Username.Should().Be("Minimal");
            savedUser.PasswordHash.Should().Be("hash");
        }

        #endregion

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}