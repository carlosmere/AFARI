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

namespace VEH.Intranet.ViewModel.Fee
{
    public class AddEditCuotaExtraordinariaViewModel : BaseViewModel
    {
        public Int32 EdificioId { get; set; }
        public Int32 UnidadTiempoId { get; set; }
        public Int32? CuotaId { get; set; }

        [Display(Name ="Fecha de Pago")]
        public String FechaPago { get; set; }
        public Decimal? Mora { get; set; }
        [Display(Name = "Monto de Cuota Extraordinaria")]
        public Decimal? MontoExtraordinaria { get; set; }
        public Int32? Leyenda { get; set; }
        public String Estado { get; set; }
        public List<SelectListItem> LstEstado { get; set; } = new List<SelectListItem>();
        public String NombreEdificio { get; set; }
        public String NombreUnidadTiempo { get; set; }
        public Int32 DepartamentoId { get; set; }
        public List<Departamento> LstDepartamento { get; set; } = new List<Departamento>();

        public void Fill(CargarDatosContext c, Int32? cuotaId, Int32 edificioId, Int32 unidadTiempoId)
        {
            baseFill(c);
            this.EdificioId = edificioId;
            this.UnidadTiempoId = unidadTiempoId;
            this.CuotaId = cuotaId;

            LstEstado.Add(new SelectListItem { Text = "Sin Pagar", Value = "0" });
            LstEstado.Add(new SelectListItem { Text = "Pagado", Value = "1" });


            NombreEdificio = c.context.Edificio.FirstOrDefault(x => x.EdificioId == this.EdificioId).Nombre;
            NombreUnidadTiempo = c.context.UnidadTiempo.FirstOrDefault(x => x.UnidadTiempoId == this.UnidadTiempoId).Descripcion;

            LstDepartamento = c.context.Departamento.Where(x => x.EdificioId == this.EdificioId && x.Estado == ConstantHelpers.EstadoActivo).ToList();

            if (this.CuotaId.HasValue)
            {
                var cuota = c.context.Cuota.FirstOrDefault(x => x.CuotaId == this.CuotaId);
                //this.EdifiId = cuota.Departamento.EdificioId;
                FechaPago = cuota.FechaPagado == null ? String.Empty : cuota.FechaPagado.Value.ToShortDateString();
                Mora = cuota.Mora;
                Leyenda = cuota.Leyenda;
                Estado = (cuota.Pagado == false ? "0" : "1").ToString();
                MontoExtraordinaria = cuota.CuotaExtraordinaria;
            }
        }
    }
}