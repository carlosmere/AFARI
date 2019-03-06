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
    public class LstAnuncioViewModel : BaseViewModel
    {
        public List<Anuncio> LstAnuncio { get; set; } = new List<Anuncio>();
        public void Fill(CargarDatosContext c)
        {
            LstAnuncio = c.context.Anuncio.Where( x => x.Estado != ConstantHelpers.EstadoEliminado).OrderByDescending( x => x.AnuncioId).ToList();
        }
    }
}