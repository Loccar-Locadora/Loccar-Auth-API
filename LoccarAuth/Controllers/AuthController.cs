using LoccarApplication.Interfaces;
using LoccarDomain.Common;
using LoccarDomain.Login;
using LoccarDomain.Register;
using LoccarDomain.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthApplication _authApplication;
    public AuthController(IAuthApplication authApplication)
    {
        _authApplication = authApplication;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<BaseReturn<string>> Login(LoginRequest request)
    {
        return await _authApplication.Login(request);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<BaseReturn<UserData>> Register(RegisterRequest request)
    {
        return await _authApplication.Register(request);
    }
}
