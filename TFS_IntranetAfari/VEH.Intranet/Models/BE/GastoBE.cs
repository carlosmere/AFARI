using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class GastoBE
    {
        public Int32 gastoId { get; set; }
        public Int32 unidadTiempoId { get; set; }
        public String unidadTiempoDescripcion { get; set; }
    }
}