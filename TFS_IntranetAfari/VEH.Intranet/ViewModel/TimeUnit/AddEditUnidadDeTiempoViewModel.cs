using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.TimeUnit
{
    public class AddEditUnidadDeTiempoViewModel : BaseViewModel

    {
        public Int32? UnidadTiempoId { get; set; }
        public String Descripcion { get; set; }
        
        [Display(Name = "Año")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Int32? Anio { get; set; }

        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Int32 Mes { get; set; }

        [Display(Name = "Es activo")]
        public Boolean EsActivo { get; set; }
        [Display(Name = "Orden")]
        public Int32? Orden { get; set; }

        public AddEditUnidadDeTiempoViewModel() { }

        public void Fill(CargarDatosContext datacontext)
        {

            baseFill(datacontext);
            if (UnidadTiempoId.HasValue)
            {
                UnidadTiempo unidadtiempo = datacontext.context.UnidadTiempo.FirstOrDefault(x => x.UnidadTiempoId == UnidadTiempoId.Value);
                this.Descripcion = unidadtiempo.Descripcion;
                this.Anio = unidadtiempo.Anio;
                this.Mes = unidadtiempo.Mes;
                this.EsActivo = unidadtiempo.EsActivo;
                this.Orden = unidadtiempo.Orden;
            }
        }
    }
}