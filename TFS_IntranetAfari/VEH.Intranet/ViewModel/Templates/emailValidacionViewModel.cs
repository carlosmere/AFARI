using System;

using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Models;
namespace VEH.Intranet.ViewModel.Templates
{
    public class tablaValidacion
    {
        public String MontoOperacion { get; set; }
        public String Beneficiario { get; set; }
        public String Referencia { get; set; }
    }
    public class emailValidacionViewModel
    {
        public String Titulo { get; set; } = String.Empty;
        public String destinatario { get; set; } = String.Empty;
        public String Mensaje { get; set; } = String.Empty;
        public String administrador { get; set; } = String.Empty;
        public String Firma { get; set; } = String.Empty;
        public String Acro { get; set; } = String.Empty;
        public List<tablaValidacion> LstValidacion = new List<tablaValidacion>();
    }
}