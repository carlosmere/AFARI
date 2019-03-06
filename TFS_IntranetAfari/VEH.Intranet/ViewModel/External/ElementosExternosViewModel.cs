using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.External
{
    public class ElementosExternosViewModel : BaseViewModel
    {
        public List<MenuPropietarioEdificio> lstElementoExterno { get; set; }
        public Int32 EdificioId { get; set; } 
        public void fill(CargarDatosContext datacontext,Int32 EdificioId)
        {
            baseFill(datacontext);
            this.EdificioId = EdificioId;
            lstElementoExterno = datacontext.context.MenuPropietarioEdificio.Where(X => X.EdificioId == EdificioId && X.Estado == "ACT").ToList();

        }
    }
}