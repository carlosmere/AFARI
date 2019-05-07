using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class ResponseGetCertificadosEquipos : BaseBE
    {
        public List<ItemOL> lstCertficadosEquipos { get; set; } = new List<ItemOL>();
    }
}