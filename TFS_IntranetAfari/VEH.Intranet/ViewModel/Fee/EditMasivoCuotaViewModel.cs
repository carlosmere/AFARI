using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.Controllers;
using VEH.Intranet.Models;
using System.Data.Entity;
using VEH.Intranet.Helpers;
using System.ComponentModel.DataAnnotations;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Fee
{
    public class EditMasivoCuotaViewModel : BaseViewModel
    {
        public Decimal TotalConsumoIndividual { get; set; } = 0;
        public Int32 EdificioId { get; set; }
        public Int32? UnidadTiempoId { get; set; }

        public Edificio Edificio { get; set; }
        public CuotaComun CuotaComun { get; set; }

        public List<Cuota> LstCuota { get; set; }
        public List<Cuota> LstCuotaAnterior { get; set; }
        public List<SelectListItem> LstComboUnidadTiempo { get; set; }
        public List<Departamento> LstDepartamento { get; set; }
        public List<GastoCuotaComun> LstGastoCuotaComun { get; set; }

        public List<Int32> LstFactor { get; set; }

        public Int32 Total { get; set; }

        public Double TotalFactorGasto { get; set; }
        public Double TotalFactorConsumoAgua { get; set; }
        public Double TotalFactorProporcional { get; set; }
        public Double MontoTotal { get; set; }

        public List<Double> LstFactorGasto { get; set; }
        public List<Double> LstFactorConsumoAgua { get; set; }
        public List<Double> LstFactorProporcional { get; set; }
        public List<Double> LstCuotaDefault { get; set; }

        public Int32 FactorAreaComun { get; set; }
        public Int32 FactorAlcantarillado { get; set; }
        public Int32 FactorCargoFijo { get; set; }

        public Int32 Factor;

        [Display(Name = "Fecha de Emisión")]
        public DateTime FechaEmision { get; set; }
        [Display(Name = "Fecha de Vencimiento")]
        public DateTime FechaVencimiento { get; set; }
        public Decimal Acumulado { get; set; }
        public String UltimaUnidadTiempoAcumulado { get; set; }

        public Int32 CantExtraordinariaManual { get; set; } = 0;

        public EditMasivoCuotaViewModel()
        {
            LstComboUnidadTiempo = new List<SelectListItem>();
            LstCuota = new List<Cuota>();
            LstDepartamento = new List<Departamento>();
            CuotaComun = new CuotaComun();
            Total = Factor = 0;
            Acumulado = 0M;
            UltimaUnidadTiempoAcumulado = "No hay Acumulado";
        }

        public void Fill(CargarDatosContext datacontext)
        {
            baseFill(datacontext);
            Gasto gastoAcumulado = datacontext.context.Gasto.OrderBy(x => -x.UnidadTiempo.Orden).FirstOrDefault(x => x.Estado.Equals(ConstantHelpers.EstadoActivo) && x.EdificioId == EdificioId && x.SaldoMes.HasValue);
            if (gastoAcumulado != null)
            {
                Acumulado = gastoAcumulado.SaldoMes.Value;
                UltimaUnidadTiempoAcumulado = gastoAcumulado.UnidadTiempo.Descripcion;
            }
           
            Edificio = datacontext.context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);
            LstDepartamento = datacontext.context.Departamento.Where(x => x.EdificioId == EdificioId && x.Estado == ConstantHelpers.EstadoActivo).ToList();

            for (int i = 0; i < LstDepartamento.Count-1; i++)
            {
                for (int k = i + 1; k < LstDepartamento.Count; k++)
                {
                    Int32 ii =10000000;
                    Int32 kk = 1000000;
                    Int32.TryParse(LstDepartamento[i].Numero,out ii);
                     Int32.TryParse(LstDepartamento[k].Numero,out kk);
                    Departamento tmpDepartamento = null;
                    if(ii>kk)
                    {
                        tmpDepartamento = LstDepartamento[i];
                        LstDepartamento[i] = LstDepartamento[k];
                        LstDepartamento[k] = tmpDepartamento;
                    }
                    tmpDepartamento = null;
                        
                }
            }

            var lstunidadtiempo = datacontext.context.UnidadTiempo.OrderByDescending(x => x.Orden).Where(x => x.Estado == ConstantHelpers.EstadoActivo);
            foreach (var item in lstunidadtiempo)
                LstComboUnidadTiempo.Add(new SelectListItem { Value = item.UnidadTiempoId.ToString(), Text = item.Descripcion.ToUpper() });

            if (UnidadTiempoId.HasValue)
            {
                CuotaComun = datacontext.context.CuotaComun.FirstOrDefault(x => x.UnidadTiempoId == UnidadTiempoId && x.EdificioId == EdificioId) ?? new CuotaComun();


                LstCuota = datacontext.context.Cuota.Where(x => x.UnidadTiempoId == UnidadTiempoId && x.Departamento.EdificioId == EdificioId
                && x.EsExtraordinaria != true).ToList();

                TotalConsumoIndividual = LstCuota.Sum(x => x.Otros) ?? 0;

                CantExtraordinariaManual = datacontext.context.Cuota.Count(x => x.UnidadTiempoId == UnidadTiempoId && x.Departamento.EdificioId == EdificioId
                && x.EsExtraordinaria == true);

                try
                {
                    UnidadTiempo Actual = datacontext.context.UnidadTiempo.FirstOrDefault(Y => Y.UnidadTiempoId == UnidadTiempoId);
                    UnidadTiempo UnidadAnterior = datacontext.context.UnidadTiempo.FirstOrDefault(X => X.Orden == Actual.Orden - 1 && X.Estado == ConstantHelpers.EstadoActivo);
                    LstCuotaAnterior = datacontext.context.Cuota.Where(x => x.UnidadTiempoId == UnidadAnterior.UnidadTiempoId && x.Departamento.EdificioId == EdificioId &&
                    x.EsExtraordinaria != true).ToList();
                }
                catch (Exception ex)
                {
                    LstCuotaAnterior = null;
                }
                LstGastoCuotaComun = datacontext.context.GastoCuotaComun.Where(X => X.CuotaComunId == CuotaComun.CuotaComunId && X.Estado == ConstantHelpers.EstadoActivo).ToList();
            }
            TotalFactorProporcional = LstDepartamento.Count;
            LstFactorProporcional = Enumerable.Repeat<double>(1, (int)TotalFactorProporcional).ToList();

            LstFactorGasto = new List<double>();
            LstFactorConsumoAgua = new List<double>();
            LstCuotaDefault = new List<double>();
            TotalFactorGasto = 0;
            TotalFactorConsumoAgua = 0;
            foreach (var item in LstDepartamento)
            {
                LstFactorGasto.Add((double)(item.FactorGasto ?? 0));
                TotalFactorGasto += (double)(item.FactorGasto ?? 0);
                LstFactorConsumoAgua.Add((double)item.LecturaAgua);
                TotalFactorConsumoAgua += (int)item.LecturaAgua;
                LstCuotaDefault.Add((double)(item.CuotaDefault ?? 0));
            }

            this.FactorAreaComun = getFactor(Edificio.FactorAreaComun);
            this.FactorAlcantarillado = getFactor(Edificio.FactorAlcantarillado);
            this.FactorCargoFijo = getFactor(Edificio.FactorCargoFijo);
        }

        private Int32 getFactor(String tipoFactor)
        {
            Int32 factor = 0;
            switch (tipoFactor)
            {
                case "IGU":
                    factor = 1;
                    break;
                case "GAS":
                    factor = 2;
                    break;
                case "CON":
                    factor = 3;
                    break;
            }
            return factor;
        }
    }
}