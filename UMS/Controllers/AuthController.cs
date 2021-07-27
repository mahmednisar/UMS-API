using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UMS.Dto;
using UMS.Dto.Auth;
using UMS.Helper.JWT;
using UMS.Services.Infrastructure.authentication;

namespace UMS.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class AuthController : BaseController
    {
        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="authService"></param>
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        #endregion

        #region Members

        private readonly IAuthService _authService;
        private TResponse _response = new();

        #endregion

        #region ActionMethod
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public TResponse Index()
        {
            return _authService.Index();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TResponse> Login(AuthenticateRequest model)
        {
            var (authResponse, keyValue, message) = await _authService.Authenticate(model, IpAddress());
            switch (keyValue)
            {
                case 0:
                    _response.ResponseCode = StatusCodes.Status403Forbidden;
                    _response.ResponseStatus = true;
                    _response.ResponseMessage = message;
                    break;
                case 1:
                    _response.ResponseCode = StatusCodes.Status200OK;
                    _response.ResponseStatus = true;
                    _response.ResponseMessage = message;
                    _response.ResponsePacket = authResponse;
                    break;
                case 2:
                    _response.ResponseCode = StatusCodes.Status200OK;
                    _response.ResponseStatus = false;
                    _response.ResponseMessage = message;
                    break;
                case 3:
                    _response.ResponseCode = StatusCodes.Status401Unauthorized;
                    _response.ResponseStatus = true;
                    _response.ResponseMessage = message;
                    break;
                case 4:
                    _response.ResponseCode = StatusCodes.Status401Unauthorized;
                    _response.ResponseStatus = false;
                    _response.ResponseMessage = message;
                    break;
                case -1:
                    _response.ResponseCode = StatusCodes.Status500InternalServerError;
                    _response.ResponseStatus = false;
                    _response.ResponseMessage = message;
                    break;
            }

            return _response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public async Task<TResponse> GetUser()
        {
            _response.ResponseCode = (int)HttpStatusCode.OK;
            _response.ResponsePacket = CurrentUser;
            _response.ResponseStatus = true;

            return _response;
        }


        [HttpGet]
        public async Task<TResponse> Test()
        {
            return await _authService.Test();
        }




        #endregion
    }
}