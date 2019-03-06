using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.Controllers;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Employee
{
    public class AddEditArchivoTrabajadorViewModel : BaseViewModel
    {
        public Int32? ArchivoTrabajadorId { get; set; }
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public string Nombre { get; set; }


        public string Ruta { get; set; }
        public Int32 EdificioId { get; set; }
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Int32 UnidadTiempoId { get; set; }
        public String Estado { get; set; }
        public Int32 GastoId { get; set; }
        public String DescripcionUnidadTiempo { get; set; }
        public Edificio Edificio { get; set; }
        public AddEditArchivoTrabajadorViewModel() { }
        public List<SelectListItem> LstComboUnidadTiempo { get; set; }

        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        [Display(Name = "Archivo")]
        public HttpPostedFileBase Archivo { get; set; }

        public void Fill(CargarDatosContext datacontext)
        {
            baseFill(datacontext);
            Edificio = datacontext.context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);
            if (ArchivoTrabajadorId.HasValue)
            {
                ArchivoTrabajador ArchivoTrabajador = datacontext.context.ArchivoTrabajador.FirstOrDefault(x => x.ArchivoTrabajadorId == ArchivoTrabajadorId.Value);
                if (ArchivoTrabajador != null)
                {
                    this.Nombre = ArchivoTrabajador.Nombre;
                    this.Ruta = ArchivoTrabajador.Ruta;
                    this.Estado = ArchivoTrabajador.Estado;
                    this.ArchivoTrabajadorId = ArchivoTrabajador.ArchivoTrabajadorId;
                    this.UnidadTiempoId = ArchivoTrabajador.UnidadTiempoId;
                    this.DescripcionUnidadTiempo = ArchivoTrabajador.UnidadTiempo.Descripcion;
                }
            }
        }

        public void FillComboUnidadTiempo(CargarDatosContext datacontext)
        {
            LstComboUnidadTiempo = new List<SelectListItem>();
            var lstunidadtiempo = datacontext.context.UnidadTiempo.OrderBy(x => x.EsActivo).OrderByDescending(x => x.Anio).OrderByDescending(x => x.Mes).Where(x => x.Estado == ConstantHelpers.EstadoActivo && x.EsActivo);
            foreach (var item in lstunidadtiempo)
                LstComboUnidadTiempo.Add(new SelectListItem { Value = item.UnidadTiempoId.ToString(), Text = item.Descripcion.ToUpper() });
        }
    }
}