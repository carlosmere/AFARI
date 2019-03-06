using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;
using VEH.Intranet.Models;
using VEH.Intranet.Helpers;
using VEH.Intranet.Controllers;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Building
{
    public class LstEdificioViewModel : BaseViewModel
    {
        public List<Edificio> LstEdificio { get; set; }
        public Int32? _NumPagina { get; set; }
        public String Nombre { get; set; }
        public List<Edificio> LstcmbEdificio { get; set; } = new List<Edificio>();
        public LstEdificioViewModel() { }

        public void Fill(CargarDatosContext datacontext, Int32? _np, String nombre)
        {
            this.Nombre = nombre;
            baseFill(datacontext);
            _NumPagina = _np ?? 1;
            var query = datacontext.context.Edificio.Where( x => x.Estado != ConstantHelpers.EstadoEliminado).OrderBy(x => x.Estado).ThenBy( x => x.Orden).AsQueryable();

            if (!String.IsNullOrEmpty(this.Nombre))
            {
                query = query.Where(x => x.Nombre.Contains(this.Nombre));
            }
            var usuarioId = datacontext.session.GetUsuarioId();
            var usuario = datacontext.context.Usuario.FirstOrDefault( x => x.UsuarioId == usuarioId);
            if (usuario != null && usuario.EsAdmin == false)
            {
                var lstPermiso = datacontext.context.PermisoEdificio.Where(x => x.UsuarioId == usuarioId).Select(x => x.EdificioId).ToList();
                query = query.Where( x => lstPermiso.Contains( x.EdificioId));
            }
            LstcmbEdificio = datacontext.context.Edificio.Where( x => x.Estado == ConstantHelpers.EstadoActivo).ToList();
            LstEdificio = query.ToList();
        }
    }
}