using System;

using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Models;
namespace VEH.Intranet.ViewModel.Templates
{
    public class infoViewModel
    {
        public String Titulo { get; set; } = String.Empty;
        public VEH.Intranet.ViewModel.Building.EnviarEmailInformativoViewModel.Destinatario destinatario { get; set; } = new Building.EnviarEmailInformativoViewModel.Destinatario();
        public String Mensaje { get; set; } = String.Empty;
        public String administrador { get; set; } = String.Empty;
        public String Firma { get; set; } = String.Empty;
        public String Acro { get; set; } = String.Empty;
        public void Fill(CargarDatosContext datacontext,String titulo,VEH.Intranet.ViewModel.Building.EnviarEmailInformativoViewModel.Destinatario des, String mensaje,String admin,String Firma="")
        {
            administrador = admin;
            Titulo = titulo;
            Mensaje = mensaje;
            this.Firma = Firma;
            this.destinatario = des;

        }
    }
}