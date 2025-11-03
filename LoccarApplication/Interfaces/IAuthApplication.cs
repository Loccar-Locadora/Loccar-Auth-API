using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoccarDomain.Common;
using LoccarDomain.Login;
using LoccarDomain.Register;
using LoccarDomain.User;

namespace LoccarApplication.Interfaces
{
    public interface IAuthApplication
    {
        Task<BaseReturn<string>> LoginAsync(LoginRequest loginRequest);
        Task<BaseReturn<UserData>> RegisterAsync(RegisterRequest request);
        Task<BaseReturn<string>> LogoutAsync();
    }
}
