using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class ResponseGetWebProvincias : BaseBE
    {
        public List<VehDB_Provincia> lstProvincia { get; set; }
    }
}