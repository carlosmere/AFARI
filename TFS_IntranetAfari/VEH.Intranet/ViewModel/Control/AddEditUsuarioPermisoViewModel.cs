using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Control
{
    public class AddEditUsuarioPermisoViewModel : BaseViewModel
    {
        public Int32 UsuarioId { get; set; }
        public String Nombres { get; set; }
        public List<Edificio> LstEdificio = new List<Edificio>();
        public void Fill(CargarDatosContext c, Int32 usuarioId)
        {
            baseFill(c);
            this.UsuarioId = usuarioId;
            this.LstEdificio = c.context.PermisoEdificio.Where(x => x.UsuarioId == this.UsuarioId).Select(x => x.Edificio).ToList();
        }
    }
}