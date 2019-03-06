using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Building
{
    public class AddEditTipoInmuebleViewModel : BaseViewModel
    {
        public Int32? TipoInmuebleId { get; set; }
        public String Nombre { get; set; }
        public String Acronimo { get; set; }
        public void Fill(CargarDatosContext c,Int32? tipoInmuebleId)
        {
            TipoInmuebleId = tipoInmuebleId;
            if (TipoInmuebleId.HasValue)
            {
                var tipo = c.context.TipoInmueble.FirstOrDefault( x => x.TipoInmuebleId == this.TipoInmuebleId);
                Nombre = tipo.Nombre;
                Acronimo = tipo.Acronimo;
            }
        }
    }
}