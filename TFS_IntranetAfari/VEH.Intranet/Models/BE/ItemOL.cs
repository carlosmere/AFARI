using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class ItemOL
    {
        public Int32 id { get; set; }
        public String Nombre { get; set; }
        public Int32? unidadTiempoId { get; set; }
        public String detalleUnidadTiempo { get; set; }
        public String documento { get; set; }
        public String tipo { get; set; }
    }
}