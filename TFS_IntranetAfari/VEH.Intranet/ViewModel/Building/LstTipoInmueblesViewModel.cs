using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Building
{
    public class LstTipoInmueblesViewModel : BaseViewModel
    {
        public List<TipoInmueble> LstTipoInmueble { get; set; } 
        public void Fill(CargarDatosContext c)
        {
            LstTipoInmueble = c.context.TipoInmueble.Where(x => x.Estado == ConstantHelpers.EstadoActivo).ToList();
        }
    }
}