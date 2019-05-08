using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class CuotaBE
    {
        public Int32 cuotaId { get; set; }
        public string estado { get; set; }
        public Int32 unidadTiempoId { get; set; }
        public string unidadTiempoDescripcion { get; set; }
        public bool? esPagoAdelantado { get; set; }
        public decimal total { get; set; }
        public decimal totalConMora { get; set; }
        public Int32 departamentoId { get; set; }
        public string departamentoDescripcion { get; set; }
        public bool pagado { get; set; }
    }
}