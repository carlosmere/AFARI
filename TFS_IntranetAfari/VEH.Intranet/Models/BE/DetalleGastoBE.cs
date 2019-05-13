using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class DetalleGastoBE
    {
        public int detalleGastoId { get; set; }
        public String nombre { get; set; }
        public decimal monto { get; set; } = 0;
    }
}