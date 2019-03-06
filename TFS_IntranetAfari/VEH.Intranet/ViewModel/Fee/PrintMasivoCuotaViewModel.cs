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
    public class PrintMasivoCuotaViewModel : BaseViewModel
    {
        public Boolean AplicaSeparacion { get; set; } = false;
        public Int32 EdificioId { get; set; }
        public Int32? UnidadTiempoId { get; set; }

        public Edificio Edificio { get; set; }
        public CuotaComun CuotaComun  { get; set; }

        public List<Cuota> LstCuota { get; set; }
        public List<SelectListItem> LstComboUnidadTiempo { get; set; }
        public List<Departamento> LstDepartamento { get; set; }
        public List<Int32> LstFactor { get; set; }
        public List<Leyenda> LstLeyendas { get; set; }
        public Int32 Total { get; set; }

        public Double TotalFactorGasto { get; set; }
        public Double TotalFactorConsumoAgua { get; set; }
        public Double TotalProporcional { get; set; }

        public List<Double> LstFactorGasto { get; set; }
        public List<Double> LstFactorConsumoAgua { get; set; }
        public List<Double> LstFactorProporcional { get; set; }

        public Int32 FactorAreaComun { get; set; }
        public Int32 FactorAlcantarillado { get; set; }
        public Int32 FactorCargoFijo { get; set; }

        public Int32 Factor;

        [Display(Name = "Fecha de Emisión")]
        public DateTime FechaEmision { get; set; }
        [Display(Name = "Fecha de Vencimiento")]
        public DateTime FechaVencimiento { get; set; }
        public String RutaUltimo { get; set; }
        public PrintMasivoCuotaViewModel()
        {
            LstComboUnidadTiempo = new List<SelectListItem>();
            LstCuota = new List<Cuota>();
            LstDepartamento = new List<Departamento>();
            CuotaComun = new CuotaComun();
        }

        public void Fill(CargarDatosContext datacontext)
        {

            baseFill(datacontext);
            Edificio = datacontext.context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);
            LstDepartamento = datacontext.context.Departamento.Where(x => x.EdificioId == EdificioId && x.Estado == ConstantHelpers.EstadoActivo).OrderBy(x=>x.Numero).ToList();
            for (int i = 0; i < LstDepartamento.Count - 1; i++)
            {
                for (int k = i + 1; k < LstDepartamento.Count; k++)
                {
                    Int32 ii = 10000000;
                    Int32 kk = 1000000;
                    Int32.TryParse(LstDepartamento[i].Numero, out ii);
                    Int32.TryParse(LstDepartamento[k].Numero, out kk);
                    Departamento tmpDepartamento = null;
                    if (ii > kk)
                    {
                        tmpDepartamento = LstDepartamento[i];
                        LstDepartamento[i] = LstDepartamento[k];
                        LstDepartamento[k] = tmpDepartamento;
                    }
                    tmpDepartamento = null;

                }
            }

            var lstunidadtiempo = datacontext.context.UnidadTiempo.OrderByDescending(x => x.Anio).ThenByDescending(x => x.Mes).Where(x => x.Estado == ConstantHelpers.EstadoActivo);
            foreach (var item in lstunidadtiempo)
                LstComboUnidadTiempo.Add(new SelectListItem { Value = item.UnidadTiempoId.ToString(), Text = item.Descripcion.ToUpper() });

            LstLeyendas = new List<Leyenda>();
            if (UnidadTiempoId.HasValue)
            { 
                CuotaComun = datacontext.context.CuotaComun.FirstOrDefault(x=>x.UnidadTiempoId == UnidadTiempoId && x.EdificioId == EdificioId) ?? new CuotaComun();
                LstCuota = datacontext.context.Cuota.Where(x => x.UnidadTiempoId == UnidadTiempoId && x.Departamento.EdificioId == EdificioId
                && x.EsExtraordinaria != true).ToList();
            }

            TotalProporcional = LstDepartamento.Count;
            LstFactorProporcional = Enumerable.Repeat<double>(1, (int)TotalProporcional).ToList();

            LstFactorGasto = new List<double>();
            LstFactorConsumoAgua = new List<double>();
            TotalFactorGasto = 0;
            TotalFactorConsumoAgua = 0;
            foreach (var item in LstDepartamento)
            {
                LstFactorGasto.Add((double)(item.FactorGasto??0));
                TotalFactorGasto += (double)(item.FactorGasto ?? 0);
                LstFactorConsumoAgua.Add((double)item.LecturaAgua);
                TotalFactorConsumoAgua += (int)item.LecturaAgua;
            }

            this.FactorAreaComun = getFactor(Edificio.FactorAreaComun);
            this.FactorAlcantarillado = getFactor(Edificio.FactorAlcantarillado);
            this.FactorCargoFijo = getFactor(Edificio.FactorCargoFijo);
            var rec = datacontext.context.ReciboMes.FirstOrDefault( x => x.UnidadTiempoId == UnidadTiempoId && x.EdificioId == EdificioId);
            if (rec != null)
                RutaUltimo = rec.Ruta;
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