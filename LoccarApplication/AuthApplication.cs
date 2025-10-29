using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LoccarApplication.Interfaces;
using LoccarDomain.Common;
using LoccarDomain.Login;
using LoccarDomain.Register;
using LoccarDomain.User;
using LoccarInfra.Interfaces;
using LoccarInfra.ORM.model;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;


public class AuthApplication : IAuthApplication
{
    private readonly IConfiguration _config;
    private readonly IAuthRepository _authRepository;
    private readonly string? _loccarApi;
    private readonly HttpClient _httpClient;

    public AuthApplication(IConfiguration config, IAuthRepository authRepository, HttpClient httpClient)
    {
        _config = config;
        _authRepository = authRepository;
        _loccarApi = config["LoccarApi:BaseUrl"];
        _httpClient = httpClient;
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

            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? string.Empty);

            // Corrige o acesso aos roles
            var roles = user.Roles?.FirstOrDefault()?.Name ?? "User";

            var claims = new[]
            {
                new KeyValuePair<string, string>("name", user.Username),
                new KeyValuePair<string, string>("id", user.Id.ToString()),
                new KeyValuePair<string, string>("role", roles)
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
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Roles = new List<Role> { new Role { Id = 1 } } // Apenas o Id já basta
                };

                await _authRepository.RegisterUser(user);

                UserData userData = new UserData()
                {
                    Username = request.Username,
                    Email = request.Email,
                    Cnh = request.Cnh,
                    Cellphone = request.CellPhone
                };

                var json = JsonSerializer.Serialize(userData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_loccarApi + "/locatario/register", content);

                if (!response.IsSuccessStatusCode)
                {
                    baseReturn.Code = "400";
                    baseReturn.Message = "Erro ao cadastrar locatário";
                    return baseReturn;
                }

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

            Console.WriteLine("Erro: " + ex);
            baseReturn.Code = "500";
            baseReturn.Message = $"Ocorreu um erro inesperado: {ex.Message}";
        }
        return baseReturn;
    }
}
