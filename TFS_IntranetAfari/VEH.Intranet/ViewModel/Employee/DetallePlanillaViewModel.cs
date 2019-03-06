using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Employee
{
    public class DetallePlanillaViewModel : BaseViewModel
    {
        public Planilla planilla { get; set; }
        public Int32 EdificioId { get; set; }
        public Int32 PlanillaId { get; set; }
        public Int32 UnidadTiempoId { get; set; }

        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        [Display(Name="Total Descuentos")]
        public Decimal TotalDescuentos { get; set; }

        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        [Display(Name = "Sueldo Total Neto")]
        public Decimal SueldoTotalNeto { get; set; }

        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        [Display(Name = "Sueldo Quincena Neto")]
        public Decimal SegundaQuincenaNeto { get; set; }

        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        [Display(Name = "Gratificaciones del Mes")]
        public Decimal GratificacionesMes { get; set; }

        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        [Display(Name = "CTS")]
        public Decimal CTSMes { get; set; }

        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        [Display(Name = "Reemplazo Vacaciones")]
        public Decimal ReemplazoVacaciones { get; set; }


        public DetallePlanillaViewModel()
        {
        }

        public void Fill(CargarDatosContext datacontext, Int32 PlanillaId, Int32 EdificioId)
        {
            baseFill(datacontext);
            this.PlanillaId = PlanillaId;
            planilla = datacontext.context.Planilla.Find(PlanillaId);
            this.EdificioId = EdificioId;
            this.UnidadTiempoId = planilla.UnidadTiempoId;

            if (planilla.Trabajador.AFP != null)
            {
                this.TotalDescuentos = planilla.TotalDescuentos;
                this.SueldoTotalNeto = planilla.SueldoTotalNeto;
                this.SegundaQuincenaNeto = planilla.SegundaQuincenaNeto;
                this.GratificacionesMes = planilla.GratificacionesMes.Value;
                this.CTSMes = planilla.CTSMes;
                this.ReemplazoVacaciones = planilla.ReemplazoVacaciones;
            }
        }

    }
}