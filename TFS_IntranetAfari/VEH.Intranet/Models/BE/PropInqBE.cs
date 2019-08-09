using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class PropInqBE
    {
        public int Id { get; set; }
        public String nombreDepartamento { get; set; }
        public String nombre { get; set; }
        public String telefono { get; set; }
        public String email { get; set; }
        public String celular { get; set; }
        public String nroDocumento { get; set; }
        public String tipo { get; set; } = "PRO";
    }
}