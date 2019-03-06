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
    public class AddEditCuotaViewModel : BaseViewModel
    {
        public Int32? CuotaId { get; set; }
        public Int32 EdificioId { get; set; }
        public Int32 DepartamentoId { get; set; }
        public Departamento Departamento { get; set; }
        public Edificio Edificio { get; set; }
        public Decimal LecturaAguaAnterior { get; set; }

        [Display(Name = "Lectura de agua")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Decimal? LecturaAgua { get; set; }

        [Display(Name = "Consumo de mes")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Decimal? ConsumoMes { get; set; }

        [Display(Name = "Consumo de agua")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Decimal? ConsumoAgua { get; set; }

        [Display(Name = "Cuota")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Decimal? Monto { get; set; }
        public Decimal Mora { get; set; }

        [Display(Name = "Total")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Decimal? Total { get; set; }

        [Display(Name = "Consumo S/")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Decimal? ConsumoSoles { get; set; }

        [Display(Name = "Área común S/")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Decimal? AreaComun { get; set; }

        [Display(Name = "Alcantarillado")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Decimal? Alcantarillado { get; set; }

        [Display(Name = "Cargo fijo")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Decimal? CargoFijo { get; set; }

        [Display(Name = "IGV")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Decimal? IGV { get; set; }

        [Display(Name = "Consumo Agua Total")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Decimal? ConsumoAguaTotal { get; set; }

        public String Estado { get; set; }

        [Display(Name = "Unidad de tiempo")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Int32 UnidadTiempoId { get; set; }

        public DateTime FechaRegistro { get; set; }
        public List<SelectListItem> LstComboUnidadTiempo { get; set; }

        public AddEditCuotaViewModel() { }

        public void Fill(CargarDatosContext datacontext)
        {
            baseFill(datacontext);
            Departamento = datacontext.context.Departamento.FirstOrDefault(x => x.DepartamentoId == DepartamentoId);
            Edificio = datacontext.context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);
            var cuotaAnterior = datacontext.context.Cuota.OrderByDescending(x => x.FechaRegistro).FirstOrDefault(x => x.DepartamentoId == DepartamentoId);
            this.LecturaAguaAnterior = cuotaAnterior != null ? cuotaAnterior.LecturaAgua : 0;

            if (CuotaId.HasValue)
            {
                Cuota cuota = datacontext.context.Cuota.FirstOrDefault(x=>x.CuotaId == CuotaId.Value);
                this.LecturaAgua = cuota.LecturaAgua;
                
                this.ConsumoAgua = cuota.ConsumoAgua;
                this.Monto = cuota.ConsumoMes;
                this.Estado = cuota.Estado;
                this.Monto = cuota.Monto;
                this.Mora = cuota.Mora;
                this.Total = cuota.Total;
                this.Estado = cuota.Estado;
                this.UnidadTiempoId = cuota.UnidadTiempoId;
                this.FechaRegistro = cuota.FechaRegistro;
                this.ConsumoSoles = cuota.ConsumoSoles;
                this.AreaComun = cuota.AreaComun;
                this.Alcantarillado = cuota.Alcantarillado;
                this.CargoFijo = cuota.CargoFijo;
                this.IGV = cuota.IGV;
                this.ConsumoAguaTotal = cuota.ConsumoAguaTotal;
            }
        }

        public void FillComboUnidadTiempo(CargarDatosContext datacontext)
        {
            LstComboUnidadTiempo = new List<SelectListItem>();
            var lstunidadtiempo = datacontext.context.UnidadTiempo.OrderBy(x => x.EsActivo).OrderByDescending(x => x.Anio).OrderByDescending(x => x.Mes).Where(x => x.Estado == ConstantHelpers.EstadoActivo && x.EsActivo);
            foreach (var item in lstunidadtiempo)
                LstComboUnidadTiempo.Add(new SelectListItem { Value = item.UnidadTiempoId.ToString(), Text = item.Descripcion });
        }
    }
}