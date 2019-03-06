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

namespace VEH.Intranet.ViewModel.Spending
{
    public class AddEditGastoViewModel : BaseViewModel
    {
        public Int32? GastoId { get; set; }
        public Int32 EdificioId { get; set; }
        public Decimal? CuentasPorCobrarO { get; set; }
        public Decimal? CuentasPorCobrarE { get; set; }
        public Edificio Edificio { get; set; }

        public String Estado { get; set; }
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Int32 UnidadTiempoId { get; set; }
        public DateTime FechaRegistro { get; set; }

        public List<SelectListItem> LstComboUnidadTiempo { get; set; }

        public AddEditGastoViewModel() { }

        public void Fill(CargarDatosContext datacontext)
        {
            baseFill(datacontext);
            Edificio = datacontext.context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);
            if (GastoId.HasValue)
            {
                Gasto gasto = datacontext.context.Gasto.FirstOrDefault(x => x.GastoId == GastoId.Value);
                this.Estado = gasto.Estado;
                this.UnidadTiempoId = gasto.UnidadTiempoId;
                this.FechaRegistro = gasto.FechaRegistro;
                this.CuentasPorCobrarO = gasto.CuentasPorCobrarO;
                this.CuentasPorCobrarE = gasto.CuentasPorCobrarE;
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