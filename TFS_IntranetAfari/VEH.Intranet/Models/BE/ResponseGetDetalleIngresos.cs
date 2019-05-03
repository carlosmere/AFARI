using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class ResponseGetDetalleIngresos : BaseBE
    {
        public List<DetalleIngresoBE> lstDetalleIngreso { get; set; } = new List<DetalleIngresoBE>();
    }
}