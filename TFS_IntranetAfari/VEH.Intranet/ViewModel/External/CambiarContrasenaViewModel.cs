using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.External
{
    public class CambiarContrasenaViewModel : BaseViewModel
    {
        public String ContrasenaNueva { get; set; }
        public String ContrasenaAntigua { get; set; }
        public Int32 UsuarioId { get; set; }
        public void Fill(CargarDatosContext c, Int32 usuarioId)
        {
            UsuarioId = usuarioId;
            var usuario = c.context.Usuario.FirstOrDefault( x => x.UsuarioId == UsuarioId);
            ContrasenaAntigua = usuario.Password;
        }
    }
}