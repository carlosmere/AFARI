using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class EstadoCuentaBancarioBE
    {
        public Int32 estadoCuentaId { get; set; }
        public String descripcionUnidadTiempo { get; set; }
        public Int32 unidadTiempoId { get; set; }
        public String documento { get; set; }
    }
}