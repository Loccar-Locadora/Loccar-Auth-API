using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LoccarDomain.Register;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using LoccarDomain.User;
using LoccarApplication.Interfaces;
using LoccarDomain.Login;
using LoccarInfra.Interfaces;
using LoccarInfra.ORM.model;
using System.Threading.Tasks;
using LoccarDomain.Common;


public class AuthApplication : IAuthApplication
{
    private readonly IConfiguration _config;
    private readonly IAuthRepository _authRepository;

    public AuthApplication(IConfiguration config, IAuthRepository authRepository)
    {
        _config = config;
        _authRepository = authRepository;
    }

    public async Task<BaseReturn<string>> Login(LoginRequest loginRequest)
    {
        BaseReturn<string> baseReturn = new BaseReturn<string>();

        try
        {
            var user = await _authRepository.FindUserByEmail(loginRequest.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
            {
                baseReturn.Code = "401";
                baseReturn.Message = "Usuário não autorizado";
                return baseReturn;
            }

            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

            var claims = new[]
            {
                new KeyValuePair<string, string>("name", user.Username),
                new KeyValuePair<string, string>("id", user.Id.ToString())
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims.Select(c => new System.Security.Claims.Claim(c.Key, c.Value)),
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            );

            if (token == null)
            {
                baseReturn.Code = "401";
                baseReturn.Message = "Usuário não autorizado";
                return baseReturn;
            }

            baseReturn.Code = "200";
            baseReturn.Message = "Usuário logado com sucesso";
            baseReturn.Data = tokenHandler.WriteToken(token);
        }
        catch (Exception ex)
        {
            baseReturn.Code = "500";
            baseReturn.Message = $"Ocorreu um erro inesperado: {ex.Message}";
        }
        return baseReturn;
    }

    public async Task<BaseReturn<UserData>> Register(RegisterRequest request)
    {
        BaseReturn<UserData> baseReturn = new BaseReturn<UserData>();
        try
        {
            User tbUser = await _authRepository.FindUserByEmail(request.Email);
            if (tbUser == null)
            {
                var user = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
                };
                await _authRepository.RegisterUser(user);
                UserData userData = new UserData()
                {
                    Username = request.Username,
                    Email = request.Email
                };
                baseReturn.Code = "201";
                baseReturn.Message = "Usuário cadastrado com sucesso!";
                baseReturn.Data = userData;
            }
            else
            {
                baseReturn.Code = "400";
                baseReturn.Message = "Já existe um usuário com esse email";
            }
        } 
        catch (Exception ex) 
        {
            baseReturn.Code = "500";
            baseReturn.Message = $"Ocorreu um erro inesperado: {ex.Message}";
        }
        return baseReturn;
    }
}
