using System;
using System.Globalization;
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
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));
        _loccarApi = config["LoccarApi:BaseUrl"];
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<BaseReturn<string>> LoginAsync(LoginRequest loginRequest)
    {
        if (loginRequest == null)
        {
            throw new ArgumentNullException(nameof(loginRequest));
        }

        BaseReturn<string> baseReturn = new BaseReturn<string>();

        try
        {
            var user = await _authRepository.FindUserByEmail(loginRequest.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
            {
                baseReturn.Code = "401";
                baseReturn.Message = "Usuario nao autorizado";
                return baseReturn;
            }

            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? string.Empty);

            // Corrige o acesso aos roles
            var roles = user.Roles?.Select(n => n.Name) ?? ["CLIENT_USER"];

            var claims = new List<Claim>
            {
                new Claim("name", user.Username),
                new Claim("id", user.Id.ToString(CultureInfo.InvariantCulture)),
            };

            // Adiciona cada role como um claim separado
            foreach (var role in roles)
            {
                claims.Add(new Claim("role", role));
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature));

            baseReturn.Code = "200";
            baseReturn.Message = "Usuario logado com sucesso";
            baseReturn.Data = tokenHandler.WriteToken(token);
        }
        catch (ArgumentException ex)
        {
            baseReturn.Code = "400";
            baseReturn.Message = $"Dados inválidos: {ex.Message}";
        }
        catch (InvalidOperationException ex)
        {
            baseReturn.Code = "500";
            baseReturn.Message = $"Erro de operação: {ex.Message}";
        }
        catch (Exception ex)
        {
            baseReturn.Code = "500";
            baseReturn.Message = $"Ocorreu um erro inesperado: {ex.Message}";
        }

        return baseReturn;
    }

    public async Task<BaseReturn<UserData>> RegisterAsync(RegisterRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

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
                    Roles = new List<Role> { new Role { Id = 1 } }, // Apenas o Id ja basta
                };

                await _authRepository.RegisterUser(user);

                UserData userData = new UserData()
                {
                    Username = request.Username,
                    Email = request.Email,
                    DriverLicense = request.DriverLicense,
                    Cellphone = request.CellPhone,
                };

                var json = JsonSerializer.Serialize(userData);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_loccarApi + "/customer/register", content);

                if (!response.IsSuccessStatusCode)
                {
                    baseReturn.Code = "400";
                    baseReturn.Message = "Erro ao cadastrar locatario";
                    return baseReturn;
                }

                baseReturn.Code = "201";
                baseReturn.Message = "Usuario cadastrado com sucesso!";
                baseReturn.Data = userData;
            }
            else
            {
                baseReturn.Code = "400";
                baseReturn.Message = "Ja existe um usuario com esse email";
            }
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine("Erro: " + ex);
            baseReturn.Code = "400";
            baseReturn.Message = $"Dados inválidos: {ex.Message}";
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine("Erro: " + ex);
            baseReturn.Code = "500";
            baseReturn.Message = $"Erro de operação: {ex.Message}";
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine("Erro: " + ex);
            baseReturn.Code = "502";
            baseReturn.Message = $"Erro de comunicação: {ex.Message}";
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro: " + ex);
            baseReturn.Code = "500";
            baseReturn.Message = $"Ocorreu um erro inesperado: {ex.Message}";
        }

        return baseReturn;
    }

    public async Task<BaseReturn<string>> LogoutAsync()
    {
        BaseReturn<string> baseReturn = new BaseReturn<string>();

        try
        {
            // Com JWT, o logout é feito no cliente removendo o token
            // Este endpoint confirma o logout e pode registrar a ação para auditoria
            
            baseReturn.Code = "200";
            baseReturn.Message = "Logout realizado com sucesso. Remova o token do armazenamento local.";
            baseReturn.Data = "Usuario deslogado";
        }
        catch (Exception ex)
        {
            baseReturn.Code = "500";
            baseReturn.Message = $"Ocorreu um erro inesperado durante o logout: {ex.Message}";
        }

        return await Task.FromResult(baseReturn);
    }
}
