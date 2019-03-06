using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Models;
using VEH.Intranet.Helpers;
using System.ComponentModel.DataAnnotations;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Building
{
    public class NormasViewModel : BaseViewModel
    {
       // public String NormasConvivencia { get; set; }
        [Display(Name = "Normas de Convivencia")]
        public String Ruta { get; set; }
        public String NombreEdificio { get; set; }
        public String AcronimoEdificio { get; set; }

        //public HttpPostedFileBase Archivo { get; set; }
        public NormasViewModel()
        {
        }

        public void Fill(CargarDatosContext datacontext)
        {
            baseFill(datacontext);
            Int32 edificioId =  datacontext.session.GetEdificioId();
            Edificio edificio = datacontext.context.Edificio.Where(x => x.EdificioId == edificioId).FirstOrDefault();
            NombreEdificio = edificio.Nombre;
            AcronimoEdificio = edificio.Acronimo;
            this.Ruta = edificio.NormasConvivencia; // esta es la ruta
        }
    }
}