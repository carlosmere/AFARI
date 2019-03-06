using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Control
{
    public class LstPermisoEdificoViewModel : BaseViewModel
    {
        public List<Usuario> LstUsuario { get; set; } = new List<Usuario>();
        public void Fill(CargarDatosContext c)
        {
            baseFill(c);
            LstUsuario = c.context.Usuario.Where( x => x.Rol == "ADM" && x.EsAdmin == false).ToList();
        }
    }
}