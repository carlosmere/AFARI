using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;

namespace VEH.Intranet.ViewModel.Home
{
    public class CambiarContrasenaViewModel
    {
        public Int32 UsuarioId { get; set; }
        public String Password { get; set; }
        public String NewPassword { get; set; }
        public String Nombres { get; set; }
        public void Fill(CargarDatosContext c, Int32 usuarioId, String nombres)
        {
            this.UsuarioId = usuarioId;
            this.Nombres = nombres;
        }
    }
}