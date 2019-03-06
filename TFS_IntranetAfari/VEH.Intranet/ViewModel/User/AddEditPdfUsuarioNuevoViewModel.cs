using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.User
{
    public class AddEditPdfUsuarioNuevoViewModel : BaseViewModel
    {
        public HttpPostedFileBase Archivo { get; set; }
        public void Fill(CargarDatosContext c)
        {
            baseFill(c);
        }
    }
}