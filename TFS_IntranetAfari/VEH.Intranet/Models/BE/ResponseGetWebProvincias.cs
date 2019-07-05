using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class ResponseGetWebProvincias : BaseBE
    {
        public List<AfariDB_Provincia> lstProvincia { get; set; }
    }
}