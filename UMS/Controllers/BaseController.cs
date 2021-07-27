using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UMS.Dto;

namespace UMS.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected CurrentUser CurrentUser => (CurrentUser)HttpContext.Items["User"];

        #region  Methods

        protected string IpAddress()
        {
            return Request.Headers.ContainsKey("X-Forwarded-For")
                ? (string)Request.Headers["X-Forwarded-For"]
                : HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }
        #endregion
    }
}
