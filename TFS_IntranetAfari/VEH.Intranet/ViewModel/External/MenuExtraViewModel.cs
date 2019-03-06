using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.External
{
    public class MenuExtraViewModel : BaseViewModel
    {
        public MenuPropietarioEdificio menu { get; set; }

        public void fill(CargarDatosContext datacontext,Int32 menuId)
        {
            baseFill(datacontext);
            menu = datacontext.context.MenuPropietarioEdificio.FirstOrDefault(X => X.MenuPropietarioEdificioId == menuId);

        }
    }
}