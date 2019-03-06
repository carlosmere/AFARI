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

namespace VEH.Intranet.ViewModel.Staff
{
    public class AddEditPlanillaViewModel : BaseViewModel
    {
        public Int32? PlanillaId { get; set; }
        public Int32 TrabajadorId { get; set; }
        public Int32 UnidadTiempoId { get; set; }
        [Display(Name = "Horas extras")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Decimal? HorasExtras { get; set; }
        [Display(Name = "Feriado")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Decimal? Feriado { get; set; }
        [Display(Name = "Adelanto quincena")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Decimal? AdelantoQuincena { get; set; }
        [Display(Name = "Segunda quincena")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Decimal? SegundaQuincena { get; set; }
        [Display(Name = "EsSalud")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Decimal? ESSALUD { get; set; }
        [Display(Name = "Aporte obligatorio")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Decimal? AporteObligatorio { get; set; }
        [Display(Name = "Prima seguro")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Decimal? PrimaSeguro { get; set; }
        [Display(Name = "Comisión AFP")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Decimal? ComisionAFP { get; set; }
        [Display(Name = "Total descuentos")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Decimal? TotalDescuentos { get; set; }
        [Display(Name = "Sueldo total neto")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Decimal? SueldoTotalNeto { get; set; }
        [Display(Name = "Segunda quincena neto")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Decimal? SegundaQuincenaNeto { get; set; }
        [Display(Name = "CTS del mes")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Decimal? CTSMes { get; set; }
        [Display(Name = "Reemplazo de vacaciones")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Decimal? ReemplazoVacaciones { get; set; }

        public Trabajador Trabajador { get; set; }
        public Int32 EdificioId { get; set; }
        public Edificio Edificio { get; set; }
        public List<SelectListItem> LstComboUnidadTiempo { get; set; }

        public AddEditPlanillaViewModel() { }

        public void Fill(CargarDatosContext datacontext)
        {
            baseFill(datacontext);
            Trabajador = datacontext.context.Trabajador.FirstOrDefault(x => x.TrabajadorId == TrabajadorId);
            Edificio = datacontext.context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);
            if (PlanillaId.HasValue)
            {
                Planilla planilla = datacontext.context.Planilla.FirstOrDefault(x => x.PlanillaId == PlanillaId.Value);
                this.TrabajadorId = planilla.TrabajadorId;
                this.UnidadTiempoId = planilla.UnidadTiempoId;
                this.HorasExtras = planilla.HorasExtras;
                this.Feriado = planilla.Feriado;
                this.AdelantoQuincena = planilla.AdelantoQuincena;
                this.SegundaQuincena = planilla.SegundaQuincena;
                this.ESSALUD = planilla.ESSALUD;
                this.AporteObligatorio = planilla.AporteObligatorio;
                this.PrimaSeguro = planilla.PrimaSeguro;
                this.ComisionAFP = planilla.ComisionAFP;
                this.TotalDescuentos = planilla.TotalDescuentos;
                this.SueldoTotalNeto = planilla.SueldoTotalNeto;
                this.SegundaQuincenaNeto = planilla.SegundaQuincenaNeto;
                this.CTSMes = planilla.CTSMes;
                this.ReemplazoVacaciones = planilla.ReemplazoVacaciones;
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