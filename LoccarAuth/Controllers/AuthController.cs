using LoccarApplication.Interfaces;
using LoccarDomain.Common;
using LoccarDomain.Login;
using LoccarDomain.Register;
using LoccarDomain.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoccarAuth.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthApplication _authApplication;

        public AuthController(IAuthApplication authApplication)
        {
            _authApplication = authApplication ?? throw new ArgumentNullException(nameof(authApplication));
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<BaseReturn<string>> LoginAsync(LoginRequest request)
        {
            return await _authApplication.LoginAsync(request);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<BaseReturn<UserData>> RegisterAsync(RegisterRequest request)
        {
            return await _authApplication.RegisterAsync(request);
        }

        [HttpPost("logout")]
        public async Task<BaseReturn<string>> LogoutAsync()
        {
            return await _authApplication.LogoutAsync();
        }
    }
}
