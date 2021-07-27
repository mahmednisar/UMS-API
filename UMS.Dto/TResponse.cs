using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UMS.Dto
{
    public class TResponse
    {
        public int ResponseCode { get; set; }
        public bool ResponseStatus { get; set; }
        public string ResponseMessage { get; set; }
        public object ResponsePacket { get; set; }
    }

    public static class ResponseMessage
    {
        public static string MaxUnsuccessLogin = "Maximum unsuccessful login attempts reached. Please try again in ";
        public static string LoginSuccess = "Login Success.";
        public static string Success = "Succes.";
        public static string LoginUnSuccess = "Login failed. User already login somewhere else. do you want to continue.";
        public static string AuthenticationFail = "Authentication fail.";
        public static string InvalidUser = "Invalid User.";
        
        public static string Get = "Success";
        public static string Error = "Something went wrong !!";
        public static string Save = "Record saved successfully";
        public static string Update = "Record updated successfully.";
        public static string Delete = "Record deleted successfully.";
        public static string Duplicate = "Record already exist.";
        public static string DeleteFailed = "Record cannot be deleted.";
    }
}