using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.Controllers;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Fee
{
    public class CloseCuotaViewModel : BaseViewModel
    {
        public Int32 EdificioId { get; set; }
        public Int32? UnidadTiempoId { get; set; }

        public Edificio Edificio { get; set; }
        public Int32 BalanceUnidadTiempoEdificioId { get; set; }

        public List<Cuota> LstCuota { get; set; }
        public List<Boolean> LstEstadoCuota { get; set; }
        public List<String> LeyendasPorDepartamento { get; set; }
        public List<SelectListItem> LstComboUnidadTiempo { get; set; }
        public List<Leyenda> LstLeyenda { get; set; }
        public List<String> LstObservacion { get; set; }
        public Dictionary<string, List<Cuota>> CuotasPorUnidadTiempo { get; set; }
        public List<Departamento> LstDepartamentos { get; set; }
        public List<String> LstUnidadTiempoString { get; set; }
        public Decimal Acumulado { get; set; }
        public String UltimaUnidadTiempoAcumulado { get; set; }
        public Decimal AcumuladoActual { get; set; }
        public Decimal SaldoActual { get; set; }
        public Decimal GastosActual { get; set; }
        public Decimal IngresosActual { get; set; }
        public String UnidadTiempoAcumuladoActual { get; set; }
        public DateTime FechaActualMora { get; set; }
        public Double MoraUnitariaGuardad { get; set; }
        public String TipoMora { get; set; }
        public CloseCuotaViewModel()
        {
            LstCuota = new List<Cuota>();
            LstEstadoCuota = new List<bool>();
            LstComboUnidadTiempo = new List<SelectListItem>();
            LstObservacion = new List<string>();
            CuotasPorUnidadTiempo = new Dictionary<string, List<Cuota>>();
            LstUnidadTiempoString = new List<string>();
        }
        private void CargarAcumuladoActual(CargarDatosContext datacontext, UnidadTiempo unidadTiempoIdSig)
        {
            #region antiguoMetodo
            //  var edificio = datacontext.context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);

            //  List<Cuota> lstIngresos = new List<Cuota>();
            ////  List<DetalleGasto> lstGastos = new List<DetalleGasto>();
            //  Decimal saldoAnterior = 0M;
            //  var departamentos = edificio.Departamento.ToList();

            //  foreach (var depa in departamentos)
            //  {

            //      Cuota cuota = datacontext.context.Cuota.FirstOrDefault(x => x.DepartamentoId == depa.DepartamentoId && x.UnidadTiempoId == unidadTiempoIdSig.UnidadTiempoId);

            //      if (cuota == null) continue;
            //      lstIngresos.Add(cuota);
            //  }

            //  Gasto Gasto = datacontext.context.Gasto.FirstOrDefault(x => x.EdificioId == EdificioId && x.UnidadTiempoId == unidadTiempoIdSig.UnidadTiempoId && x.Estado.Equals(ConstantHelpers.EstadoActivo));
            //  if (Gasto == null) return;
            //  var lstGastos = datacontext.context.DetalleGasto.Where(x => x.GastoId == Gasto.GastoId && x.Estado.Equals(ConstantHelpers.EstadoActivo)).ToList();

            //  UnidadTiempo objUnidadTiempoAnterior = datacontext.context.UnidadTiempo.FirstOrDefault(x => x.Orden == unidadTiempoIdSig.Orden - 1 && x.Estado.Equals(ConstantHelpers.EstadoActivo));
            //  if (objUnidadTiempoAnterior == null)
            //  {
            //      saldoAnterior = 0M;
            //  }
            //  else
            //  {
            //      var GastoMesAnterior = datacontext.context.Gasto.FirstOrDefault(x => x.EdificioId == EdificioId && x.UnidadTiempoId == objUnidadTiempoAnterior.UnidadTiempoId && x.Estado.Equals(ConstantHelpers.EstadoActivo));
            //      saldoAnterior = GastoMesAnterior.SaldoMes.Value;
            //  }
            //  Decimal TotalIngresosTotal = 0M;
            //  Decimal TotalIngresosMora = 0M;
            //  Decimal TotalIngresosCuota = 0M;
            //  Decimal TotalGastos = 0M;

            //  List<Departamento> LstDepartamentos = new List<Departamento>();
            //  LstDepartamentos = datacontext.context.Departamento.Where(x => x.EdificioId == EdificioId && x.Estado.Equals(ConstantHelpers.EstadoActivo)).ToList();
            //  List<DateTime> LstFechasEmision = new List<DateTime>();

            //  UnidadTiempo unidadTiempoActual = unidadTiempoIdSig;
            //  for (int i = 0; i < lstIngresos.Count; i++)
            //      if (!lstIngresos[i].Pagado)
            //          LstFechasEmision.Add(new DateTime(unidadTiempoActual.Anio, unidadTiempoActual.Mes, edificio.DiaEmisionCuota));
            //      else
            //          LstFechasEmision.Add(DateTime.MinValue);

            //  UnidadTiempo unidadTiempoAnterior = unidadTiempoActual;
            //  while (true)
            //  {
            //      if (unidadTiempoAnterior.Orden == 1) break;

            //      unidadTiempoAnterior = datacontext.context.UnidadTiempo.FirstOrDefault(x => x.Orden == unidadTiempoAnterior.Orden - 1 && x.Estado.Equals(ConstantHelpers.EstadoActivo));
            //      if (unidadTiempoAnterior == null) break;

            //      CuotaComun cuotaComun = datacontext.context.CuotaComun.FirstOrDefault(x => x.EdificioId == EdificioId && x.UnidadTiempoId == unidadTiempoAnterior.UnidadTiempoId);
            //      if (cuotaComun == null || cuotaComun.Pagado) break;

            //      for (int i = 0; i < lstIngresos.Count; i++)
            //          if (!lstIngresos[i].Pagado)
            //          {
            //              LstFechasEmision[i] = new DateTime(unidadTiempoAnterior.Anio, unidadTiempoAnterior.Mes, edificio.DiaEmisionCuota);
            //          }

            //  }

            //  for (int i = 0; i < LstDepartamentos.Count; i++)
            //  {
            //      // int DiasTranscurridosMora = LstFechasEmision[i].Equals(DateTime.MinValue) ? 0 : (DateTime.Now.Date - LstFechasEmision[i].Date).Days - 1; ;
            //      int DiasTranscurridosMora = LstFechasEmision[i].Equals(DateTime.MinValue) ? 0 : (FechaActualMora.Date - LstFechasEmision[i].Date).Days - 1; ;
            //      Decimal moraUnitaria = edificio.TipoMora.Equals(ConstantHelpers.TipoMoraPorcentual) ? edificio.MontoCuota * edificio.PMora.Value / 100M : edificio.PMora.Value;
            //      LstDepartamentos[i].MontoMora = moraUnitaria * DiasTranscurridosMora;
            //  }

            //  foreach (var gasto in lstGastos)
            //  {
            //      TotalGastos += gasto.Monto;
            //  }
            //  foreach (var cuota in lstIngresos)
            //  {
            //      cuota.Mora = cuota.Departamento.OmitirMora ? 0M : cuota.Departamento.MontoMora;
            //      TotalIngresosMora += cuota.Mora;
            //      TotalIngresosCuota += cuota.Total;
            //      TotalIngresosTotal += cuota.Total + cuota.Mora;
            //  }

            //  SaldoActual = TotalIngresosTotal - TotalGastos;

            //  Decimal SaldoAcumulado = 0M;

            //  if (Gasto.SaldoMes != null)
            //      SaldoAcumulado = Gasto.SaldoMes.Value; //Se utiliza el saldo acumulado de ese entonces 
            //  else
            //      SaldoAcumulado = saldoAnterior + SaldoActual;

            //  AcumuladoActual = SaldoAcumulado;
            //  GastosActual = TotalGastos;
            //  IngresosActual = TotalIngresosTotal;
            //  UnidadTiempoAcumuladoActual = unidadTiempoActual.Descripcion;
            #endregion
            Decimal TotalPagosCuotas = datacontext.context.Cuota.Where(X => X.Pagado && X.Departamento.EdificioId == EdificioId).Sum(X => X.Total);
            Decimal TotalIngresosAdicionales = datacontext.context.Ingreso.Where(X => X.EdificioId == EdificioId).Sum(X => X.DetalleIngreso.Sum(Y => Y.Monto));
            Decimal TotalGasto = datacontext.context.Gasto.Where(X => X.EdificioId == EdificioId).Sum(X => X.DetalleGasto.Sum(Y => Y.Monto));
        
            GastosActual = datacontext.context.Gasto.Where(X => X.UnidadTiempoId == UnidadTiempoId && EdificioId == X.EdificioId).Sum(X => X.DetalleGasto.Sum(Y => Y.Monto));
            IngresosActual = datacontext.context.Gasto.Where(X => X.UnidadTiempoId == UnidadTiempoId && EdificioId == X.EdificioId).Sum(X => X.DetalleGasto.Sum(Y => Y.Monto));
            SaldoActual = IngresosActual - GastosActual;
            Acumulado = TotalPagosCuotas+TotalIngresosAdicionales-TotalGasto;
            AcumuladoActual = Acumulado - SaldoActual;


        }
        public void Fill(CargarDatosContext datacontext)
        {
            try
            {
                baseFill(datacontext);

                UnidadTiempo unidadTiempoSig = null;
                Gasto gastoAcumulado = datacontext.context.Gasto.OrderBy(x => -x.UnidadTiempo.Orden).FirstOrDefault(x => x.Estado.Equals(ConstantHelpers.EstadoActivo) && x.EdificioId == EdificioId && x.SaldoMes.HasValue);
                if (gastoAcumulado != null)
                {
                    Acumulado = gastoAcumulado.SaldoMes.Value;
                    UltimaUnidadTiempoAcumulado = gastoAcumulado.UnidadTiempo.Descripcion;
                    unidadTiempoSig = datacontext.context.UnidadTiempo.FirstOrDefault(x => x.Orden == gastoAcumulado.UnidadTiempo.Orden + 1 && x.Estado.Equals(ConstantHelpers.EstadoActivo));
                }
                Edificio = datacontext.context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);
                TipoMora = Edificio.TipoMora;
                MoraUnitariaGuardad = (double)Edificio.PMora.Value;

                List<UnidadTiempo> lstunidadtiempo = datacontext.context.UnidadTiempo.OrderBy(x => -x.Orden).Where(x => x.Estado == ConstantHelpers.EstadoActivo).ToList();
                foreach (var item in lstunidadtiempo)
                    LstComboUnidadTiempo.Add(new SelectListItem { Value = item.UnidadTiempoId.ToString(), Text = item.Descripcion.ToUpper() });

                FechaActualMora = DateTime.Now;

                try
                {
                    if (unidadTiempoSig != null)
                        CargarAcumuladoActual(datacontext, unidadTiempoSig);

                }
                catch (Exception ex)
                {

                }

                if (UnidadTiempoId.HasValue)
                {

                    LstLeyenda = datacontext.context.Leyenda.Where(X => X.BalanceUnidadTiempoEdificio.UnidadDeTiempoId == UnidadTiempoId && X.BalanceUnidadTiempoEdificio.EdificioId == EdificioId).ToList();
                 
                    LeyendasPorDepartamento = new List<string>();

                    LstDepartamentos = new List<Departamento>();
                    LstDepartamentos = datacontext.context.Departamento.Where(x => x.EdificioId == EdificioId && x.Estado.Equals(ConstantHelpers.EstadoActivo)).ToList();

                    UnidadTiempo unidadTiempoActual = datacontext.context.UnidadTiempo.FirstOrDefault(x => x.UnidadTiempoId == UnidadTiempoId);

                    List<DateTime> LstFechasEmision = new List<DateTime>();
                    //LstDepartamentos.ForEach(x => x.MontoMora = );


                    LstCuota = datacontext.context.Cuota.Where(x => x.UnidadTiempoId == UnidadTiempoId && x.Departamento.EdificioId == EdificioId).ToList();


                    CuotasPorUnidadTiempo.Add(unidadTiempoActual.UnidadTiempoId.ToString(), LstCuota);
                    UnidadTiempo unidadTiempoAnterior = unidadTiempoActual;
                    LstUnidadTiempoString.Add(unidadTiempoAnterior.Descripcion.Remove(unidadTiempoAnterior.Descripcion.Length - 5));

                    // DateTime fechaEmisionUnidadTiempoActual = new DateTime(unidadTiempoActual.Anio, unidadTiempoActual.Mes, Edificio.DiaEmisionCuota);
                    // LstDepartamentos.ForEach(x => LstFechasEmision.Add(new DateTime(unidadTiempoAnterior.Anio, unidadTiempoAnterior.Mes, Edificio.DiaEmisionCuota)));
                    for (int i = 0; i < LstCuota.Count; i++)
                    {
                        if (LstCuota[i].Leyenda.HasValue)
                            LeyendasPorDepartamento.Add(LstCuota[i].Leyenda.Value.ToString());
                        else
                        {
                            LeyendasPorDepartamento.Add(" ");
                        }
                        if (!LstCuota[i].Pagado)
                            LstFechasEmision.Add(new DateTime(unidadTiempoAnterior.Anio, unidadTiempoAnterior.Mes, Edificio.DiaEmisionCuota));
                        else
                            LstFechasEmision.Add(DateTime.MinValue);
                    }
                    while (true)
                    {
                        if (unidadTiempoAnterior.Orden == 1) break;

                        unidadTiempoAnterior = datacontext.context.UnidadTiempo.FirstOrDefault(x => x.Orden == unidadTiempoAnterior.Orden - 1 && x.Estado.Equals(ConstantHelpers.EstadoActivo));
                        if (unidadTiempoAnterior == null) break;

                        CuotaComun cuotaComun = datacontext.context.CuotaComun.FirstOrDefault(x => x.EdificioId == EdificioId && x.UnidadTiempoId == unidadTiempoAnterior.UnidadTiempoId);
                        if (cuotaComun == null || cuotaComun.Pagado) break;
                        LstUnidadTiempoString.Add(unidadTiempoAnterior.Descripcion.Remove(unidadTiempoAnterior.Descripcion.Length - 5));
                        LstCuota = datacontext.context.Cuota.Where(x => x.UnidadTiempo.Estado.Equals(ConstantHelpers.EstadoActivo) && x.UnidadTiempo.Mes == unidadTiempoAnterior.Mes && x.UnidadTiempo.Anio == unidadTiempoAnterior.Anio && x.Departamento.EdificioId == EdificioId).ToList();
                        CuotasPorUnidadTiempo.Add(unidadTiempoAnterior.UnidadTiempoId.ToString(), LstCuota);
                        for (int i = 0; i < Math.Min(LstCuota.Count, LstFechasEmision.Count); i++)
                            if (!LstCuota[i].Pagado)
                                LstFechasEmision[i] = new DateTime(unidadTiempoAnterior.Anio, unidadTiempoAnterior.Mes, Edificio.DiaEmisionCuota);

                    }

                    for (int i = 0; i < LstDepartamentos.Count; i++)
                    {
                        int DiasTranscurridosMora = ((LstDepartamentos[i].FechaPago.HasValue ? LstDepartamentos[i].FechaPago.Value.Date : FechaActualMora.Date) - FechaActualMora.Date).Days - 1;
                        DiasTranscurridosMora = DiasTranscurridosMora < 0 ? 0 : DiasTranscurridosMora;
                        Decimal moraUnitaria = Edificio.TipoMora.Equals(ConstantHelpers.TipoMoraPorcentual) ? Edificio.MontoCuota * Edificio.PMora.Value / 100M : Edificio.PMora.Value;
                        LstDepartamentos[i].MontoMora = moraUnitaria * DiasTranscurridosMora;
                        LstDepartamentos[i].FechaPago = LstDepartamentos[i].FechaPago == null ? DateTime.Now : LstDepartamentos[i].FechaPago;
                    }

                    //foreach (Cuota c in LstCuota)
                    //{
                    //    LstEstadoCuota.Add(c.Estado == "FIN");
                    //    if (HasDebt(c)) LstObservacion.Add(ConstantHelpers.DeudasActivas);
                    //    else LstObservacion.Add(ConstantHelpers.DeudasCerradas);
                    //}
                }
            }
            catch (Exception)
            {
                CuotasPorUnidadTiempo = new Dictionary<string, List<Cuota>>();
                LstUnidadTiempoString = new List<string>();
                LstDepartamentos = new List<Departamento>();
            }

        }
        private bool HasDebt(Cuota cuota)
        {
            try
            {
                Departamento departamento = cuota.Departamento;
                List<Cuota> LstCuotaDepartamento = departamento.Cuota.Where(x => x.Estado == "PEN" && x.UnidadTiempoId != cuota.UnidadTiempoId).ToList();
                if (LstCuotaDepartamento.Count > 0) return true;
                return false;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}