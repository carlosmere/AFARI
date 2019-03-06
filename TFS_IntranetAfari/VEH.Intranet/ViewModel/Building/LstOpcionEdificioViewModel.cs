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
    public class LstOpcionEdificioViewModel : BaseViewModel
    {
        public Int32 EdificioId { get; set; }
        public String Estado { get; set; }
        public String NombreEdificio { get; set; } = String.Empty;
        public LstOpcionEdificioViewModel() { }

        public void Fill(CargarDatosContext c, Int32 edificioId)
        {
            this.EdificioId = edificioId;
            var edificio = c.context.Edificio.FirstOrDefault( x => x.EdificioId == this.EdificioId);
            NombreEdificio = edificio.Nombre;
            this.Estado = edificio.Estado;
        }
    }
}