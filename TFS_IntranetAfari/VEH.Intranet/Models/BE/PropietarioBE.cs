using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class PropietarioBE
    {
        public int propietarioId { get; set;}
        public String nombreDepartamento { get; set; }
        public String nombrePropietario { get; set; }
        public String telefono { get; set; }
        public String email { get; set; }
        public String celular { get; set; }
        public String nombreInquilino { get; set; }
        public String nroDocumento { get; set; }
        public String parentesco { get; set; }
        public String celularInquilino { get; set; }
        public String emailInquilino { get; set; }
    }
}