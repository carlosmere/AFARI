using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.TimeUnit
{
    public class LstUnidadDeTiempoViewModel : BaseViewModel
    {
        public List<UnidadTiempo> LstUnidadDeTiempo { get; set; }
        public LstUnidadDeTiempoViewModel() { }

        public void Fill(CargarDatosContext datacontext)
        {
            baseFill(datacontext);
            LstUnidadDeTiempo = new List<UnidadTiempo>();
            LstUnidadDeTiempo = datacontext.context.UnidadTiempo.OrderByDescending(x => x.Anio).Where(x => x.Estado == ConstantHelpers.EstadoActivo).ToList();
        }
    }
}