using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMS.Dto;
using UMS.Dto.Auth;

namespace UMS.Services.Infrastructure.authentication
{
    public interface IAuthService
    {
        TResponse Index();

        Task<(AuthenticateResponse, int, string)> Authenticate(AuthenticateRequest model, string ipAddress);

        Task<bool> ValidateLogin(string authKey, int userId, string compName);
        Task<bool> ValidateRole(string roles, string path);
        Task<TResponse> Test();
    }
}