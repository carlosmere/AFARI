using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class ResponseGetWebDistritos : BaseBE
    {
        public List<AfariDB_Distrito> lstDistrito { get; set; }
    }
}