using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMS.Dto;
using UMS.Dto.Auth;
using UMS.Helper.JWT;
using UMS.Services.Infrastructure;
using System.Dynamic;
using UMS.Services.Infrastructure.authentication;

namespace UMS.Helper.JWT
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AppSettings _appSettings;

        public JwtMiddleware(RequestDelegate next, IOptions<AppSettings> appSettings)
        {
            _next = next;
            _appSettings = appSettings.Value;
        }

        public async Task Invoke(HttpContext context, IAuthService authService)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
                await AttachUserToContext(context, authService, token);

            await _next(context);
        }

        private async Task AttachUserToContext(HttpContext context, IAuthService authService, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = false,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out var validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);
                var roleIds = jwtToken.Claims.First(x => x.Type == "roles").Value;
                var authKey = jwtToken.Claims.First(x => x.Type == "authKey").Value;
                var compName = jwtToken.Claims.First(x => x.Type == "compName").Value;
                var LocID = int.Parse(jwtToken.Claims.First(x => x.Type == "LocID").Value);
                if (string.IsNullOrEmpty(context.Session.GetString("compName")))
                {
                    context.Session.SetString("compName", jwtToken.Claims.First(x => x.Type == "compName").Value);
                }
                if (!await authService.ValidateLogin(authKey, Convert.ToInt32(userId), compName)) return;
                if (userId != 0)
                {
                    CurrentUser user = new CurrentUser() { CompName = compName, AuthKey = authKey, Id = userId, RoleIDs = roleIds, LocID= LocID };
                    context.Items["User"] = user;
                }
            }
            catch
            {
                // do nothing if jwt validation fails
                // user is not attached to context so request won't have access to secure routes
            }
        }
    }
}