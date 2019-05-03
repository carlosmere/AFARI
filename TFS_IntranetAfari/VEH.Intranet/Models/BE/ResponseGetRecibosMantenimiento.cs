using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class ResponseGetRecibosMantenimiento : BaseBE
    {
        public List<ReciboMantenimientoBE> lstRecibosMantenimiento { get; set; } = new List<ReciboMantenimientoBE>();
    }
}