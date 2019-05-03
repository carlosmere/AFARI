using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class ReciboMantenimientoBE
    {
        public Int32 departamentoId { get; set; }
        public Int32 unidadTiempoId { get; set; }
        public String departamentoDescripcion { get; set; }
        public String unidadTiempoDescripcion { get; set; }
    }
}