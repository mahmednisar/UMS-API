using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using UMS.Dto;
using UMS.Dto.Auth;

namespace UMS.Helper.JWT
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = (CurrentUser) context.HttpContext.Items["User"];
            if (user == null)
            {
                context.Result = new JsonResult(new TResponse() {ResponseMessage = ResponseMessage.AuthenticationFail, ResponseCode = StatusCodes.Status401Unauthorized, ResponseStatus = false}) {StatusCode = StatusCodes.Status401Unauthorized};
            }
        }
    }
}