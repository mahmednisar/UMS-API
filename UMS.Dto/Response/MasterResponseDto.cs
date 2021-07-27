using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UMS.Response.Dto
{
    public class MasterResponseDto
    {
        public int ID { get; set;  }
        public string Name { get; set; }
        public string Code{ get; set; }
        public bool Status{ get; set; }
        public string Date{ get; set; }
        public string User{ get; set; }
    }


}
