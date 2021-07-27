using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UMS.Core;

namespace UMS.Controllers
{
    public class CipherController : BaseController
    {
        #region ActionMethod

        // [Authorize]
        [HttpGet]
        public string GetEncryption(string value, string passkey)
        {
            return Cryptography.Encrypt(value, passkey);
        }

        // [Authorize]
        [HttpGet]
        public string GetDecryption(string value, string passkey)
        {
            return Cryptography.Decrypt(value, passkey);
        }

        //[HttpGet]
        //public string GetEncConnectionString(string IP, string Database, string Username, string Password)
        //{
        //   string Value =  @"Data Source="+ IP + "; Database="+Database+"; User Id="+Username+"; Password="+Password+";";
        //    return Cryptography.Decrypt(Value, "OAaQ82lM4USYvH5f3O7uCA==");
        //}


        [HttpGet]
        public string GetEncConnectionString(string value)
        {
            return Cryptography.Encrypt(value, "OAaQ82lM4USYvH5f3O7uCA==");
        }

        #endregion
    }
}