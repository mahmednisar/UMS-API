using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UMS.Dto;
using UMS.Helper;
using UMS.Helper.JWT;
using UMS.Services.Infrastructure;
using UMS.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using UMS.Dto.Auth;
using AutoMapper;
using UMS.Helper.helper;
using UMS.Services.Infrastructure.authentication;
using System.Diagnostics;

namespace UMS.Services.authentication
{
    public class AuthService : IAuthService
    {
        #region ClassMembers

        private DataManager _dataManager;
        private readonly AppSettings _appSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private DataTable _dataTable = new();
        private DataSet _dataSet = new();
        private TResponse _response = new();
        private List<ViewParam> _list = new();

        #endregion

        #region Constructor

        public AuthService(IOptions<AppSettings> appSettings, IConfiguration config,
            IHttpContextAccessor httpContextAccessor)
        {
            _appSettings = appSettings.Value;
            _dataManager = new DataManager(config, httpContextAccessor);
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region  Services

        public TResponse Index()
        {
            dynamic obj = new ExpandoObject();
            obj.CompHead = _appSettings.CompHead;
            obj.CompName = _appSettings.CompName;

            _response.ResponseCode = StatusCodes.Status200OK;
            _response.ResponseMessage = ResponseMessage.Success;
            _response.ResponseStatus = true;
            _response.ResponsePacket = obj;
            return _response;
        }

        public async Task<(AuthenticateResponse, int, string)> Authenticate(AuthenticateRequest model, string ipAddress)
        {
            var authenticateResponse = new AuthenticateResponse();
            var retVal = 0;
            var message = "";
            const string query =
                "select User_kid,  User_Code,User_Passkey, User_Password,User_SuccessLoginTime,  User_FailedAttempt,isnull( User_FailedAttemptdatetime, DATEADD(MINUTE,-30,GETDATE())) User_FailedAttemptdatetime,User_AuthKey, User_SingleSession from U_User where User_Status=1 and User_Code=@userCode";
            _list.Add(new ViewParam() { Name = "userCode", Value = model.UserCode });
            dynamic data  = await _dataManager.ExecuteReaderWithQuery(query, _list, model.CompName);

            if (data != null && data.Count>0)
            {
                if (data[0].Count > 0)
                {
                    var data1 = (IDictionary<string, object>)data[0][0];
                    var attempt1 = data1["User_Code"].ToString();
                    var attempt = Convert.ToInt32(data1["User_FailedAttempt"].ToString());
                    var failedTime = Convert.ToDateTime(data1["User_FailedAttemptdatetime"].ToString());
                    var id = data1["User_kid"].ToString();
                    if (attempt <= 3 || failedTime < DateTime.Now.AddMinutes(-15))
                    {
                        if (string.Equals(
                            Cryptography.Encrypt(model.Password, data1["User_Passkey"].ToString()),
                            data1["User_Password"].ToString()))
                        {
                            var singleSession = Convert.ToBoolean(data1["User_SingleSession"].ToString());
                            var authKey = data1["User_AuthKey"].ToString();
                            if (singleSession != true || authKey == "" || model.LogIn == true)
                            {
                                authKey = Cryptography.GetKey();
                                var lastLogin = data1["User_SuccessLoginTime"].ToString();
                                _list.Clear();
                                _list.Add(new ViewParam() { Name = "UserId", Value = id });
                                _list.Add(new ViewParam() { Name = "ipAddress", Value = ipAddress });
                                _list.Add(new ViewParam() { Name = "authkey", Value = authKey });
                                dynamic  dataset = await _dataManager.ExecuteReaderWithSP("H_LoginUserSuccess", _list, model.CompName);
                                if (dataset != null && dataset.Count > 0 && dataset[0].Count > 0)
                                {
                                    var dataset1 = (IDictionary<string, object>)dataset[0][0];
                                    var roles = dataset1["Roles"].ToString();
                                    var LocID = dataset1["LocID"].ToString();
                                    AuthenticateResponse authenticate = new AuthenticateResponse();    
                                    authenticateResponse = Maper.Map(dataset, authenticate);
                                    if (authenticateResponse != null)
                                    {
                                        authenticateResponse.LastLogin = lastLogin == "" ? (DateTime?)null : Convert.ToDateTime(lastLogin);
                                        authenticateResponse.Token = GenerateJwtToken(id, authKey, model.CompName, roles, LocID);
                                    }
                                    message = ResponseMessage.LoginSuccess;
                                    retVal = 1; //return 200 true --->   User login success;
                                }
                                else
                                {
                                    message = ResponseMessage.Error;
                                    retVal = -1; //return 501  -->  Error occured
                                }
                            }
                            else
                            {
                                message = ResponseMessage.LoginUnSuccess;
                                retVal = 2; //Return 200-false ---->User already login  
                            }
                        }
                        else
                        {
                            _list.Clear();
                            _list.Add(new ViewParam() { Name = "UserId", Value = id });
                            _list.Add(new ViewParam() { Name = "ipAddress", Value = ipAddress });
                            _list.Add(new ViewParam() { Name = "description", Value = "Incorrect password count " + (attempt + 1).ToString() + "." });
                            dynamic dataSet = await _dataManager.ExecuteReaderWithSP("H_LoginUserFailed", _list, model.CompName);
                            if (dataSet == null || dataSet.Count <= 0) return (authenticateResponse, retVal, message);
                            message = ResponseMessage.AuthenticationFail + " Remaining atempt = " + (4 - attempt).ToString() + ".";
                            retVal = 3; //Return 401-true ---->  User login failed 
                        }
                    }
                    else
                    {
                        _list.Clear();
                        _list.Add(new ViewParam() { Name = "UserId", Value = id });
                        _list.Add(new ViewParam() { Name = "ipAddress", Value = ipAddress });
                        _list.Add(new ViewParam() { Name = "description", Value = "Incorrect password count " + (attempt + 1).ToString() + "." });
                        dynamic dataset= await _dataManager.ExecuteReaderWithSP("H_LoginUserFailed", _list, model.CompName);
                        if (dataset == null || dataset.Count <= 0) return (authenticateResponse, retVal, message);
                        message = ResponseMessage.MaxUnsuccessLogin + (failedTime.AddMinutes(15) - DateTime.Now).Minutes.ToString() + " minutes.";
                        retVal = 4; //Return 401-false -->  max unsuccessful login reached 
                    }
                }
                else
                {
                    message = ResponseMessage.InvalidUser;
                    retVal = 0; //Return 403 User not found..
                }
            }
            else
            {
                message = ResponseMessage.Error;
                retVal = -1; //Return Error if any Response->500
            }

            return (authenticateResponse, retVal, message);
        }
        #endregion

        #region GetOrValidateCurrentUser

        // public AuthenticateResponse GetCurrentUser(int id)
        // {
        //     const string query = "select user_kid as Id ,user_code UserCode,User_fname FirstName,User_mname MiddleName,User_lname LastName,User_pcontact Contact,User_pemail Email,User_lastlogin LastLogin, '' as Roles from h_User where user_kid=@id";;
        //     _list.Clear();
        //     _list.Add(new ViewParam() {Name = "id", Value =id});
        //     _dataTable = _dataManager.Get(query, _list);
        //     return _dataTable.DataTableToList<AuthenticateResponse>().FirstOrDefault();
        // }
        public async Task<bool> ValidateLogin(string authKey, int userId, string compName)
        {
            //const string query = "select count(User_kid) from  h_User where User_kid=@UserId and (User_AuthKey=@authKey or User_SingleSession=0) and User_Status=1";
            //_list.Add(new ViewParam() { Name = "UserId", Value = userId });
            //_list.Add(new ViewParam() { Name = "authKey", Value = authKey });
            //var count = await _dataManager.GetValue(query, _list, compName);

            //return Convert.ToInt32(count) > 0;
            return true;
        }
        public async Task<bool> ValidateRole(string roles, string path)
        {
            //var count = Task.Run(async () => await _dataManager.GetValue(""));
            //  return Convert.ToInt32(count) > 0;
            return true;
        }

        #endregion

        #region GenerateJwt
        /// <summary>
        /// //here role is comma separated  role id i.e.  if role is Admin, HR having ids 1 & 2  then roles will be 1,2  
        /// </summary>
        /// <param name="id"></param>
        /// <param name="authKey"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        private string GenerateJwtToken(string id, string authKey, string compname, string roles = "", string LocID = "")
        {
            // generate token that is valid for 8 hrs only.
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _appSettings.Issuer,
                IssuedAt = DateTime.Now,
                Subject = new ClaimsIdentity(new[] { new Claim("id", id), new Claim("roles", roles), new Claim("authKey", authKey), new Claim("compName", compname), new Claim("LocID", LocID) }),
                Expires = DateTime.UtcNow.AddHours(_appSettings.AccessTokenExpiration),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<TResponse> Test()
        {
            dynamic currentUser = await _dataManager.ExecuteReaderWithSP("test", _list, "ind");
            var currentUsers = Maper.MapList<Country>(currentUser[0]);
            _response.ResponseCode = 200;
            _response.ResponsePacket = currentUsers;
            return _response;
        }

        #endregion
    }
}