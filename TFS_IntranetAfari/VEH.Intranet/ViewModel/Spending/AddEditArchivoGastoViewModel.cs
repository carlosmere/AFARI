using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Spending
{
    public class AddEditArchivoGastoViewModel : BaseViewModel
    {
        public Int32? ArchivoGastoId { get; set; }
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public string Nombre { get; set; }
       
       
        public string Ruta { get; set; }
        public Int32 EdificioId { get; set; }
        public String Estado { get; set; }
        public Int32 GastoId { get; set; }
        public String DescripcionUnidadMedida { get; set; }
        public Edificio Edificio { get; set; }
        public Gasto Gasto { get; set; }
        public AddEditArchivoGastoViewModel() { }

         [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
         [Display(Name = "Archivo")]
        public List<HttpPostedFileBase> Archivo { get; set; }

        public void Fill(CargarDatosContext datacontext)
        {
            baseFill(datacontext);
            Edificio = datacontext.context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);
            Gasto = datacontext.context.Gasto.FirstOrDefault(x => x.GastoId == GastoId);
            DescripcionUnidadMedida = Gasto.UnidadTiempo.Descripcion.ToUpper();
            if (ArchivoGastoId.HasValue)
            {
                ArchivoGasto ArchivoGasto = datacontext.context.ArchivoGasto.FirstOrDefault(x => x.ArchivoGastoId == ArchivoGastoId.Value);
                if (ArchivoGasto != null)
                {
                    this.Nombre = ArchivoGasto.Nombre;
                    this.Ruta = ArchivoGasto.Ruta;
                    this.Estado = ArchivoGasto.Estado;
                    this.GastoId = ArchivoGasto.GastoId;
                }
            }
        }
    }
}