using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Income
{
    public class AddEditArchivoIngresoViewModel : BaseViewModel
    {
        public Int32? ArchivoIngresoId { get; set; }
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public string Nombre { get; set; }
       
       
        public string Ruta { get; set; }
        public Int32 EdificioId { get; set; }
        public String Estado { get; set; }
        public Int32 IngresoId { get; set; }
        public String DescripcionUnidadMedida { get; set; }
        public Edificio Edificio { get; set; }
        public Ingreso Ingreso { get; set; }
        public AddEditArchivoIngresoViewModel() { }

         [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
         [Display(Name = "Archivo")]
        public HttpPostedFileBase Archivo { get; set; }

        public void Fill(CargarDatosContext datacontext)
        {
            baseFill(datacontext);
            Edificio = datacontext.context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);
            Ingreso = datacontext.context.Ingreso.FirstOrDefault(x => x.IngresoId == IngresoId);
            DescripcionUnidadMedida = Ingreso.UnidadTiempo.Descripcion.ToUpper();
            if (ArchivoIngresoId.HasValue)
            {
                ArchivoIngreso ArchivoIngreso = datacontext.context.ArchivoIngreso.FirstOrDefault(x => x.ArchivoIngresoId == ArchivoIngresoId.Value);
                if (ArchivoIngreso != null)
                {
                    this.Nombre = ArchivoIngreso.Nombre;
                    this.Ruta = ArchivoIngreso.Ruta;
                    this.Estado = ArchivoIngreso.Estado;
                    this.IngresoId = ArchivoIngreso.IngresoId;
                }
            }
        }
    }
}