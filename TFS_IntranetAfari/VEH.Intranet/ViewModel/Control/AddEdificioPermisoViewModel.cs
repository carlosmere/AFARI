using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Control
{
    public class AddEdificioPermisoViewModel : BaseViewModel
    {
        public String Nombres { get; set; }
        public Int32 UsuarioId { get; set; }
        public Int32 EdificioId { get; set; }
        public List<Edificio> LstEdificio { get; set; } = new List<Edificio>();
        public void Fill(CargarDatosContext c, Int32 usuarioId)
        {
            baseFill(c);
            this.UsuarioId = usuarioId;

            var lstActual = c.context.PermisoEdificio.Where(x => x.UsuarioId == this.UsuarioId).Select(x => x.EdificioId).ToList();
            this.LstEdificio = c.context.Edificio.Where(x => x.Estado == ConstantHelpers.EstadoActivo
            && lstActual.Contains(x.EdificioId) == false ).ToList();

            var usuario = c.context.Usuario.FirstOrDefault(x => x.UsuarioId == this.UsuarioId);
            this.Nombres = usuario.Nombres + "-" + usuario.Apellidos;
        }
    }
}