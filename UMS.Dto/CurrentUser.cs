using System;

namespace UMS.Dto
{
    public class CurrentUser
    {
        public  int Id { get; set; }
        public  string RoleIDs{ get; set; }
        public  string CompName{ get; set; }
        public  int LocID { get; set; }
        public  string AuthKey{ get; set; }
    }

    public class Country
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Date { get; set; }
        public string UserID { get; set; }
        public bool status { get; set; }
        public string ISD { get; set; }
    }
}