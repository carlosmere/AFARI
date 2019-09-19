using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class ResponseGetVentaAlquiler : BaseBE
    {
        public List<VehDB_Edificio> lstEdificio { get; set; }
    }
}