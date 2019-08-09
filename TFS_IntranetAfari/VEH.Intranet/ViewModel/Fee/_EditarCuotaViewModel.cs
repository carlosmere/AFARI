using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.Controllers;

namespace VEH.Intranet.ViewModel.Fee
{
    public class _EditarCuotaViewModel
    {
        public Int32 CuotaId { get; set; }
        public Int32 EdifiId { get; set; }
        public String FechaPago { get; set; }
        public Decimal? Mora { get; set; }
        public Int32? Leyenda { get; set; }
        public String Estado { get; set; }
        [Display(Name = "¿Es Adelantado?")]
        public String EsAdelantado { get; set; }
        [Display(Name ="Mostrar en Cuadro de Morosidad")]
        public String NoEsVisibleMorosidad { get; set; }
        public List<SelectListItem> LstEstado { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> LstAdelantado { get; set; } = new List<SelectListItem>();
        public void Fill(CargarDatosContext c, Int32 cuotaId)
        {
            LstEstado.Add(new SelectListItem { Text = "Pagado", Value = "1" });
            LstEstado.Add(new SelectListItem { Text = "Sin Pagar", Value = "0" });

            LstAdelantado.Add(new SelectListItem { Text = "SÍ", Value = "1" });
            LstAdelantado.Add(new SelectListItem { Text = "NO", Value = "0" });

            this.CuotaId = cuotaId;
            var cuota = c.context.Cuota.FirstOrDefault( x => x.CuotaId == this.CuotaId);
            this.EdifiId = cuota.Departamento.EdificioId;
            FechaPago = cuota.FechaPagado == null ? String.Empty : cuota.FechaPagado.Value.ToShortDateString();
            Mora = cuota.Mora;
            Leyenda = cuota.Leyenda;
            Estado = (cuota.Pagado == false ? "0" : "1").ToString();
            EsAdelantado = cuota.EsAdelantado.HasValue ? (cuota.EsAdelantado.Value == true ? "1" : "0") : "0";
            NoEsVisibleMorosidad = cuota.NoEsVisibleMorosidad.HasValue ? (cuota.NoEsVisibleMorosidad.Value == true ? "0" : "1") : "1";
        }   
    }
}