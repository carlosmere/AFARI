using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Employee
{
    public class LstAFPViewModel : BaseViewModel
    {
        public List<AFP> LstAFP { get; set; }
        public List<ComisionAFP> LstComisionAFP { get; set; }
        public LstAFPViewModel()
        {
            LstAFP = new List<AFP>();
        }

        public void Fill(CargarDatosContext datacontext)
        {
            baseFill(datacontext);
            LstAFP = datacontext.context.AFP.OrderBy(x => x.Nombre).ToList();
            LstComisionAFP = datacontext.context.ComisionAFP.OrderBy(x => x.TipoDescuento.Detalle).ToList();
        }
    }
}