using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.Controllers;
using VEH.Intranet.Models;
using VEH.Intranet.Helpers;
using System.ComponentModel.DataAnnotations;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Income
{
    public class AddEditIngresoViewModel : BaseViewModel
    {
        public Int32? IngresoId { get; set; }
        public Int32 EdificioId { get; set; }
        public Edificio Edificio { get; set; }

        public String Estado { get; set; }
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Int32 UnidadTiempoId { get; set; }
        public DateTime FechaRegistro { get; set; }

        public List<SelectListItem> LstComboUnidadTiempo { get; set; }

        public AddEditIngresoViewModel() { }

        public void Fill(CargarDatosContext datacontext)
        {
            baseFill(datacontext);
            Edificio = datacontext.context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);
            if (IngresoId.HasValue)
            {
                Ingreso ingreso = datacontext.context.Ingreso.FirstOrDefault(x => x.IngresoId == IngresoId.Value);
                this.Estado = ingreso.Estado;
                this.UnidadTiempoId = ingreso.UnidadTiempoId;
                this.FechaRegistro = ingreso.FechaRegistro;
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