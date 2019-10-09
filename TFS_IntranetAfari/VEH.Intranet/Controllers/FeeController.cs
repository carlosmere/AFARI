using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;
using VEH.Intranet.Filters;
using VEH.Intranet.Helpers;
using VEH.Intranet.Logic;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Fee;
using OfficeOpenXml;
using System.Text.RegularExpressions;
using System.Data.Entity;
using OfficeOpenXml.Style;
using System.Data.Entity.Validation;
using System.Transactions;
using System.Threading.Tasks;
using System.Threading;
//using System.Drawing;

namespace VEH.Intranet.Controllers
{
    [AppAuthorize(AppRol.Administrador)]
    public class FeeController : BaseController
    {
        public ActionResult LstEstadoCuentaBancarioProp(Int32 EdificioId, Int32? Anio)
        {
            DetailEstadoCuentaViewModel ViewModel = new DetailEstadoCuentaViewModel();
            ViewModel.Anio = null;
            ViewModel.Mes = null;
            ViewModel.EdificioId = EdificioId;
            ViewModel.DepartamentoId = SessionHelpers.GetDepartamentoId(Session);
            ViewModel.Fill(CargarDatosContext(), null, this);
            return View(ViewModel);
        }
        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-money")]
        public ActionResult LstCuota(Int32 DepartamentoId, Int32 EdificioId, Int32? np)
        {
            LstCuotaViewModel ViewModel = new LstCuotaViewModel();
            ViewModel.DepartamentoId = DepartamentoId;
            ViewModel.EdificioId = EdificioId;
            ViewModel.Fill(CargarDatosContext(), np);
            return View(ViewModel);
        }
        [AppAuthorize(AppRol.Administrador)]
        public ActionResult _EditarCuota(Int32 CuotaId)
        {
            var model = new _EditarCuotaViewModel();
            model.Fill(CargarDatosContext(), CuotaId);
            return View(model);
        }
        [AppAuthorize(AppRol.Administrador)]
        [HttpPost]
        public ActionResult _EditarCuota(_EditarCuotaViewModel model)
        {

            try
            {
                Cuota Cuota = context.Cuota.FirstOrDefault(x => x.CuotaId == model.CuotaId);

                if (model.FechaPago == null)
                    Cuota.FechaPagado = null;
                else
                    Cuota.FechaPagado = model.FechaPago.ToDateTime();

                Cuota.Mora = model.Mora ?? 0;
                Cuota.Leyenda = model.Leyenda;
                Cuota.Pagado = model.Estado == "0" ? false : true;
                Cuota.EsAdelantado = model.EsAdelantado == "0" ? false : true;
                Cuota.NoEsVisibleMorosidad = model.NoEsVisibleMorosidad == "0" ? true : false;

                context.SaveChanges();
                PostMessage(MessageType.Success);
                return RedirectToAction("CerrarCuota", new { EdificioId = Cuota.Departamento.EdificioId, DepartamentoId = Cuota.DepartamentoId, Estado = String.Empty });
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error);
                return RedirectToAction("CerrarCuota");
            }

        }
        [AppAuthorize(AppRol.Administrador)]
        [HttpPost]
        public ActionResult AnularCuotas(String lstcuotas, Int32 EdificioId, Int32? DepartamentoId)
        {

            try
            {
                var LstCuota = lstcuotas.Split(',');
                foreach (var item in LstCuota)
                {
                    if (!String.IsNullOrEmpty(item))
                    {
                        var Id = item.ToInteger();
                        Cuota Cuota = context.Cuota.FirstOrDefault(x => x.CuotaId == Id);
                        Cuota.FechaPagado = null;
                        Cuota.Mora = 0;
                        Cuota.Leyenda = null;
                        Cuota.Pagado = false;
                    }
                }

                context.SaveChanges();
                PostMessage(MessageType.Success);
                return RedirectToAction("CerrarCuota", new { EdificioId = EdificioId, DepartamentoId = DepartamentoId, Estado = "0" });
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error);
                return RedirectToAction("CerrarCuota", new { EdificioId = EdificioId, DepartamentoId = DepartamentoId, Estado = "0" });
            }

        }
        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-clock-o")]
        public ActionResult CerrarCuota(Int32 EdificioId, Int32? UnidadTiempoInicio, Int32? UnidadTiempoFin, Int32? DepartamentoId, String Estado)
        {
            var model = new CerrarCuotaViewModel();
            model.Fill(CargarDatosContext(), UnidadTiempoInicio, UnidadTiempoFin, DepartamentoId, EdificioId, Estado);
            return View(model);
        }
        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-clock-o")]
        [HttpPost]
        public ActionResult CerrarCuota(CerrarCuotaViewModel model, FormCollection formCollection)
        {
            try
            {
                Leyenda ley = null;
                BalanceUnidadTiempoEdificio BalanceUnidadTiempoEdificio = null;
                BalanceUnidadTiempoEdificio = context.BalanceUnidadTiempoEdificio.FirstOrDefault(X => X.EdificioId == model.EdificioId && X.UnidadDeTiempoId == model.UnidadTiempoFin.Value);
                if (BalanceUnidadTiempoEdificio == null)
                {
                    BalanceUnidadTiempoEdificio = new BalanceUnidadTiempoEdificio();
                    BalanceUnidadTiempoEdificio.EdificioId = model.EdiId.Value;
                    BalanceUnidadTiempoEdificio.FechaDeActualizacion = DateTime.Now;
                    BalanceUnidadTiempoEdificio.GastosTotalesMes = 0;
                    BalanceUnidadTiempoEdificio.IngresosTotalesMes = 0;
                    BalanceUnidadTiempoEdificio.SaldoAcumulado = 0;
                    BalanceUnidadTiempoEdificio.SaldoMes = 0;
                    BalanceUnidadTiempoEdificio.UnidadDeTiempoId = model.UnidadTiempoFin.Value;
                    context.BalanceUnidadTiempoEdificio.Add(BalanceUnidadTiempoEdificio);
                    context.SaveChanges();
                }
                else
                {
                    var LstLeyendas = context.Leyenda.Where(x => x.BalanceUnidadTiempoEdificioId == BalanceUnidadTiempoEdificio.BalanceUnidadTiempoEdificioId).ToList();
                    context.Leyenda.RemoveRange(LstLeyendas);
                }
                var strLeyendas = formCollection[0].ToString().Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var str in strLeyendas)
                {
                    var leyenda = str.Split('.');

                    ley = new Leyenda();
                    ley.BalanceUnidadTiempoEdificioId = context.BalanceUnidadTiempoEdificio.FirstOrDefault(X => X.UnidadDeTiempoId == model.UnidadTiempoFin.Value && X.EdificioId == model.EdificioId).BalanceUnidadTiempoEdificioId;
                    ley.Numero = leyenda[0].ToInteger();
                    ley.Descripcion = leyenda[1].ToString();
                    context.Leyenda.Add(ley);
                    context.SaveChanges();
                }

                var FechasPago = formCollection.AllKeys.Where(X => X.StartsWith("fp-")).ToList();
                foreach (var item in FechasPago)
                {
                    var valor = formCollection[item];
                    if (!String.IsNullOrEmpty(valor))
                    {
                        var CuotaId = item.Replace("fp-", String.Empty).ToInteger();
                        var Cuota = context.Cuota.FirstOrDefault(x => x.CuotaId == CuotaId);
                        Cuota.Pagado = true;
                        Cuota.Mora = formCollection["m-" + CuotaId.ToString()].ToDecimal();
                        Cuota.FechaPagado = valor.ToDateTime();
                        Cuota.Leyenda = String.IsNullOrEmpty(formCollection["l-" + CuotaId.ToString()]) ? 0 : formCollection["l-" + CuotaId.ToString()].ToInteger();
                        Cuota.EsAdelantado = formCollection["adel-" + CuotaId.ToString()] != null ? formCollection["adel-" + CuotaId.ToString()].ToString() == "on" ? true : false : false;
                        var unidadActual = context.UnidadTiempo.FirstOrDefault(x => x.EsActivo);

                        var LstCuotas = context.Cuota.Where(x => x.DepartamentoId == Cuota.DepartamentoId && x.UnidadTiempo.Orden - 1 == Cuota.UnidadTiempo.Orden).ToList();

                        foreach (var c in LstCuotas)
                        {
                            c.Leyenda = Cuota.Leyenda;
                            c.FechaLeyenda = Cuota.FechaPagado;
                        }
                        context.SaveChanges();
                    }
                }
                PostMessage(MessageType.Success);
                return RedirectToAction("CerrarCuota", new { EdificioId = model.EdificioId, DepartamentoId = model.DepartamentoId });
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error);
                return RedirectToAction("CerrarCuota", new { EdificioId = model.EdificioId, DepartamentoId = model.DepartamentoId });
            }
        }
        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-clock-o")]
        public ActionResult CloseCuota(Int32 EdificioId, Int32? UnidadTiempoId)
        {
            CloseCuotaViewModel model = new CloseCuotaViewModel();
            model.EdificioId = EdificioId;
            model.UnidadTiempoId = UnidadTiempoId;
            model.Fill(CargarDatosContext());
            return View(model);
        }
        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-clock-o")]
        [HttpPost]
        public ActionResult CloseCuota(CloseCuotaViewModel viewModel, FormCollection formCollection)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    viewModel.Fill(CargarDatosContext());
                    TryUpdateModel(viewModel);
                    return View(viewModel);
                }
                viewModel.Edificio = context.Edificio.FirstOrDefault(X => X.EdificioId == viewModel.EdificioId && X.Estado == ConstantHelpers.EstadoActivo);
                foreach (var item in viewModel.CuotasPorUnidadTiempo)
                {
                    int unidadTiempoId = Convert.ToInt32(item.Key);
                    UnidadTiempo unidadTiempo = context.UnidadTiempo.FirstOrDefault(x => x.UnidadTiempoId == unidadTiempoId);
                    List<Cuota> LstCuotas = context.Cuota.Where(x => x.Departamento.EdificioId == viewModel.EdificioId && x.UnidadTiempoId == unidadTiempo.UnidadTiempoId && x.UnidadTiempo.Estado == ConstantHelpers.EstadoActivo).ToList();

                    for (int i = 0; i < Math.Min(LstCuotas.Count, item.Value.Count); i++)
                    {

                        if (!LstCuotas[i].Pagado && item.Value[i].Pagado)
                        {
                            var departameto = viewModel.LstDepartamentos.FirstOrDefault(X => X.DepartamentoId == LstCuotas[i].DepartamentoId);
                            if (departameto != null)
                                LstCuotas[i].FechaPagado = departameto.FechaPago;
                            if (!String.IsNullOrEmpty(formCollection["moraDpto" + LstCuotas[i].DepartamentoId]))

                                LstCuotas[i].Mora = Decimal.Parse(formCollection["moraDpto" + LstCuotas[i].DepartamentoId].ToString());
                        }
                        LstCuotas[i].Pagado = item.Value[i].Pagado;


                        if (unidadTiempoId == viewModel.UnidadTiempoId)
                        {
                            //Check
                            LstCuotas[i].Leyenda = viewModel.LeyendasPorDepartamento[i].ToInteger();
                        }

                    }
                    context.CuotaComun.FirstOrDefault(x => x.EdificioId == viewModel.EdificioId && x.UnidadTiempoId == unidadTiempo.UnidadTiempoId).Pagado = LstCuotas.All(x => x.Pagado);
                }
                List<Departamento> LstDepartamentos = context.Departamento.Where(x => x.EdificioId == viewModel.EdificioId && x.Estado.Equals(ConstantHelpers.EstadoActivo)).ToList();
                for (int i = 0; i < LstDepartamentos.Count; i++)
                {
                    LstDepartamentos[i].OmitirMora = viewModel.LstDepartamentos[i].OmitirMora;
                    LstDepartamentos[i].FechaPago = viewModel.LstDepartamentos[i].FechaPago;
                    int DiasTranscurridosMora = (LstDepartamentos[i].FechaPago.Value.Date - viewModel.FechaActualMora.Date).Days - 1;
                    DiasTranscurridosMora = DiasTranscurridosMora < 0 ? 0 : DiasTranscurridosMora;
                    Decimal moraUnitaria = viewModel.Edificio.TipoMora.Equals(ConstantHelpers.TipoMoraPorcentual) ? viewModel.Edificio.MontoCuota * viewModel.Edificio.PMora.Value / 100M : viewModel.Edificio.PMora.Value;
                    LstDepartamentos[i].MontoMora = moraUnitaria * DiasTranscurridosMora;

                }
                //for (int i = 0; i < viewModel.LstCuota.Count; ++i)
                //{
                //    Cuota cuota = context.Cuota.FirstOrDefault(x => x.CuotaId == viewModel.LstCuota[i].CuotaId);
                //    cuota.Mora = viewModel.LstCuota[i].Mora;
                //    cuota.Estado = (viewModel.LstEstadoCuota[i] == true) ? "FIN" : "PEN";
                //}
                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error);
            }
            return RedirectToAction("CloseCuota", new { EdificioId = viewModel.EdificioId });
        }

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-print")]
        public ActionResult PrintMasivoCuota(Int32 EdificioId, Int32? UnidadTiempoId)
        {
            PrintMasivoCuotaViewModel ViewModel = new PrintMasivoCuotaViewModel();
            ViewModel.UnidadTiempoId = UnidadTiempoId;
            ViewModel.EdificioId = EdificioId;
            ViewModel.Fill(CargarDatosContext());
            return View(ViewModel);
        }

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-print")]
        [HttpPost]
        public ActionResult PrintMasivoCuota(PrintMasivoCuotaViewModel viewModel, FormCollection formCollection)
        {
            ViewBag.SyncOrAsync = "Asynchronous";
            try
            {
                var seImprimeCE = formCollection["seImprimeCE"].ToString();

                ReporteLogic reporteLogic = new ReporteLogic();
                reporteLogic.Server = Server;
                reporteLogic.context = context;

                DateTime fechaEmision = viewModel.FechaEmision;
                DateTime fechaVencimiento = viewModel.FechaVencimiento;

                var departamentos = context.Departamento.Where(x => x.EdificioId == viewModel.EdificioId && x.Estado == ConstantHelpers.EstadoActivo);

                var cuotaComun = context.CuotaComun.FirstOrDefault(x => x.EdificioId == viewModel.EdificioId && x.UnidadTiempoId == viewModel.UnidadTiempoId);

                List<Cuota> listaCuota = new List<Cuota>();
                Edificio objEdificio = context.Edificio.Find(viewModel.EdificioId);
                UnidadTiempo objUnidad = context.UnidadTiempo.Find(viewModel.UnidadTiempoId);
                var DicNumeroRecibo = new Dictionary<Int32, long?>();
                List<Cuota> CuotasDelEdifico = context.Cuota.Include(x => x.UnidadTiempo)
                    .Include(x => x.Departamento)
                    .Include(x => x.Departamento.Propietario)
                    .Include(x => x.Departamento.Propietario.Select(y => y.Inquilino))
                    .Include(x => x.Departamento.TipoInmueble)
                    .Include(x => x.ConsumoIndividual)
                    .Where(X => X.Departamento.EdificioId == objEdificio.EdificioId
                && X.UnidadTiempo.Estado == ConstantHelpers.EstadoActivo
                && X.UnidadTiempoId <= objUnidad.UnidadTiempoId).ToList();


                long? ultimoNumeroRecibo = 0;
                List<Cuota> lstcuota = new List<Cuota>();

                foreach (var departamento in departamentos)
                {
                    lstcuota = context.Cuota.Where(x => x.DepartamentoId == departamento.DepartamentoId && x.UnidadTiempoId == viewModel.UnidadTiempoId && x.UnidadTiempo.Estado == ConstantHelpers.EstadoActivo).ToList();

                    if (lstcuota != null && lstcuota.Count > 0)
                    {
                        if (lstcuota.Count == 2)
                        {
                            var ext = lstcuota.FirstOrDefault(x => x.EsExtraordinaria == true);
                            var ord = lstcuota.FirstOrDefault(x => x.EsExtraordinaria == false);

                            lstcuota[0] = ord;
                            lstcuota[1] = ext;
                        }
                        foreach (var cuota in lstcuota)
                        {
                            if (DicNumeroRecibo.ContainsKey(departamento.DepartamentoId) == false)
                            {
                                var nroRecibo = departamento.UnidadTiempoReciboDepartamento.FirstOrDefault(x => x.UnidadTiempoId == viewModel.UnidadTiempoId
                            && x.DepartamentoId == departamento.DepartamentoId);
                                if (nroRecibo != null)
                                {
                                    DicNumeroRecibo.Add(departamento.DepartamentoId, nroRecibo.NumeroRecibo);
                                }
                                else
                                {
                                    try
                                    {
                                        ultimoNumeroRecibo = departamento.UnidadTiempoReciboDepartamento.Where(x => x.DepartamentoId == departamento.DepartamentoId).Max(x => x.NumeroRecibo);
                                    }
                                    catch (Exception ex)
                                    {
                                        ultimoNumeroRecibo = 0;
                                    }
                                    var ultimoRecibo = departamento.UnidadTiempoReciboDepartamento.FirstOrDefault(x => x.NumeroRecibo == ultimoNumeroRecibo);
                                    var diferenciaMes = ultimoRecibo.UnidadTiempo.Orden - objUnidad.Orden;

                                    if (objUnidad.Orden > ultimoRecibo.UnidadTiempo.Orden)
                                    {
                                        DicNumeroRecibo.Add(departamento.DepartamentoId, ultimoRecibo.NumeroRecibo + (departamentos.Count() * Math.Abs(diferenciaMes.Value)));

                                        var utRecibo = new UnidadTiempoReciboDepartamento();
                                        utRecibo.DepartamentoId = departamento.DepartamentoId;
                                        utRecibo.UnidadTiempoId = objUnidad.UnidadTiempoId;
                                        utRecibo.NumeroRecibo = DicNumeroRecibo[departamento.DepartamentoId].Value;
                                        context.UnidadTiempoReciboDepartamento.Add(utRecibo);
                                        //context.SaveChanges();
                                    }
                                    else
                                    {
                                        DicNumeroRecibo.Add(departamento.DepartamentoId, ultimoRecibo.NumeroRecibo - (departamentos.Count() * Math.Abs(diferenciaMes.Value)));

                                        var utRecibo = new UnidadTiempoReciboDepartamento();
                                        utRecibo.DepartamentoId = departamento.DepartamentoId;
                                        utRecibo.UnidadTiempoId = objUnidad.UnidadTiempoId;
                                        utRecibo.NumeroRecibo = DicNumeroRecibo[departamento.DepartamentoId].Value;
                                        context.UnidadTiempoReciboDepartamento.Add(utRecibo);

                                    }
                                }
                            }
                        }
                    }
                }

                context.SaveChanges();

                foreach (var departamento in departamentos)
                {
                    lstcuota = context.Cuota.Where(x => x.DepartamentoId == departamento.DepartamentoId && x.UnidadTiempoId == viewModel.UnidadTiempoId && x.UnidadTiempo.Estado == ConstantHelpers.EstadoActivo).ToList();

                    if (lstcuota != null && lstcuota.Count > 0)
                    {
                        if (lstcuota.Count == 2)
                        {
                            var ext = lstcuota.FirstOrDefault(x => x.EsExtraordinaria == true);
                            var ord = lstcuota.FirstOrDefault(x => x.EsExtraordinaria == false);

                            lstcuota[0] = ord;
                            lstcuota[1] = ext;
                        }
                        Cuota cuotaAux = null;

                        foreach (var cuota in lstcuota)
                        {
                            cuotaAux = cuota;

                            cuotaAux.FechaEmision = fechaEmision;
                            cuotaAux.FechaVencimiento = fechaVencimiento;

                            if (cuotaAux.EsExtraordinaria.HasValue && cuotaAux.EsExtraordinaria.Value)
                            {
                                var validacionExtra = listaCuota.FirstOrDefault(x => x.DepartamentoId == cuotaAux.DepartamentoId);
                                if (validacionExtra != null)
                                {
                                    listaCuota.Remove(validacionExtra);

                                    if (cuotaAux.FechaPagado.HasValue && validacionExtra.FechaPagado.HasValue && cuotaAux.FechaPagado.Value.Month == validacionExtra.FechaPagado.Value.Month)
                                    {
                                        validacionExtra.CuotaExtraordinaria += cuotaAux.CuotaExtraordinaria;
                                        validacionExtra.Total += cuotaAux.CuotaExtraordinaria ?? 0;
                                    }
                                    else if (cuotaAux.FechaPagado.HasValue && validacionExtra.FechaPagado.HasValue && cuotaAux.FechaPagado.Value.Month != validacionExtra.FechaPagado.Value.Month)
                                    {
                                        validacionExtra.Total += cuotaAux.CuotaExtraordinaria ?? 0;
                                    }
                                    else if (cuotaAux.FechaEmision.Value.Month == validacionExtra.FechaEmision.Value.Month)
                                    {
                                        validacionExtra.CuotaExtraordinaria += cuotaAux.CuotaExtraordinaria;
                                        validacionExtra.Total += validacionExtra.CuotaExtraordinaria ?? 0;
                                    }

                                    listaCuota.Add(validacionExtra);
                                }
                                else
                                {
                                    listaCuota.Add(cuotaAux);
                                }
                                //context.Entry(validacionExtra).State = EntityState.Unchanged;
                            }
                            else
                            {
                                listaCuota.Add(cuotaAux);
                            }
                            //context.Entry(cuota).State = EntityState.Unchanged;
                        }
                    }
                }

                var presupuestoMes = listaCuota.Sum(x => x.Monto);
                var totalM2 = departamentos.Sum(x => x.DepartamentoM2 ?? 0) + departamentos.Sum(x => x.EstacionamientoM2 ?? 0) + departamentos.Sum(x => x.EstacionamientoM2 ?? 0);

                UnidadTiempo lastUnidad = context.UnidadTiempo.FirstOrDefault(x => x.Orden == objUnidad.Orden - 1 && x.Estado == ConstantHelpers.EstadoActivo);
                var unidadTiempoActualId = context.UnidadTiempo.FirstOrDefault(X => X.EsActivo == true).UnidadTiempoId;

                reporteLogic.UnidadTiempoActualId = unidadTiempoActualId;

                if (!viewModel.AplicaSeparacion)
                {
                    foreach (Cuota c in listaCuota)
                    {
                        String fileName = reporteLogic.GetReport(c, fechaEmision, fechaVencimiento,
                            presupuestoMes, totalM2, objUnidad, CuotasDelEdifico, lastUnidad, DicNumeroRecibo[c.DepartamentoId]);

                    }
                }
                else
                {         
                    foreach (Cuota c in listaCuota)
                    {
                        var valorExtraordinaria = c.CuotaExtraordinaria ?? 0;
                        c.CuotaExtraordinaria = 0;
                    
                        if (seImprimeCE == "0")
                        {
                            c.Total -= valorExtraordinaria;

                            String fileName = reporteLogic.GetReport(c, fechaEmision, fechaVencimiento,
                                presupuestoMes, totalM2, objUnidad, CuotasDelEdifico, lastUnidad, DicNumeroRecibo[c.DepartamentoId]);
                        }
                        else
                        {
                            c.CuotaExtraordinaria = valorExtraordinaria;
                            String fileName = reporteLogic.GetReport(c, fechaEmision, fechaVencimiento,
                                presupuestoMes, totalM2, objUnidad, CuotasDelEdifico, lastUnidad, DicNumeroRecibo[c.DepartamentoId], true);
                        }

                    }
                }

                reporteLogic.GetReportTable(listaCuota, objUnidad.Descripcion);
                
                var contextaux = new SIVEHEntities();
                using (var ts = new TransactionScope())
                {
                    MemoryStream outputMemoryStream = reporteLogic.ZipFiles();
                    String fileName2 = Server.MapPath("~/Resources") + "\\" + (seImprimeCE == "1" ? "CE-" : "") + "Boletas - " + objEdificio.Nombre + " - " + objUnidad.Descripcion + ".zip";
                    using (FileStream file = new FileStream(fileName2, FileMode.Create, System.IO.FileAccess.Write))
                    {
                        byte[] bytes = new byte[outputMemoryStream.Length];
                        outputMemoryStream.Read(bytes, 0, (int)outputMemoryStream.Length);
                        file.Write(bytes, 0, bytes.Length);

                        PostMessage(MessageType.Success);

                        String FileDownloadName = (seImprimeCE == "1" ? "CE-":"") + "Boletas - " + objEdificio.Nombre + " - " + objUnidad.Descripcion + ".zip";
                        String Lugar = Path.Combine(Server.MapPath("~/Resources"), FileDownloadName);

                        ReciboMes bk = contextaux.ReciboMes.FirstOrDefault(x => x.UnidadTiempoId == viewModel.UnidadTiempoId.Value && x.EdificioId == viewModel.EdificioId);

                        if (bk == null)
                        {
                            bk = new ReciboMes();
                            contextaux.ReciboMes.Add(bk);
                            bk.Ruta = "http://afari.pe/intranet/Resources/" + FileDownloadName;
                            bk.UnidadTiempoId = viewModel.UnidadTiempoId.Value;
                            bk.EdificioId = viewModel.EdificioId;

                        }
                        contextaux.SaveChanges();
                        ts.Complete();
                        return Redirect("http://afari.pe/intranet/Resources/" + FileDownloadName);
                    }
                }

            }
            catch (DbEntityValidationException e)
            {
                var error = String.Empty;
                foreach (var eve in e.EntityValidationErrors)
                {
                    error = String.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        error += " - " + ve.PropertyName + " " + ve.ErrorMessage;
                    }
                }
                PostMessage(MessageType.Error, error);
            }
            //catch (Exception ex)
            //{
            //    PostMessage(MessageType.Error, ex.Message + " " + (ex.InnerException != null ? ex.InnerException.Message : String.Empty));
            //}
            return RedirectToAction("PrintMasivoCuota", new { EdificioId = viewModel.EdificioId });
        }

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-money")]
        public ActionResult EditMasivoCuota(Int32 EdificioId, Int32? UnidadTiempoId)
        {
            EditMasivoCuotaViewModel ViewModel = new EditMasivoCuotaViewModel();
            ViewModel.UnidadTiempoId = UnidadTiempoId;
            ViewModel.EdificioId = EdificioId;
            ViewModel.Fill(CargarDatosContext());
            return View(ViewModel);
        }

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-money")]
        [HttpPost]
        public ActionResult EditMasivoCuota(EditMasivoCuotaViewModel viewModel, FormCollection formCollection)
        {
            try
            {
                //ReporteLogic reporteLogic = new ReporteLogic();
                //reporteLogic.Server = Server;

                //Gastos comunes


                var departamentos = context.Departamento.Where(x => x.EdificioId == viewModel.EdificioId && x.Estado == ConstantHelpers.EstadoActivo);

                var cuotaComun = context.CuotaComun.FirstOrDefault(x => x.EdificioId == viewModel.EdificioId && x.UnidadTiempoId == viewModel.UnidadTiempoId);

                if (cuotaComun == null)
                {
                    cuotaComun = new CuotaComun();
                    cuotaComun.FechaRegistro = DateTime.Now;
                    cuotaComun.UnidadTiempoId = viewModel.UnidadTiempoId.Value;
                    cuotaComun.EdificioId = viewModel.EdificioId;

                    cuotaComun.Estado = ConstantHelpers.EstadoPendiente;
                    cuotaComun.SaldoMes = 0M;
                    cuotaComun.Pagado = false;
                    context.CuotaComun.Add(cuotaComun);
                }
                Decimal TotalOtrosGastos = 0;
                List<GastoCuotaComun> AnterioresGastos = context.GastoCuotaComun.Where(X => X.CuotaComunId == cuotaComun.CuotaComunId).ToList();
                foreach (var item in AnterioresGastos)
                {
                    context.GastoCuotaComun.Remove(item);
                }
                List<Int32> NumeroDeOtrosGastos = formCollection.AllKeys.Where(X => X.Contains("gasto-edificio-concepto-")).Select(Y => Int32.Parse(Y.Replace("gasto-edificio-concepto-", ""))).ToList();
                foreach (var item in NumeroDeOtrosGastos)
                {
                    GastoCuotaComun objGasto = new GastoCuotaComun();

                    objGasto.Concepto = formCollection["gasto-edificio-concepto-" + item.ToString()].ToString();
                    objGasto.Detalle = formCollection["gasto-edificio-detalle-" + item.ToString()].ToString();
                    objGasto.Monto = formCollection["gasto-edificio-monto-" + item.ToString()].ToDecimal();
                    objGasto.Estado = ConstantHelpers.EstadoActivo;
                    objGasto.CuotaComunId = cuotaComun.CuotaComunId;
                    TotalOtrosGastos += objGasto.Monto;
                    context.GastoCuotaComun.Add(objGasto);
                }
                cuotaComun.TotalAreaComun = formCollection["comun-area-comun"].ToDecimal();
                cuotaComun.TotalAlcantarillado = formCollection["comun-alcantarillado"].ToDecimal();
                cuotaComun.TotalCargoFijo = formCollection["comun-cargo-fijo"].ToDecimal();
                cuotaComun.TotalMontoAgua = formCollection["comun-consumo-soles"].ToDecimal();
                cuotaComun.TotalOtrosGastos = TotalOtrosGastos;

                //RENZO - ESTO SE HACE EN EL EXPORTAR
                //    cuotaComun.SaldoMes = formCollection["comun-total"].ToDecimal();
                //if (cuotaComun.Estado.Equals(ConstantHelpers.EstadoPagado) || cuotaComun.Estado.Equals(ConstantHelpers.EstadoCerrado))
                //{
                //    Edificio edificio = context.Edificio.FirstOrDefault(x => x.EdificioId == viewModel.EdificioId);
                //    if (edificio != null)
                //    {
                //        edificio.SaldoAcumulado += cuotaComun.SaldoMes;
                //    }
                //}
                long? ultimoNumeroRecibo = 1;

                foreach (var departamento in departamentos)
                {
                    try
                    {
                        ultimoNumeroRecibo = departamento.UnidadTiempoReciboDepartamento.Where(x => x.DepartamentoId == departamento.DepartamentoId).Max(x => x.NumeroRecibo);
                    }
                    catch (Exception ex)
                    {
                        var ut = new UnidadTiempoReciboDepartamento();
                        ut.DepartamentoId = departamento.DepartamentoId;
                        ut.NumeroRecibo = ultimoNumeroRecibo.Value;
                        ut.UnidadTiempoId = viewModel.UnidadTiempoId.Value;
                        context.UnidadTiempoReciboDepartamento.Add(ut);
                        ultimoNumeroRecibo = ultimoNumeroRecibo + 1;
                    }

                    var cuotaLecturaAnterior = formCollection["comun-lectura-anterior-" + departamento.DepartamentoId];
                    var cuotaLecturaActual = formCollection["cuota-lectura-actual-" + departamento.DepartamentoId];
                    var cuotaConsumoAgua = formCollection["cuota-consumo-agua-" + departamento.DepartamentoId];
                    var cuotaConsumoSoles = formCollection["cuota-consumo-soles-" + departamento.DepartamentoId];
                    var cuotaAreaComun = formCollection["cuota-area-comun-" + departamento.DepartamentoId];
                    var cuotaAlcantarillado = formCollection["cuota-alcantarillado-" + departamento.DepartamentoId];
                    var cuotaCargoFijo = formCollection["cuota-cargo-fijo-" + departamento.DepartamentoId];
                    var cuotaIgv = formCollection["cuota-igv-" + departamento.DepartamentoId];
                    var cuotaConsumoTotalAgua = formCollection["cuota-consumo-total-agua-" + departamento.DepartamentoId];
                    var cuotaCuota = formCollection["cuota-cuota-" + departamento.DepartamentoId];
                    var cuotaExtraordinaria = formCollection["cuota-cuota-extraordinaria-" + departamento.DepartamentoId];
                    var cuotaTotal = formCollection["cuota-total-" + departamento.DepartamentoId];

                    Cuota cuota = context.Cuota.FirstOrDefault(x => x.DepartamentoId == departamento.DepartamentoId && x.UnidadTiempoId == viewModel.UnidadTiempoId && x.UnidadTiempo.Estado == ConstantHelpers.EstadoActivo && x.EsExtraordinaria != true);
                    if (cuota == null)
                    {
                        cuota = new Cuota();
                        cuota.FechaRegistro = DateTime.Now;
                        cuota.FechaEmision = DateTime.Now;
                        cuota.Estado = ConstantHelpers.EstadoPendiente;
                        cuota.DepartamentoId = departamento.DepartamentoId;
                        cuota.UnidadTiempoId = viewModel.UnidadTiempoId.Value;
                        cuota.Pagado = false;
                        cuota.EsExtraordinaria = false;
                        context.Cuota.Add(cuota);
                    }

                    cuota.LecturaAgua = cuotaLecturaActual.ToDecimal();
                    cuota.ConsumoAgua = cuotaConsumoAgua.ToDecimal();
                    cuota.ConsumoMes = cuotaConsumoAgua.ToDecimal();
                    cuota.Monto = cuotaCuota.ToDecimal();
                    cuota.CuotaExtraordinaria = cuotaExtraordinaria.ToDecimal();
                    cuota.Total = cuotaTotal.ToDecimal();
                    cuota.Mora = cuota.Mora; //?????????
                    cuota.UnidadTiempoId = viewModel.UnidadTiempoId.Value;
                    cuota.ConsumoSoles = cuotaConsumoSoles.ToDecimal();
                    cuota.AreaComun = cuotaAreaComun.ToDecimal();
                    cuota.Alcantarillado = cuotaAlcantarillado.ToDecimal();
                    cuota.CargoFijo = cuotaCargoFijo.ToDecimal();
                    cuota.IGV = cuotaIgv.ToDecimal();
                    cuota.ConsumoAguaTotal = cuotaConsumoTotalAgua.ToDecimal();
                    cuota.LecturaAnterior = cuotaLecturaAnterior.ToDecimal();
                    //context.SaveChanges();
                    //String fileName = reporteLogic.GetReport(cuota);
                }
                context.SaveChanges();
                //reporteLogic.ZipFiles();
                PostMessage(MessageType.Success);
            }

            catch (Exception ex)
            {
                PostMessage(MessageType.Error);
            }
            return RedirectToAction("EditMasivoCuota", new { EdificioId = viewModel.EdificioId });

        }

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-print")]
        public ActionResult ExportTextFile(Int32 UnidadTiempoId, Int32 EdificioId)
        {
            try
            {
                ReporteLogic reportLogic = new ReporteLogic();
                reportLogic.Server = Server;
                reportLogic.context = context;
                List<Cuota> cuotas = context.Cuota.Where(x => x.UnidadTiempoId == UnidadTiempoId && x.Departamento.EdificioId == EdificioId && x.UnidadTiempo.Estado == ConstantHelpers.EstadoActivo).ToList();

                UnidadTiempo objUnidadTiempo = context.UnidadTiempo.FirstOrDefault(x => x.UnidadTiempoId == UnidadTiempoId);
                var cuotasPendientes = context.Cuota.Where(x => x.UnidadTiempoId != UnidadTiempoId && x.UnidadTiempo.Estado.Equals(ConstantHelpers.EstadoActivo) && x.UnidadTiempo.Orden < objUnidadTiempo.Orden && x.Departamento.EdificioId == EdificioId && x.Estado == ConstantHelpers.ESTADO_PENDIENTE).OrderBy(y => -y.UnidadTiempo.Orden).ToList();
                foreach (var cuota in cuotasPendientes)
                {
                    // Se pidió solo ingresar las cuotas del mes actual e ignorar las anteriores
                    //cuotas.Add(cuota);
                }
                MemoryStream outputMemoryStream = reportLogic.ExportToBankFormat(cuotas);
                return File(outputMemoryStream, "text/plain", "ReporteBCP.txt");
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error);
            }

            return RedirectToAction("PrintMasivoCuota", new { EdificioId = EdificioId });
        }
        public ActionResult DescargarCuadroMoroso(Int32 EdificioId)
        {
            var ruta = Server.MapPath(@"~\Files\CuadroMoroso.xlsx");
            try
            {
                var Edificio = context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId).Nombre;
                System.Drawing.Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#80cd32");
                using (FileStream fs = System.IO.File.OpenRead(ruta))
                using (ExcelPackage excelPackage = new ExcelPackage(fs))
                {
                    ExcelWorkbook excelWorkBook = excelPackage.Workbook;
                    ExcelWorksheet excelWorksheet = excelWorkBook.Worksheets.FirstOrDefault();
                    if (excelWorksheet != null)
                    {
                        var unidadTiempoActivo = context.UnidadTiempo.FirstOrDefault(X => X.EsActivo);

                        var LstCuotasT = context.Cuota.Include(x => x.Departamento)
                            .Include(x => x.UnidadTiempo)
                            .Include(x => x.Departamento.Propietario)
                            .Where(x => x.Departamento.EdificioId == EdificioId
                            && x.Pagado == false
                            && x.UnidadTiempoId < unidadTiempoActivo.UnidadTiempoId
                            && x.UnidadTiempo.Estado == ConstantHelpers.EstadoActivo
                            && (x.NoEsVisibleMorosidad == null || x.NoEsVisibleMorosidad == false)).OrderBy(x => x.UnidadTiempo.Orden).ThenBy(x => x.CuotaId).ToList();

                        List<Cuota> LstCuotas = new List<Cuota>();

                        foreach (var cuota in LstCuotasT)
                        {
                            if (cuota.EsExtraordinaria.HasValue && cuota.EsExtraordinaria.Value)
                            {
                                var validacionExtra = LstCuotas.FirstOrDefault(x => x.DepartamentoId == cuota.DepartamentoId);
                                if (validacionExtra != null)
                                {

                                    if (cuota.UnidadTiempo.Mes == validacionExtra.UnidadTiempo.Mes)
                                    {
                                        LstCuotas.Remove(validacionExtra);
                                        validacionExtra.CuotaExtraordinaria += cuota.CuotaExtraordinaria;
                                        validacionExtra.Total += cuota.CuotaExtraordinaria ?? 0;
                                        LstCuotas.Add(validacionExtra);
                                    }
                                    else if (cuota.UnidadTiempo.Mes != validacionExtra.UnidadTiempo.Mes)
                                    {
                                        //validacionExtra.Total += cuota.CuotaExtraordinaria ?? 0;
                                        LstCuotas.Add(cuota);
                                    }
                                    else if (cuota.FechaEmision.Value.Month == validacionExtra.FechaEmision.Value.Month)
                                    {
                                        LstCuotas.Remove(validacionExtra);
                                        validacionExtra.CuotaExtraordinaria += cuota.CuotaExtraordinaria;
                                        validacionExtra.Total += validacionExtra.CuotaExtraordinaria ?? 0;
                                        LstCuotas.Add(validacionExtra);
                                    }


                                }
                                else
                                {
                                    LstCuotas.Add(cuota);
                                }

                            }
                            else
                            {
                                LstCuotas.Add(cuota);
                            }
                        }

                        //var LstCuotas = context.usp_SelCuotasMoras(EdificioId, unidadTiempoActivo.UnidadTiempoId).ToList();

                        var Mes = String.Empty;
                        var NombreInquilino = String.Empty;
                        var ContNombreInquilino = 0;
                        Int32 Col = 4;
                        Dictionary<Int32, Int32> LstMeses = new Dictionary<Int32, Int32>();
                        foreach (var item in LstCuotas)
                        {
                            if (!LstMeses.ContainsKey(item.UnidadTiempo.Orden.Value))
                            {
                                excelWorksheet.Cells[7, Col].Value = item.UnidadTiempo.Descripcion.Substring(0, 3) + "-" + item.UnidadTiempo.Descripcion.Substring(item.UnidadTiempo.Descripcion.Length - 4);
                                excelWorksheet.Cells[7, Col].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                                excelWorksheet.Cells[7, Col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                excelWorksheet.Cells[7, Col].Style.Fill.BackgroundColor.SetColor(colFromHex);

                                Mes = item.UnidadTiempo.Descripcion;
                                LstMeses.Add(item.UnidadTiempo.Orden.Value, Col);
                                Col++;
                            }
                        }
                        excelWorksheet.Cells[7, Col].Value = "Total";
                        excelWorksheet.Cells[7, Col].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                        excelWorksheet.Cells[7, Col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        excelWorksheet.Cells[7, Col].Style.Fill.BackgroundColor.SetColor(colFromHex);

                        //excelWorksheet.Cells["A5"].Value = "EDIFICIO " + Edificio;
                        excelWorksheet.Cells[4, 1, 4, Col].Merge = true;
                        excelWorksheet.Cells[5, 1, 5, Col].Merge = true;

                        excelWorksheet.Cells["A4"].Value = "MOROSIDAD CUOTAS ORDINARIAS";
                        excelWorksheet.Cells["A4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        excelWorksheet.Cells["A5"].Value = "EDIFICIO " + Edificio;
                        excelWorksheet.Cells["A5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        Int32 i = 8;
                        List<Int32> LstDepartamentoId = new List<Int32>();
                        //LstCuotas = LstCuotas.OrderBy(x => x.UnidadTiempoId).ThenBy(x => x.DepartamentoId).ToList();
                        LstCuotas = LstCuotas.OrderBy(x => x.DepartamentoId).ToList();
                        decimal? TotalGeneral = 0;
                        foreach (var item in LstCuotas)
                        {
                            if (LstDepartamentoId.Contains(item.DepartamentoId) == false)
                            {
                                decimal? Total = 0;
                                var objTitular = item.Departamento.Propietario.FirstOrDefault(x => x.ParentescoTitular.Contains("Titular") && x.Estado == ConstantHelpers.EstadoActivo);

                                if (objTitular == null)
                                    objTitular = item.Departamento.Propietario.FirstOrDefault(x => x.Estado == ConstantHelpers.EstadoActivo);


                                if (objTitular != null)
                                {
                                    NombreInquilino = objTitular.Inquilino.FirstOrDefault(x => x.Estado == ConstantHelpers.EstadoActivo) == null ? String.Empty : objTitular.Inquilino.FirstOrDefault(x => x.Estado == ConstantHelpers.EstadoActivo).Nombres;
                                }
                                else
                                {
                                    NombreInquilino = String.Empty;
                                }
                                if (!String.IsNullOrEmpty(NombreInquilino))
                                {
                                    ContNombreInquilino++;
                                }

                                foreach (var mes in LstMeses)
                                {
                                    var cuota = LstCuotas.FirstOrDefault(x => x.Departamento.Numero == item.Departamento.Numero && x.UnidadTiempo.Orden == mes.Key);
                                    //excelWorksheet.Cells[i, mes.Value].Value = cuota != null ? String.Format("{0:N}", cuota.Total - cuota.CuotaExtraordinaria) : String.Empty;
                                    excelWorksheet.Cells[i, mes.Value].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                                    //Total += cuota != null ? (cuota.Total - cuota.CuotaExtraordinaria) : 0;

                                    if (cuota != null)
                                    {
                                        if (objTitular.FechaCreacion.HasValue)
                                        {
                                            var fechaComparar = new DateTime();
                                            if (cuota.FechaVencimiento.HasValue)
                                            {
                                                try
                                                {
                                                    fechaComparar = new DateTime(cuota.FechaVencimiento.Value.Year, cuota.UnidadTiempo.Mes, cuota.FechaVencimiento.Value.Day);
                                                }
                                                catch (Exception ex)
                                                {
                                                    try
                                                    {
                                                        fechaComparar = new DateTime(cuota.FechaVencimiento.Value.Year, cuota.UnidadTiempo.Mes, cuota.FechaVencimiento.Value.Day - 1);
                                                    }
                                                    catch (Exception ex2)
                                                    {
                                                        fechaComparar = new DateTime(cuota.FechaVencimiento.Value.Year, cuota.UnidadTiempo.Mes, cuota.FechaVencimiento.Value.Day - 3);
                                                    }
                                                }
                                            }
                                            //else
                                            //{
                                            //    fechaComparar = new DateTime(cuota.UnidadTiempo.Anio, cuota.UnidadTiempo.Mes + 1, cuota.Departamento.Edificio.DiaMora ?? 30);
                                            //}
                                            else
                                            {
                                                if (cuota.UnidadTiempo.Mes + 1 >= 13)
                                                {
                                                    fechaComparar = new DateTime(cuota.UnidadTiempo.Anio + 1, 1, cuota.Departamento.Edificio.DiaMora ?? 30);
                                                }
                                                else
                                                    fechaComparar = new DateTime(cuota.UnidadTiempo.Anio, cuota.UnidadTiempo.Mes + 1, cuota.Departamento.Edificio.DiaMora ?? 30);
                                            }


                                            if (fechaComparar >= objTitular.FechaCreacion.Value.Date)
                                            {
                                                excelWorksheet.Cells[i, mes.Value].Value = cuota != null ? String.Format("{0:N}", cuota.Total - cuota.CuotaExtraordinaria) : String.Empty;
                                                Total += cuota != null ? ((cuota.Total - cuota.CuotaExtraordinaria).Value) : 0;
                                            }
                                            else
                                            {
                                                excelWorksheet.Cells[i, mes.Value].Value = String.Empty;
                                            }
                                        }
                                        else
                                        {
                                            excelWorksheet.Cells[i, mes.Value].Value = cuota != null ? String.Format("{0:N}", cuota.Total - cuota.CuotaExtraordinaria) : String.Empty;
                                            Total += cuota != null ? ((cuota.Total - cuota.CuotaExtraordinaria).Value) : 0;
                                        }
                                    }
                                    else
                                    {
                                        excelWorksheet.Cells[i, mes.Value].Value = cuota != null ? String.Format("{0:N}", cuota.Total - cuota.CuotaExtraordinaria) : String.Empty;
                                        Total += cuota != null ? ((cuota.Total - cuota.CuotaExtraordinaria).Value) : 0;
                                    }
                                }
                                if (Total > 0)
                                {
                                    excelWorksheet.Cells["A" + i].Value = item.Departamento.Numero;
                                    excelWorksheet.Cells["A" + i].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                                    excelWorksheet.Cells["B" + i].Value = objTitular != null ? objTitular.Nombres : String.Empty;
                                    excelWorksheet.Cells["B" + i].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                                    excelWorksheet.Cells["C" + i].Value = NombreInquilino;
                                    excelWorksheet.Cells["C" + i].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                                    TotalGeneral += Total;
                                    excelWorksheet.Cells[i, Col].Value = Total;
                                    excelWorksheet.Cells[i, Col].Style.Font.Bold = true;
                                    excelWorksheet.Cells[i, Col].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                                    i++;
                                }
                                //TotalGeneral = 0;
                                Total = 0;
                                //LstDepartamentoId.Add(item.DepartamentoId);

                                //var lstHistoria = context.DepartamentoHistorico.Where(x => x.DepartamentoId == item.DepartamentoId && x.Fecha < objTitular.FechaCreacion).ToList();
                                var lstHistoria = context.DepartamentoHistorico.Where(x => x.DepartamentoId == item.DepartamentoId && x.Propietario.FechaCreacion < objTitular.FechaCreacion).ToList();
                                if (lstHistoria.Count > 0)
                                {
                                    var objTitular2 = item.Departamento.Propietario.FirstOrDefault(x => x.ParentescoTitular.Contains("Titular") && x.Estado == ConstantHelpers.EstadoActivo);

                                    if (objTitular2 == null)
                                        objTitular2 = item.Departamento.Propietario.FirstOrDefault(x => x.Estado == ConstantHelpers.EstadoActivo);

                                    foreach (var historia in lstHistoria)
                                    {
                                        objTitular = historia.Propietario;

                                        if (objTitular != null)
                                        {
                                            NombreInquilino = objTitular.Inquilino.FirstOrDefault(x => x.Estado == ConstantHelpers.EstadoActivo) == null ? String.Empty : objTitular.Inquilino.FirstOrDefault(x => x.Estado == ConstantHelpers.EstadoActivo).Nombres;
                                        }
                                        else
                                        {
                                            NombreInquilino = String.Empty;
                                        }
                                        if (!String.IsNullOrEmpty(NombreInquilino))
                                        {
                                            ContNombreInquilino++;
                                        }

                                        foreach (var mes in LstMeses)
                                        {
                                            //Total = 0;
                                            var cuota = LstCuotas.FirstOrDefault(x => x.Departamento.Numero == item.Departamento.Numero && x.UnidadTiempo.Orden == mes.Key);
                                            //excelWorksheet.Cells[i, mes.Value].Value = cuota != null ? String.Format("{0:N}", cuota.Total - cuota.CuotaExtraordinaria) : String.Empty;
                                            excelWorksheet.Cells[i, mes.Value].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                                            //Total += cuota != null ? (cuota.Total - cuota.CuotaExtraordinaria) : 0;

                                            if (cuota != null)
                                            {
                                                var fechaComparar = new DateTime();
                                                if (cuota.FechaVencimiento.HasValue)
                                                {
                                                    try
                                                    {
                                                        fechaComparar = new DateTime(cuota.FechaVencimiento.Value.Year, cuota.UnidadTiempo.Mes, cuota.FechaVencimiento.Value.Day);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        try
                                                        {
                                                            fechaComparar = new DateTime(cuota.FechaVencimiento.Value.Year, cuota.UnidadTiempo.Mes, cuota.FechaVencimiento.Value.Day - 1);
                                                        }
                                                        catch (Exception ex2)
                                                        {
                                                            fechaComparar = new DateTime(cuota.FechaVencimiento.Value.Year, cuota.UnidadTiempo.Mes, cuota.FechaVencimiento.Value.Day - 3);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (cuota.UnidadTiempo.Mes + 1 >= 13)
                                                    {
                                                        fechaComparar = new DateTime(cuota.UnidadTiempo.Anio + 1, 1, cuota.Departamento.Edificio.DiaMora ?? 30);
                                                    }
                                                    else
                                                        fechaComparar = new DateTime(cuota.UnidadTiempo.Anio, cuota.UnidadTiempo.Mes + 1, cuota.Departamento.Edificio.DiaMora ?? 30);
                                                }


                                                if (objTitular.FechaCreacion.HasValue)
                                                {
                                                    //if (cuota.FechaEmision >= objTitular.FechaCreacion.Value.Date && cuota.FechaEmision <= historia.Fecha)
                                                    if (fechaComparar >= historia.Propietario.FechaCreacion.Value.Date &&
                                        (fechaComparar < objTitular2.FechaCreacion.Value.Date
                                        || (fechaComparar.Month == historia.Propietario.FechaCreacion.Value.Month
                                        && fechaComparar.Year == historia.Propietario.FechaCreacion.Value.Year)
                                        ))
                                                    {
                                                        excelWorksheet.Cells[i, mes.Value].Value = cuota != null ? String.Format("{0:N}", cuota.Total - cuota.CuotaExtraordinaria) : String.Empty;
                                                        Total += cuota != null ? ((cuota.Total - cuota.CuotaExtraordinaria).Value) : 0;
                                                    }
                                                    else
                                                    {
                                                        excelWorksheet.Cells[i, mes.Value].Value = String.Empty;
                                                    }
                                                }
                                                else
                                                {
                                                    excelWorksheet.Cells[i, mes.Value].Value = cuota != null ? String.Format("{0:N}", cuota.Total - cuota.CuotaExtraordinaria) : String.Empty;
                                                    Total += cuota != null ? ((cuota.Total - cuota.CuotaExtraordinaria).Value) : 0;
                                                }
                                            }
                                            else
                                            {
                                                excelWorksheet.Cells[i, mes.Value].Value = cuota != null ? String.Format("{0:N}", cuota.Total - cuota.CuotaExtraordinaria) : String.Empty;
                                                Total += cuota != null ? ((cuota.Total - cuota.CuotaExtraordinaria).Value) : 0;
                                            }
                                        }

                                        if (Total > 0)
                                        {
                                            excelWorksheet.Cells["A" + i].Value = item.Departamento.Numero;
                                            excelWorksheet.Cells["A" + i].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                                            excelWorksheet.Cells["B" + i].Value = objTitular != null ? objTitular.Nombres : String.Empty;
                                            excelWorksheet.Cells["B" + i].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                                            excelWorksheet.Cells["C" + i].Value = NombreInquilino;
                                            excelWorksheet.Cells["C" + i].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                                            TotalGeneral += Total;
                                            excelWorksheet.Cells[i, Col].Value = Total;
                                            excelWorksheet.Cells[i, Col].Style.Font.Bold = true;
                                            excelWorksheet.Cells[i, Col].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                                            i++;
                                        }
                                    }
                                }

                                LstDepartamentoId.Add(item.DepartamentoId);
                            }

                        }
                        excelWorksheet.Cells[i, Col].Value = TotalGeneral;
                        excelWorksheet.Cells[i, Col].Style.Font.Bold = true;
                        excelWorksheet.Cells[i, Col].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                        excelWorksheet.Cells["A" + i].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                        excelWorksheet.Cells["B" + i].Value = "Totales:";
                        excelWorksheet.Cells["B" + i].Style.Font.Bold = true;
                        excelWorksheet.Cells["B" + i].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                        excelWorksheet.Cells["C" + i].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                        foreach (var mes in LstMeses)
                        {
                            excelWorksheet.Cells[i, mes.Value].Value = LstCuotas.Where(x => x.UnidadTiempo.Orden == mes.Key).Sum(x => x.Total) - LstCuotas.Where(x => x.UnidadTiempo.Orden == mes.Key).Sum(x => x.CuotaExtraordinaria);
                            excelWorksheet.Cells[i, mes.Value].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                            excelWorksheet.Cells[i, mes.Value].Style.Font.Bold = true;
                        }

                        if (ContNombreInquilino == 0)
                        {
                            excelWorksheet.DeleteColumn(3);
                        }

                        i += 5;
                        LstCuotas = LstCuotas.Where(x => x.CuotaExtraordinaria > 0).OrderBy(x => x.UnidadTiempo.Orden).ToList();

                        if (LstCuotas.Count > 0)
                        {
                            excelWorksheet.Cells["A" + i].Value = "Dpto.";
                            excelWorksheet.Cells["A" + i].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            excelWorksheet.Cells["A" + i].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            excelWorksheet.Cells["A" + i].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                            excelWorksheet.Cells["A" + i].Style.Fill.BackgroundColor.SetColor(colFromHex);
                            excelWorksheet.Cells["A" + i].Style.Font.Size = 8;
                            excelWorksheet.Cells["A" + i].Style.Font.Bold = true;

                            excelWorksheet.Cells["B" + i].Value = "Propietario";
                            excelWorksheet.Cells["B" + i].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            excelWorksheet.Cells["B" + i].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            excelWorksheet.Cells["B" + i].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                            excelWorksheet.Cells["B" + i].Style.Fill.BackgroundColor.SetColor(colFromHex);
                            excelWorksheet.Cells["B" + i].Style.Font.Size = 8;
                            excelWorksheet.Cells["B" + i].Style.Font.Bold = true;



                            Col = 3;
                            LstMeses = new Dictionary<Int32, Int32>();
                            foreach (var item in LstCuotas)
                            {
                                if (!LstMeses.ContainsKey(item.UnidadTiempo.Orden.Value))
                                {
                                    excelWorksheet.Cells[i, Col].Value = item.UnidadTiempo.Descripcion.Substring(0, 3) + "-" + item.UnidadTiempo.Descripcion.Substring(item.UnidadTiempo.Descripcion.Length - 4);
                                    excelWorksheet.Cells[i, Col].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                                    excelWorksheet.Cells[i, Col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                    excelWorksheet.Cells[i, Col].Style.Fill.BackgroundColor.SetColor(colFromHex);
                                    excelWorksheet.Cells[i, Col].Style.Font.Size = 8;
                                    excelWorksheet.Cells[i, Col].Style.Font.Bold = true;
                                    excelWorksheet.Cells[i, Col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    Mes = item.UnidadTiempo.Descripcion;
                                    LstMeses.Add(item.UnidadTiempo.Orden.Value, Col);
                                    Col++;
                                }
                            }
                            excelWorksheet.Cells[i, Col].Value = "Total";
                            excelWorksheet.Cells[i, Col].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                            excelWorksheet.Cells[i, Col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            excelWorksheet.Cells[i, Col].Style.Fill.BackgroundColor.SetColor(colFromHex);
                            excelWorksheet.Cells[i, Col].Style.Font.Size = 8;
                            excelWorksheet.Cells[i, Col].Style.Font.Bold = true;
                            excelWorksheet.Cells[i, Col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                            excelWorksheet.Cells[(i - 2), 1, (i - 2), Col].Merge = true;
                            excelWorksheet.Cells["A" + (i - 2)].Value = "MOROSIDAD CUOTAS EXTRAORDINARIAS";
                            excelWorksheet.Cells["A" + (i - 2)].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            excelWorksheet.Cells["A" + (i - 2)].Style.Font.Size = 12;
                            excelWorksheet.Cells["A" + (i - 2)].Style.Font.Bold = true;


                            i++;
                            LstDepartamentoId = new List<Int32>();
                            LstCuotas = LstCuotas.OrderBy(x => x.DepartamentoId).ToList();
                            TotalGeneral = 0;
                            foreach (var item in LstCuotas)
                            {
                                if (LstDepartamentoId.Contains(item.DepartamentoId) == false)
                                {
                                    decimal? Total = 0;
                                    var objTitular = item.Departamento.Propietario.FirstOrDefault(x => x.ParentescoTitular.Contains("Titular") && x.Estado == ConstantHelpers.EstadoActivo);
                                    if (objTitular == null)
                                        objTitular = item.Departamento.Propietario.FirstOrDefault(x => x.Estado == ConstantHelpers.EstadoActivo);

                                    excelWorksheet.Cells["A" + i].Value = item.Departamento.Numero;
                                    excelWorksheet.Cells["A" + i].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                                    excelWorksheet.Cells["B" + i].Value = objTitular != null ? objTitular.Nombres : String.Empty;
                                    excelWorksheet.Cells["B" + i].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                                    foreach (var mes in LstMeses)
                                    {
                                        var cuota = LstCuotas.FirstOrDefault(x => x.Departamento.Numero == item.Departamento.Numero && x.UnidadTiempo.Orden == mes.Key);
                                        excelWorksheet.Cells[i, mes.Value].Value = cuota != null ? String.Format("{0:N}", cuota.CuotaExtraordinaria) : String.Empty;
                                        excelWorksheet.Cells[i, mes.Value].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                                        Total += cuota != null ? cuota.CuotaExtraordinaria : 0;

                                        try
                                        {
                                            //var lstHistoria = context.DepartamentoHistorico.Where(x => x.DepartamentoId == cuota.DepartamentoId && x.Propietario.ParentescoTitular == "Titular" && x.Fecha < objTitular.FechaCreacion).ToList();
                                            //var lstHistoria = context.DepartamentoHistorico.Where(x => x.DepartamentoId == cuota.DepartamentoId && x.Fecha < objTitular.FechaCreacion).ToList();
                                            var lstHistoria = context.DepartamentoHistorico.Where(x => x.DepartamentoId == cuota.DepartamentoId && x.Propietario.FechaCreacion < objTitular.FechaCreacion).ToList();
                                            if (lstHistoria.Count == 0)
                                            {
                                                lstHistoria = context.DepartamentoHistorico.Where(x => x.DepartamentoId == cuota.DepartamentoId).ToList();
                                            }

                                            var objTitular2 = item.Departamento.Propietario.FirstOrDefault(x => x.ParentescoTitular.Contains("Titular") && x.Estado == ConstantHelpers.EstadoActivo);

                                            if (objTitular2 == null)
                                                objTitular2 = item.Departamento.Propietario.FirstOrDefault(x => x.Estado == ConstantHelpers.EstadoActivo);

                                            var fechaComparar = new DateTime();
                                            if (cuota.FechaVencimiento.HasValue)
                                            {
                                                try
                                                {
                                                    fechaComparar = new DateTime(cuota.FechaVencimiento.Value.Year, cuota.UnidadTiempo.Mes, cuota.FechaVencimiento.Value.Day);
                                                }
                                                catch (Exception ex)
                                                {
                                                    try
                                                    {
                                                        fechaComparar = new DateTime(cuota.FechaVencimiento.Value.Year, cuota.UnidadTiempo.Mes, cuota.FechaVencimiento.Value.Day - 1);
                                                    }
                                                    catch (Exception ex2)
                                                    {
                                                        fechaComparar = new DateTime(cuota.FechaVencimiento.Value.Year, cuota.UnidadTiempo.Mes, cuota.FechaVencimiento.Value.Day - 3);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (cuota.UnidadTiempo.Mes + 1 >= 13)
                                                {
                                                    fechaComparar = new DateTime(cuota.UnidadTiempo.Anio + 1, 1, cuota.Departamento.Edificio.DiaMora ?? 30);
                                                }
                                                else
                                                    fechaComparar = new DateTime(cuota.UnidadTiempo.Anio, cuota.UnidadTiempo.Mes + 1, cuota.Departamento.Edificio.DiaMora ?? 30);
                                            }


                                            foreach (var historia in lstHistoria)
                                            {
                                                //if (cuota.FechaEmision <= historia.Fecha)
                                                if (fechaComparar >= historia.Propietario.FechaCreacion.Value.Date &&
                                        (fechaComparar < objTitular2.FechaCreacion.Value.Date
                                        || (fechaComparar.Month == historia.Propietario.FechaCreacion.Value.Month
                                        && fechaComparar.Year == historia.Propietario.FechaCreacion.Value.Year)
                                        ))
                                                {
                                                    excelWorksheet.Cells["B" + i].Value = historia.Propietario.Nombres;
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    }
                                    TotalGeneral += Total;
                                    excelWorksheet.Cells[i, Col].Value = Total;
                                    excelWorksheet.Cells[i, Col].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                                    excelWorksheet.Cells[i, Col].Style.Font.Bold = true;
                                    i++;
                                    LstDepartamentoId.Add(item.DepartamentoId);
                                }

                            }
                            excelWorksheet.Cells[i, Col].Value = TotalGeneral;
                            excelWorksheet.Cells[i, Col].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                            excelWorksheet.Cells[i, Col].Style.Font.Bold = true;
                            //excelWorksheet.Cells["A" + i].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                            excelWorksheet.Cells["B" + i].Value = "Totales:";
                            excelWorksheet.Cells["B" + i].Style.Font.Bold = true;
                            excelWorksheet.Cells["B" + i].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                            excelWorksheet.Cells["C" + i].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                            foreach (var mes in LstMeses)
                            {
                                excelWorksheet.Cells[i, mes.Value].Value = LstCuotas.Where(x => x.UnidadTiempo.Orden == mes.Key).Sum(x => x.CuotaExtraordinaria);
                                excelWorksheet.Cells[i, mes.Value].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                                excelWorksheet.Cells[i, mes.Value].Style.Font.Bold = true;
                            }

                        }

                    }
                    var fileStreamResult = new FileContentResult(excelPackage.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

                    MemoryStream stream = new MemoryStream();
                    var aux = fileStreamResult.FileContents;
                    stream.Write(aux, 0, aux.Length);

                    Spire.Xls.Workbook workbook = new Spire.Xls.Workbook();
                    
                    workbook.LoadFromStream(stream);                    
                    var sheet = workbook.Worksheets[0];
                    sheet.PageSetup.Orientation = Spire.Xls.PageOrientationType.Landscape;
                    sheet.PageSetup.IsFitToPage = true;

                    MemoryStream streamPdf = new MemoryStream();
                    workbook.SaveToStream(streamPdf, Spire.Xls.FileFormat.PDF);

                    MemoryStream outputMemStream = new MemoryStream();
                    ZipOutputStream zipStream = new ZipOutputStream(outputMemStream);
                    zipStream.SetLevel(2);

                    var nombre = "Cuadro Moroso " + Edificio + ".pdf";
                    ZipEntry entry_pdf = new ZipEntry(nombre);
                    entry_pdf.DateTime = DateTime.Now;
                    zipStream.PutNextEntry(entry_pdf);
                    StreamUtils.Copy(new MemoryStream(streamPdf.ToArray()), zipStream, new byte[4096]);
                    zipStream.CloseEntry();

                    nombre = "Cuadro Moroso " + Edificio + ".xlsx";
                    ZipEntry entry_excel = new ZipEntry(nombre);
                    entry_excel.DateTime = DateTime.Now;
                    zipStream.PutNextEntry(entry_excel);
                    StreamUtils.Copy(new MemoryStream(fileStreamResult.FileContents), zipStream, new byte[4096]);
                    zipStream.CloseEntry();

                    zipStream.IsStreamOwner = false;
                    zipStream.Close();
                    outputMemStream.Position = 0;

                    return File(outputMemStream, "application/octet-stream", "Cuadro Moroso " + Edificio + ".zip");
                }
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error, ex.Message + " " + (ex.InnerException != null ? ex.InnerException.Message : ""));
                return RedirectToAction("CuadroMoroso", new { EdificioId = EdificioId });
            }
        }

        [ViewParameter("Edificio", "fa fa-money")]
        public ActionResult DeleteCuotaExtraordinaria(Int32 CuotaId)
        {
            Int32 UnidadTiempoId = 0;
            Int32 EdificioId = 0;

            try
            {
                var cuota = context.Cuota.FirstOrDefault(x => x.CuotaId == CuotaId);
                UnidadTiempoId = cuota.UnidadTiempoId;
                EdificioId = cuota.Departamento.EdificioId;
                context.Cuota.Remove(cuota);
                context.SaveChanges();

                PostMessage(MessageType.Success);
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error, ex.Message + " " + (ex.InnerException != null ? ex.InnerException.Message : ""));
            }
            return RedirectToAction("LstCuotaExtraordinaria", "Fee", new { EdificioId = EdificioId, UnidadTiempoId = UnidadTiempoId });
        }
        [ViewParameter("Edificio", "fa fa-money")]
        public ActionResult LstCuotaExtraordinaria(Int32 EdificioId, Int32 UnidadTiempoId)
        {
            LstCuotaExtraordinariaViewModel ViewModel = new LstCuotaExtraordinariaViewModel();
            ViewModel.Fill(CargarDatosContext(), EdificioId, UnidadTiempoId);
            return View(ViewModel);
        }
        [ViewParameter("Edificio", "fa fa-money")]
        public ActionResult LstConsumoIndividual(Int32 EdificioId, Int32 UnidadTiempoId)
        {
            LstConsumoIndividualViewModel ViewModel = new LstConsumoIndividualViewModel();
            ViewModel.Fill(CargarDatosContext(), EdificioId, UnidadTiempoId);
            return View(ViewModel);
        }
        [ViewParameter("Edificio", "fa fa-money")]
        public ActionResult AddEditCuotaExtraordinaria(Int32? CuotaId, Int32 EdificioId, Int32 UnidadTiempoId)
        {
            AddEditCuotaExtraordinariaViewModel ViewModel = new AddEditCuotaExtraordinariaViewModel();
            ViewModel.Fill(CargarDatosContext(), CuotaId, EdificioId, UnidadTiempoId);
            return View(ViewModel);
        }
        [ViewParameter("Edificio", "fa fa-money")]
        public ActionResult _AddEditMConsumoIndividual(Int32? CuotaId, Int32 EdificioId, Int32 UnidadTiempoId)
        {
            _AddEditMConsumoIndividual ViewModel = new _AddEditMConsumoIndividual();
            ViewModel.Fill(CargarDatosContext(), CuotaId, EdificioId, UnidadTiempoId);
            return View(ViewModel);
        }
        [HttpPost]
        public ActionResult _AddEditMConsumoIndividual(_AddEditMConsumoIndividual model)
        {
            try
            {
                var cuota = context.Cuota.FirstOrDefault(x => x.UnidadTiempoId == model.UnidadTiempoId
                && x.DepartamentoId == model.DepartamentoId);
                if (cuota != null)
                {
                    for (int i = 0; i < model.lstdetalle.Count; i++)
                    {
                        if (!String.IsNullOrEmpty(model.lstdetalle[i]))
                        {
                            ConsumoIndividual Consumo = null;

                            Consumo = new ConsumoIndividual();
                            Consumo.CuotaId = cuota.CuotaId;
                            Consumo.Estado = ConstantHelpers.EstadoActivo;
                            context.ConsumoIndividual.Add(Consumo);


                            Consumo.Monto = model.lstmonto[i];
                            Consumo.Detalle = model.lstdetalle[i];

                            context.SaveChanges();
                        }
                    }

                    var lstOtros = context.ConsumoIndividual.Where(x => x.CuotaId == cuota.CuotaId).Sum(x => x.Monto);
                    cuota.Total = cuota.Monto + cuota.ConsumoAguaTotal + (lstOtros);
                    cuota.Otros = lstOtros;
                    //cuota.Total += lstOtros;

                    context.SaveChanges();

                    PostMessage(MessageType.Success);
                }
                else
                {
                    PostMessage(MessageType.Warning, "NO EXISTEN CUOTAS CREADAS PARA ESTA UNIDAD DE TIEMPO.");
                }
            }
            catch { PostMessage(MessageType.Error); }
            return RedirectToAction("LstConsumoIndividual", new { EdificioId = model.EdificioId, UnidadTiempoId = model.UnidadTiempoId });
        }
        [ViewParameter("Edificio", "fa fa-money")]
        public ActionResult _AddEditConsumoIndividual(Int32? CuotaId, Int32 ConsumoIndividualId, Int32 EdificioId, Int32 UnidadTiempoId)
        {
            _AddEditConsumoIndividualViewModel ViewModel = new _AddEditConsumoIndividualViewModel();
            ViewModel.Fill(CargarDatosContext(), CuotaId, ConsumoIndividualId, EdificioId, UnidadTiempoId);
            return View(ViewModel);
        }
        [HttpPost]
        public ActionResult _AddEditConsumoIndividual(_AddEditConsumoIndividualViewModel ViewModel)
        {
            try
            {
                ConsumoIndividual Consumo = null;
                //if (ViewModel.ConsumoIndividualId)
                //{
                    Consumo = context.ConsumoIndividual.FirstOrDefault(x => x.ConsumoIndividualId == ViewModel.ConsumoIndividualId);
                //}
                //else
                //{
                //    Consumo = new ConsumoIndividual();
                //    Consumo.CuotaId = ViewModel.CuotaId.Value;
                //    Consumo.Estado = ConstantHelpers.EstadoActivo;
                //    context.ConsumoIndividual.Add(Consumo);
                //}

                Consumo.Monto = ViewModel.Monto;
                Consumo.Detalle = ViewModel.Detalle;

                context.SaveChanges();

                var lstOtros = context.ConsumoIndividual.Where(x => x.CuotaId == Consumo.CuotaId).Sum(x => x.Monto);
                var cuota = context.Cuota.FirstOrDefault(x => x.CuotaId == Consumo.CuotaId);
                cuota.Total = cuota.Monto + cuota.ConsumoAguaTotal + (lstOtros);
                cuota.Otros = lstOtros;
                //cuota.Total += lstOtros;

                context.SaveChanges();

                PostMessage(MessageType.Success);
            }
            catch { PostMessage(MessageType.Error); }
            return RedirectToAction("LstConsumoIndividual", new { EdificioId = ViewModel.EdificioId, UnidadTiempoId = ViewModel.UnidadTiempoId });
        }
        public ActionResult DeleteConsumoIndividual(Int32 ConsumoIndividualId, Int32 EdificioId, Int32 UnidadTiempoId)
        {
            try
            {
                var Consumo = context.ConsumoIndividual.FirstOrDefault(x => x.ConsumoIndividualId == ConsumoIndividualId);
                Consumo.Estado = ConstantHelpers.EstadoInactivo;

                context.SaveChanges();

                var lstOtros = context.ConsumoIndividual.Where(x => x.CuotaId == Consumo.CuotaId).Sum(x => x.Monto);
                var cuota = context.Cuota.FirstOrDefault(x => x.CuotaId == Consumo.CuotaId);
                cuota.Total = cuota.Total - (cuota.Otros ?? 0);
                cuota.Otros = lstOtros;
                cuota.Total += lstOtros;

                context.SaveChanges();

                PostMessage(MessageType.Success);
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error);
            }
            return RedirectToAction("LstConsumoIndividual", new { EdificioId = EdificioId, UnidadTiempoId = UnidadTiempoId });
        }
        [HttpPost]
        public ActionResult AddEditCuotaExtraordinaria(AddEditCuotaExtraordinariaViewModel ViewModel)
        {
            try
            {
                Cuota Cuota = null;
                if (ViewModel.CuotaId.HasValue)
                {
                    Cuota = context.Cuota.FirstOrDefault(x => x.CuotaId == ViewModel.CuotaId);
                }
                else
                {
                    Cuota = new Cuota();
                    Cuota.LecturaAgua = 0;
                    Cuota.ConsumoAgua = 0;
                    Cuota.ConsumoMes = 0;
                    Cuota.Monto = 0;
                    Cuota.Total = ViewModel.MontoExtraordinaria.ToDecimal();
                    Cuota.UnidadTiempoId = ViewModel.UnidadTiempoId;
                    Cuota.Estado = ConstantHelpers.EstadoPendiente;
                    Cuota.ConsumoSoles = 0;
                    Cuota.AreaComun = 0;
                    Cuota.CuotaExtraordinaria = ViewModel.MontoExtraordinaria.ToDecimal();
                    Cuota.Alcantarillado = 0;
                    Cuota.CargoFijo = 0;
                    Cuota.IGV = 0;
                    Cuota.Pagado = false;
                    Cuota.ConsumoAguaTotal = 0;
                    Cuota.FechaRegistro = DateTime.Now;
                    Cuota.FechaEmision = DateTime.Now;
                    Cuota.EsExtraordinaria = true;
                    context.Cuota.Add(Cuota);
                }

                Cuota.DepartamentoId = ViewModel.DepartamentoId;

                if (ViewModel.FechaPago == null)
                    Cuota.FechaPagado = null;
                else
                    Cuota.FechaPagado = ViewModel.FechaPago.ToDateTime();

                Cuota.Mora = ViewModel.Mora ?? 0;
                Cuota.Leyenda = ViewModel.Leyenda;
                Cuota.Pagado = ViewModel.Estado == "0" ? false : true;
                Cuota.CuotaExtraordinaria = ViewModel.MontoExtraordinaria;
                Cuota.Total = ViewModel.MontoExtraordinaria ?? 0;
                context.SaveChanges();
                PostMessage(MessageType.Success);

                //context.SaveChanges();
                //PostMessage(MessageType.Success);
            }
            catch { PostMessage(MessageType.Error); }
            return RedirectToAction("LstCuotaExtraordinaria", new { EdificioId = ViewModel.EdificioId, UnidadTiempoId = ViewModel.UnidadTiempoId });
        }
        [ViewParameter("Edificio", "fa fa-money")]
        public ActionResult _AddEditMCuotaExtraordinaria(Int32 EdificioId, Int32 UnidadTiempoId)
        {
            _AddEditCuotaExtraordinariaViewModel ViewModel = new _AddEditCuotaExtraordinariaViewModel();
            ViewModel.Fill(CargarDatosContext(), EdificioId, UnidadTiempoId);
            return View(ViewModel);
        }
        [HttpPost]
        public ActionResult _AddEditMCuotaExtraordinaria(_AddEditCuotaExtraordinariaViewModel ViewModel)
        {
            try
            {
                var lstDepartamentoId = context.Departamento.Where(x => x.EdificioId == ViewModel.EdificioId && x.Estado == ConstantHelpers.EstadoActivo)
                    .Select(x => x.DepartamentoId).ToList();

                var lstCuotas = context.Cuota.Count(x => x.UnidadTiempoId == ViewModel.UnidadTiempoId && x.EsExtraordinaria == true && x.Departamento.EdificioId == ViewModel.EdificioId);

                if (lstCuotas == 0)
                {
                    foreach (var DepartamentoId in lstDepartamentoId)
                    {
                        Cuota cuota = new Cuota();
                        cuota.LecturaAgua = 0;
                        cuota.ConsumoAgua = 0;
                        cuota.ConsumoMes = 0;
                        cuota.Monto = 0;
                        cuota.DepartamentoId = DepartamentoId;
                        cuota.Total = ViewModel.MontoExtraordinaria.ToDecimal();
                        cuota.Mora = 0;
                        cuota.UnidadTiempoId = ViewModel.UnidadTiempoId;
                        cuota.Estado = ConstantHelpers.EstadoPendiente;
                        cuota.ConsumoSoles = 0;
                        cuota.AreaComun = 0;
                        cuota.Pagado = false;
                        cuota.Alcantarillado = 0;
                        cuota.CuotaExtraordinaria = ViewModel.MontoExtraordinaria.ToDecimal();
                        cuota.CargoFijo = 0;
                        cuota.IGV = 0;
                        cuota.ConsumoAguaTotal = 0;
                        cuota.FechaRegistro = DateTime.Now;
                        cuota.FechaEmision = DateTime.Now;
                        cuota.EsExtraordinaria = true;
                        context.Cuota.Add(cuota);
                    }
                }

                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch { PostMessage(MessageType.Error); }
            return RedirectToAction("EditMasivoCuota", new { EdificioId = ViewModel.EdificioId, UnidadTiempoId = ViewModel.UnidadTiempoId });
        }
        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-money")]
        public ActionResult AddEditCuota(Int32? CuotaId, Int32 DepartamentoId, Int32 EdificioId)
        {
            AddEditCuotaViewModel ViewModel = new AddEditCuotaViewModel();
            ViewModel.DepartamentoId = DepartamentoId;
            ViewModel.EdificioId = EdificioId;
            ViewModel.Fill(CargarDatosContext());
            ViewModel.FillComboUnidadTiempo(CargarDatosContext());
            return View(ViewModel);
        }

        [HttpPost]
        public ActionResult AddEditCuota(AddEditCuotaViewModel ViewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewModel.Fill(CargarDatosContext());
                ViewModel.FillComboUnidadTiempo(CargarDatosContext());
                TryUpdateModel(ViewModel);
                return View(ViewModel);
            }
            try
            {
                if (ViewModel.CuotaId.HasValue)
                {
                    Cuota cuota = context.Cuota.FirstOrDefault(x => x.CuotaId == ViewModel.CuotaId.Value);
                    cuota.LecturaAgua = ViewModel.LecturaAgua.ToDecimal();
                    cuota.ConsumoAgua = ViewModel.ConsumoAgua.ToDecimal();
                    cuota.ConsumoMes = ViewModel.ConsumoMes.ToDecimal();
                    cuota.Monto = ViewModel.Monto.ToDecimal();
                    cuota.Total = ViewModel.Total.ToDecimal();
                    cuota.Mora = ViewModel.Mora;
                    cuota.UnidadTiempoId = ViewModel.UnidadTiempoId;
                    cuota.ConsumoSoles = ViewModel.ConsumoSoles.ToDecimal();
                    cuota.AreaComun = ViewModel.AreaComun.ToDecimal();
                    cuota.Alcantarillado = ViewModel.Alcantarillado.ToDecimal();
                    cuota.CargoFijo = ViewModel.CargoFijo.ToDecimal();
                    cuota.IGV = ViewModel.IGV.ToDecimal();
                    cuota.ConsumoAguaTotal = cuota.ConsumoAguaTotal;
                    context.Entry(cuota).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    Cuota cuota = new Cuota();
                    cuota.LecturaAgua = ViewModel.LecturaAgua.ToDecimal();
                    cuota.ConsumoAgua = ViewModel.ConsumoAgua.ToDecimal();
                    cuota.ConsumoMes = ViewModel.ConsumoMes.ToDecimal();
                    cuota.Monto = ViewModel.Monto.ToDecimal();
                    cuota.DepartamentoId = ViewModel.DepartamentoId;
                    cuota.Total = ViewModel.Total.ToDecimal();
                    cuota.Mora = ViewModel.Mora;
                    cuota.UnidadTiempoId = ViewModel.UnidadTiempoId;
                    cuota.Estado = ConstantHelpers.EstadoPendiente;
                    cuota.ConsumoSoles = ViewModel.ConsumoSoles.ToDecimal();
                    cuota.AreaComun = ViewModel.AreaComun.ToDecimal();
                    cuota.Alcantarillado = ViewModel.Alcantarillado.ToDecimal();
                    cuota.CargoFijo = ViewModel.CargoFijo.ToDecimal();
                    cuota.IGV = ViewModel.IGV.ToDecimal();
                    cuota.ConsumoAguaTotal = cuota.ConsumoAguaTotal;
                    cuota.FechaRegistro = DateTime.Now;
                    cuota.FechaEmision = DateTime.Now;
                    cuota.EsExtraordinaria = false;
                    context.Cuota.Add(cuota);
                }
                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch { PostMessage(MessageType.Error); }
            return RedirectToAction("LstCuota", new { DepartamentoId = ViewModel.DepartamentoId, EdificioId = ViewModel.EdificioId });
        }

        [ViewParameter(PageIcon: "fa fa-eye-slash")]
        public ActionResult _ActDeactCuota(Int32 CuotaId, Int32 DepartamentoId, Int32 EdificioId, String Estado)
        {
            ViewBag.CuotaId = CuotaId;
            ViewBag.DepartamentoId = DepartamentoId;
            ViewBag.EdificioId = EdificioId;
            ViewBag.Estado = Estado;
            return PartialView();
        }

        [HttpPost]
        public ActionResult ActDeactCuota(Int32 CuotaId, Int32 DepartamentoId, Int32 EdificioId)
        {
            try
            {
                Cuota Cuota = context.Cuota.FirstOrDefault(x => x.CuotaId == CuotaId);
                if (Cuota != null)
                {
                    Cuota.Estado = Cuota.Estado == ConstantHelpers.EstadoInactivo ? ConstantHelpers.EstadoPendiente : ConstantHelpers.EstadoInactivo;
                    context.Entry(Cuota).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                    PostMessage(MessageType.Success);
                }
            }
            catch { PostMessage(MessageType.Error); }
            return RedirectToAction("LstCuota", new { DepartamentoId = DepartamentoId, EdificioId = EdificioId });
        }

        [ViewParameter("Moroso", "fa fa-frown-o")]
        [AppAuthorize(AppRol.Propietario)]
        public ActionResult LstMoroso(Int32? UnidadTiempoId)
        {
            LstMorosoViewModel ViewModel = new LstMorosoViewModel();
            ViewModel.EdificioId = SessionHelpers.GetEdificioId(Session);
            ViewModel.UnidadTiempoId = UnidadTiempoId;
            ViewModel.Fill(CargarDatosContext());
            return View(ViewModel);
        }

        [ViewParameter("Moroso", "fa fa-frown-o")]
        [AppAuthorize(AppRol.Propietario, AppRol.Administrador)]
        public ActionResult CuadroMoroso(Int32 EdificioId)
        {
            CuadroMorosoViewModel ViewModel = new CuadroMorosoViewModel();
            try
            {
                ViewModel.Fill(CargarDatosContext(), EdificioId);
                return View(ViewModel);
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error, ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty) + ex.StackTrace);
                //throw ex;
                return View(ViewModel);
            }

        }

        [ViewParameter("EstadoCuenta", "fa fa-bar-chart-o")]
        [AppAuthorize(AppRol.Propietario)]
        public ActionResult LstDetailEstadoCuenta(Int32? np, Int32? Anio, Int32? Mes)
        {
            DetailEstadoCuentaViewModel ViewModel = new DetailEstadoCuentaViewModel();
            ViewModel.Anio = Anio;
            ViewModel.Mes = Mes;
            ViewModel.EdificioId = SessionHelpers.GetEdificioId(Session);
            ViewModel.DepartamentoId = SessionHelpers.GetDepartamentoId(Session);
            ViewModel.Fill(CargarDatosContext(), np, this);
            return View(ViewModel);
        }

        public ActionResult DescargarRecibo(Int32 EdificioId, Int32 DepartamentoId)
        {
            Document document = new Document(PageSize.A4, 5, 5, 5, 5);
            try
            {
                var Edificio = context.Edificio.Find(EdificioId);
                var Departamento = context.Departamento.FirstOrDefault(x => x.DepartamentoId == DepartamentoId);
                var propietario = context.Propietario.FirstOrDefault(x => x.DepartamentoId == DepartamentoId);
                var UnidadTiempo = context.UnidadTiempo.FirstOrDefault(x => x.EsActivo && x.Estado == ConstantHelpers.EstadoActivo);
                var Cuota = context.Cuota.FirstOrDefault(x => x.DepartamentoId == DepartamentoId && x.UnidadTiempoId == UnidadTiempo.UnidadTiempoId);// && x.Estado == ConstantHelpers.EstadoPendiente);
                var CuotaAnterior = context.Cuota.Where(x => x.DepartamentoId == DepartamentoId && x.FechaRegistro < Cuota.FechaRegistro && x.Estado == ConstantHelpers.EstadoPendiente && x.UnidadTiempo.Estado == ConstantHelpers.EstadoActivo).OrderByDescending(x => x.FechaRegistro).Take(1);
                var DeudasPendientes = context.Cuota.Where(x => x.Estado == ConstantHelpers.EstadoPendiente && x.CuotaId != Cuota.CuotaId && x.UnidadTiempo.Estado == ConstantHelpers.EstadoActivo).Take(6);

                var memoryStream = new MemoryStream();
                var pdfWriter = PdfWriter.GetInstance(document, memoryStream);
                document.AddTitle("Edificio " + Edificio.Nombre + " | Departamento" + Departamento.Numero);
                document.AddSubject("Recibo - " + DateTime.Now.ToShortDateString());
                document.AddCreator("Administrador");
                document.AddHeader("Expires", "0");
                document.Open();

                Font font = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20);
                Font fontC = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 15);
                Font fNormal = FontFactory.GetFont(FontFactory.HELVETICA, 11);
                Font fLess = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                Font fBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);
                BaseColor BColorCabeceraTabla = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#A9D08E"));
                #region cabecera

                Image tif = Image.GetInstance(Server.MapPath("~/Content/img/VEH_logo.png"));
                tif.ScalePercent(16f);
                tif.SetAbsolutePosition(5f, document.PageSize.Height - 85f);

                Paragraph pTitulo = new Paragraph("JUNTA DE PROPIETARIOS DEL EDIFICIO " + Edificio.Nombre, font);
                pTitulo.Alignment = Element.ALIGN_CENTER;

                PdfPTable tCabecera = new PdfPTable(new float[] { 85f, 285f, 105f });
                tCabecera.WidthPercentage = 100;
                PdfPCell cLogo = new PdfPCell(tif);
                cLogo.Border = 0;
                PdfPCell cTitulo = new PdfPCell(pTitulo);
                cTitulo.HorizontalAlignment = Element.ALIGN_CENTER;
                cTitulo.Border = 0;
                cTitulo.VerticalAlignment = Element.ALIGN_MIDDLE;
                PdfPCell cAcronimo = new PdfPCell(new Phrase(Edificio.Nombre, fontC));
                cAcronimo.Border = 0;
                cAcronimo.HorizontalAlignment = Element.ALIGN_CENTER;
                cAcronimo.VerticalAlignment = Element.ALIGN_MIDDLE;
                tCabecera.AddCell(cLogo);
                tCabecera.AddCell(cTitulo);
                tCabecera.AddCell(cAcronimo);
                document.Add(tCabecera);

                #endregion

                #region CodDeposito - ReciboMant

                PdfPTable tDepositoRecibo = new PdfPTable(3);
                tDepositoRecibo.WidthPercentage = 100;
                tDepositoRecibo.SpacingAfter = 10;
                PdfPCell cCodigoDeposito = new PdfPCell(new Phrase("CÓDIGO DE DEPÓSITO", fBold));
                cCodigoDeposito.HorizontalAlignment = Element.ALIGN_CENTER;
                cCodigoDeposito.VerticalAlignment = Element.ALIGN_MIDDLE;
                cCodigoDeposito.BackgroundColor = BColorCabeceraTabla;
                tDepositoRecibo.AddCell(cCodigoDeposito);

                PdfPCell cBlancoSinBorde = new PdfPCell(new Phrase(" "));
                cBlancoSinBorde.Border = 0;
                tDepositoRecibo.AddCell(cBlancoSinBorde);

                PdfPCell cReciboMant = new PdfPCell(new Phrase("RECIBO DE MANTENIMIENTO", fBold));
                cReciboMant.HorizontalAlignment = Element.ALIGN_CENTER;
                cReciboMant.VerticalAlignment = Element.ALIGN_MIDDLE;
                cReciboMant.BackgroundColor = BColorCabeceraTabla;
                tDepositoRecibo.AddCell(cReciboMant);

                var cPrimero = new PdfPCell(new Phrase("1232.22"/*string.Empty*/, fNormal));
                cPrimero.VerticalAlignment = Element.ALIGN_MIDDLE;
                cPrimero.HorizontalAlignment = Element.ALIGN_CENTER;
                tDepositoRecibo.AddCell(cPrimero);// codigo deposito
                tDepositoRecibo.AddCell(cBlancoSinBorde);
                cPrimero = new PdfPCell(new Phrase("1232.22"/*string.Empty*/, fNormal));
                cPrimero.VerticalAlignment = Element.ALIGN_MIDDLE;
                cPrimero.HorizontalAlignment = Element.ALIGN_CENTER;
                tDepositoRecibo.AddCell(cPrimero); // recibo mantenimiento 

                document.Add(tDepositoRecibo);

                #endregion

                #region detalle  - estado de cuenta

                PdfPTable tDetalleEstCta = new PdfPTable(new float[] { 100f, 60f, 43f, 43f, 43f, 43f, 45f, 45f });
                tDetalleEstCta.WidthPercentage = 100;
                PdfPCell cDetalle = new PdfPCell(new Phrase("DETALLE", fBold));
                cDetalle.HorizontalAlignment = Element.ALIGN_CENTER;
                cDetalle.VerticalAlignment = Element.ALIGN_MIDDLE;
                cDetalle.BackgroundColor = BColorCabeceraTabla;
                cDetalle.Colspan = 2;

                PdfPCell cEstadoCuenta = new PdfPCell(new Phrase("ESTADO DE CUENTA", fBold));
                cEstadoCuenta.Colspan = 6;
                cEstadoCuenta.HorizontalAlignment = Element.ALIGN_CENTER;
                cEstadoCuenta.VerticalAlignment = Element.ALIGN_MIDDLE;
                cEstadoCuenta.BackgroundColor = BColorCabeceraTabla;

                tDetalleEstCta.AddCell(cDetalle);
                tDetalleEstCta.AddCell(cEstadoCuenta);

                PdfPCell cDetalleDes = new PdfPCell(new Phrase("Cuota de mantenimiento " + UnidadTiempo.Descripcion, fLess));
                cDetalleDes.HorizontalAlignment = Element.ALIGN_CENTER;
                cDetalleDes.VerticalAlignment = Element.ALIGN_TOP; cDetalleDes.BorderWidthRight = 0;
                cDetalleDes.Rowspan = 6;

                PdfPCell cDetalleVal = new PdfPCell(new Phrase("222.34"/*Cuota.Total.ToString()*/, fLess));
                cDetalleVal.HorizontalAlignment = Element.ALIGN_CENTER;
                cDetalleVal.VerticalAlignment = Element.ALIGN_TOP; cDetalleVal.BorderWidthLeft = 0;
                cDetalleVal.Rowspan = 6;

                PdfPCell cConsumosIndividuales = new PdfPCell(new Phrase("CONSUMOS INDIVIDUALES", fBold));
                cConsumosIndividuales.BackgroundColor = BColorCabeceraTabla;
                cConsumosIndividuales.HorizontalAlignment = Element.ALIGN_CENTER; cConsumosIndividuales.VerticalAlignment = Element.ALIGN_MIDDLE;
                cConsumosIndividuales.Colspan = 6;
                tDetalleEstCta.AddCell(cDetalleDes);
                tDetalleEstCta.AddCell(cDetalleVal);
                tDetalleEstCta.AddCell(cConsumosIndividuales);

                PdfPCell cEstCtaDetalle = new PdfPCell(new Phrase("Concepto", fNormal));
                cEstCtaDetalle.VerticalAlignment = Element.ALIGN_MIDDLE; cEstCtaDetalle.HorizontalAlignment = Element.ALIGN_CENTER;
                cEstCtaDetalle.BackgroundColor = BColorCabeceraTabla;

                tDetalleEstCta.AddCell(cEstCtaDetalle);

                cEstCtaDetalle = new PdfPCell(new Phrase("Lect. Ant.", fNormal));
                cEstCtaDetalle.VerticalAlignment = Element.ALIGN_MIDDLE; cEstCtaDetalle.HorizontalAlignment = Element.ALIGN_CENTER;
                cEstCtaDetalle.BackgroundColor = BColorCabeceraTabla;

                tDetalleEstCta.AddCell(cEstCtaDetalle);

                cEstCtaDetalle = new PdfPCell(new Phrase("Lect. Act.", fNormal));
                cEstCtaDetalle.VerticalAlignment = Element.ALIGN_MIDDLE; cEstCtaDetalle.HorizontalAlignment = Element.ALIGN_CENTER;
                cEstCtaDetalle.BackgroundColor = BColorCabeceraTabla;

                tDetalleEstCta.AddCell(cEstCtaDetalle);

                cEstCtaDetalle = new PdfPCell(new Phrase("Cons. Mes.", fNormal));
                cEstCtaDetalle.VerticalAlignment = Element.ALIGN_MIDDLE; cEstCtaDetalle.HorizontalAlignment = Element.ALIGN_CENTER;
                cEstCtaDetalle.BackgroundColor = BColorCabeceraTabla;

                tDetalleEstCta.AddCell(cEstCtaDetalle);

                cEstCtaDetalle = new PdfPCell(new Phrase("S./ Total", fNormal));
                cEstCtaDetalle.Colspan = 2;
                cEstCtaDetalle.VerticalAlignment = Element.ALIGN_MIDDLE; cEstCtaDetalle.HorizontalAlignment = Element.ALIGN_CENTER;
                cEstCtaDetalle.BackgroundColor = BColorCabeceraTabla;

                tDetalleEstCta.AddCell(cEstCtaDetalle);
                PdfPCell CBlancoConBorde = new PdfPCell();

                PdfPCell cBlancoConBorde2 = new PdfPCell();
                CBlancoConBorde.Colspan = 2;




                for (int i = 0; i < 4; i++)
                {
                    PdfPCell cCeldaValorConcepto = new PdfPCell();
                    cCeldaValorConcepto.HorizontalAlignment = Element.ALIGN_CENTER;
                    cCeldaValorConcepto.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cCeldaValorConcepto.BorderWidthBottom = 1;
                    cCeldaValorConcepto.BorderWidthLeft = 1;

                    PdfPCell cCeldaValorNro = new PdfPCell();
                    cCeldaValorNro.HorizontalAlignment = Element.ALIGN_CENTER;
                    cCeldaValorNro.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cCeldaValorNro.BorderWidthBottom = 1;
                    cCeldaValorNro.BorderWidthLeft = 1;

                    PdfPCell cCeldaValorM2 = new PdfPCell();
                    cCeldaValorNro.HorizontalAlignment = Element.ALIGN_CENTER;
                    cCeldaValorNro.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cCeldaValorNro.BorderWidthBottom = 1;
                    cCeldaValorNro.BorderWidthLeft = 1;
                }

                //tDetalleEstCta.AddCell();
                tDetalleEstCta.AddCell(CBlancoConBorde);
                tDetalleEstCta.AddCell(CBlancoConBorde);
                tDetalleEstCta.AddCell(CBlancoConBorde);
                tDetalleEstCta.AddCell(CBlancoConBorde);


                document.Add(tDetalleEstCta);

                #endregion


                pdfWriter.CloseStream = false;
                document.Close();
                memoryStream.Position = 0;
                return File(memoryStream, "application/pdf");
            }
            catch { }
            return RedirectToAction("LstCuota", new { DepartamentoId = DepartamentoId, EdificioId = EdificioId });
        }
        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-list")]
        [HttpGet]
        public ActionResult AddEditLeyenda(Int32? LeyendaId, Int32 EdificioId, Int32? DepartamentoId, Int32 UnidadTiempoId)
        {
            AddEditLeyendaViewModel model = new AddEditLeyendaViewModel();
            model.Fill(CargarDatosContext(), LeyendaId);
            model.EdificioId = EdificioId;
            model.DepartamentoId = DepartamentoId;
            model.UnidadTiempoId = UnidadTiempoId;
            return View(model);
        }
        public ActionResult _AddEditLeyenda(Int32? LeyendaId, Int32 EdificioId, Int32? DepartamentoId, Int32 UnidadTiempoId)
        {
            AddEditLeyendaViewModel model = new AddEditLeyendaViewModel();
            model.Fill(CargarDatosContext(), LeyendaId);
            model.EdificioId = EdificioId;
            model.DepartamentoId = DepartamentoId;
            model.UnidadTiempoId = UnidadTiempoId;
            return View(model);
        }
        [HttpPost]
        public JsonResult _AddEditLeyenda(AddEditLeyendaViewModel model)
        {
            Leyenda ley = null;
            BalanceUnidadTiempoEdificio BalanceUnidadTiempoEdificio = null;
            if (model.LeyendaId.HasValue)
            {
                ley = context.Leyenda.FirstOrDefault(x => x.LeyendaId == model.LeyendaId.Value);

            }
            else
            {

                BalanceUnidadTiempoEdificio = context.BalanceUnidadTiempoEdificio.FirstOrDefault(X => X.EdificioId == model.EdificioId && X.UnidadDeTiempoId == model.UnidadTiempoId);
                if (BalanceUnidadTiempoEdificio == null)
                {
                    BalanceUnidadTiempoEdificio = new BalanceUnidadTiempoEdificio();
                    BalanceUnidadTiempoEdificio.EdificioId = model.EdificioId;
                    BalanceUnidadTiempoEdificio.FechaDeActualizacion = DateTime.Now;
                    BalanceUnidadTiempoEdificio.GastosTotalesMes = 0;
                    BalanceUnidadTiempoEdificio.IngresosTotalesMes = 0;
                    BalanceUnidadTiempoEdificio.SaldoAcumulado = 0;
                    BalanceUnidadTiempoEdificio.SaldoMes = 0;
                    BalanceUnidadTiempoEdificio.UnidadDeTiempoId = model.UnidadTiempoId;
                    context.BalanceUnidadTiempoEdificio.Add(BalanceUnidadTiempoEdificio);
                    context.SaveChanges();
                }

                ley = new Leyenda();
                ley.BalanceUnidadTiempoEdificioId = context.BalanceUnidadTiempoEdificio.FirstOrDefault(X => X.UnidadDeTiempoId == model.UnidadTiempoId && X.EdificioId == model.EdificioId).BalanceUnidadTiempoEdificioId;
                context.Leyenda.Add(ley);
            }

            ley.Descripcion = model.Descripcion;
            ley.Numero = model.Numero;

            context.SaveChanges();

            return Json(true);
        }
        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-list")]
        [HttpGet]
        public ActionResult DeleteLeyenda(Int32? LeyendaId, Int32 BalanceEdificioUnidadTiempoId, Int32 DepartamentoId)
        {

            Leyenda ley = context.Leyenda.FirstOrDefault(X => X.LeyendaId == LeyendaId.Value);
            context.Leyenda.Remove(ley);
            context.SaveChanges();
            BalanceUnidadTiempoEdificio bal = context.BalanceUnidadTiempoEdificio.FirstOrDefault(X => X.BalanceUnidadTiempoEdificioId == BalanceEdificioUnidadTiempoId);
            return RedirectToAction("CerrarCuota", "Fee", new { EdificioId = bal.EdificioId, UnidadTiempoId = bal.UnidadDeTiempoId, DepartamentoId = DepartamentoId });
        }

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-list")]
        [HttpPost]
        public ActionResult AddEditLeyenda(AddEditLeyendaViewModel model)
        {
            try
            {
                Leyenda ley = null;
                BalanceUnidadTiempoEdificio BalanceUnidadTiempoEdificio = null;
                if (model.LeyendaId.HasValue)
                {
                    ley = context.Leyenda.FirstOrDefault(x => x.LeyendaId == model.LeyendaId.Value);

                }
                else
                {

                    BalanceUnidadTiempoEdificio = context.BalanceUnidadTiempoEdificio.FirstOrDefault(X => X.EdificioId == model.EdificioId && X.UnidadDeTiempoId == model.UnidadTiempoId);
                    if (BalanceUnidadTiempoEdificio == null)
                    {
                        BalanceUnidadTiempoEdificio = new BalanceUnidadTiempoEdificio();
                        BalanceUnidadTiempoEdificio.EdificioId = model.EdificioId;
                        BalanceUnidadTiempoEdificio.FechaDeActualizacion = DateTime.Now;
                        BalanceUnidadTiempoEdificio.GastosTotalesMes = 0;
                        BalanceUnidadTiempoEdificio.IngresosTotalesMes = 0;
                        BalanceUnidadTiempoEdificio.SaldoAcumulado = 0;
                        BalanceUnidadTiempoEdificio.SaldoMes = 0;
                        BalanceUnidadTiempoEdificio.UnidadDeTiempoId = model.UnidadTiempoId;
                        context.BalanceUnidadTiempoEdificio.Add(BalanceUnidadTiempoEdificio);
                        context.SaveChanges();
                    }

                    ley = new Leyenda();
                    ley.BalanceUnidadTiempoEdificioId = context.BalanceUnidadTiempoEdificio.FirstOrDefault(X => X.UnidadDeTiempoId == model.UnidadTiempoId && X.EdificioId == model.EdificioId).BalanceUnidadTiempoEdificioId;
                    context.Leyenda.Add(ley);
                }

                ley.Descripcion = model.Descripcion;
                ley.Numero = model.Numero;

                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch { PostMessage(MessageType.Error); }
            BalanceUnidadTiempoEdificio bal = context.BalanceUnidadTiempoEdificio.FirstOrDefault(X => X.EdificioId == model.EdificioId && X.UnidadDeTiempoId == model.UnidadTiempoId);
            return RedirectToAction("CerrarCuota", "Fee", new { EdificioId = bal.EdificioId, DepartamentoId = model.DepartamentoId, UnidadTiempoFin = model.UnidadTiempoId });

        }
        //PRUEBA
        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-money")]
        [HttpPost]
        public ActionResult ExportToFile(EditMasivoCuotaViewModel viewModel, FormCollection formCollection)
        {
            try
            {
                var departamentos = context.Departamento.Where(x => x.EdificioId == viewModel.EdificioId && x.Estado == ConstantHelpers.EstadoActivo);

                var cuotaComun = context.CuotaComun.FirstOrDefault(x => x.EdificioId == viewModel.EdificioId && x.UnidadTiempoId == viewModel.UnidadTiempoId);

                if (cuotaComun == null)
                {
                    cuotaComun = new CuotaComun();
                    cuotaComun.FechaRegistro = DateTime.Now;
                    cuotaComun.UnidadTiempoId = viewModel.UnidadTiempoId.Value;
                    cuotaComun.EdificioId = viewModel.EdificioId;
                    cuotaComun.Estado = ConstantHelpers.EstadoPendiente;
                    cuotaComun.SaldoMes = 0M;
                    context.CuotaComun.Add(cuotaComun);
                }

                cuotaComun.TotalAreaComun = formCollection["comun-area-comun"].ToDecimal();
                cuotaComun.TotalAlcantarillado = formCollection["comun-alcantarillado"].ToDecimal();
                cuotaComun.TotalCargoFijo = formCollection["comun-cargo-fijo"].ToDecimal();
                cuotaComun.TotalMontoAgua = formCollection["comun-consumo-soles"].ToDecimal();

                foreach (var departamento in departamentos)
                {
                    var cuotaLecturaActual = formCollection["cuota-lectura-actual-" + departamento.DepartamentoId];
                    var cuotaConsumoAgua = formCollection["cuota-consumo-agua-" + departamento.DepartamentoId];
                    var cuotaConsumoSoles = formCollection["cuota-consumo-soles-" + departamento.DepartamentoId];
                    var cuotaAreaComun = formCollection["cuota-area-comun-" + departamento.DepartamentoId];
                    var cuotaAlcantarillado = formCollection["cuota-alcantarillado-" + departamento.DepartamentoId];
                    var cuotaCargoFijo = formCollection["cuota-cargo-fijo-" + departamento.DepartamentoId];
                    var cuotaIgv = formCollection["cuota-igv-" + departamento.DepartamentoId];
                    var cuotaConsumoTotalAgua = formCollection["cuota-consumo-total-agua-" + departamento.DepartamentoId];
                    var cuotaCuota = formCollection["cuota-cuota-" + departamento.DepartamentoId];
                    var cuotaTotal = formCollection["cuota-total-" + departamento.DepartamentoId];

                    Cuota cuota = context.Cuota.FirstOrDefault(x => x.DepartamentoId == departamento.DepartamentoId && x.UnidadTiempoId == viewModel.UnidadTiempoId);
                    if (cuota == null)
                    {
                        cuota = new Cuota();
                        cuota.FechaRegistro = DateTime.Now;
                        cuota.FechaEmision = DateTime.Now;
                        cuota.Estado = ConstantHelpers.EstadoPendiente;
                        cuota.DepartamentoId = departamento.DepartamentoId;
                        cuota.UnidadTiempoId = viewModel.UnidadTiempoId.Value;
                        context.Cuota.Add(cuota);
                    }

                    cuota.LecturaAgua = cuotaLecturaActual.ToDecimal();
                    cuota.ConsumoAgua = cuotaConsumoAgua.ToDecimal();
                    cuota.ConsumoMes = cuotaConsumoAgua.ToDecimal();
                    cuota.Monto = cuotaCuota.ToDecimal();
                    cuota.Total = cuotaTotal.ToDecimal();
                    cuota.Mora = 0;
                    cuota.UnidadTiempoId = viewModel.UnidadTiempoId.Value;
                    cuota.ConsumoSoles = cuotaConsumoSoles.ToDecimal();
                    cuota.AreaComun = cuotaAreaComun.ToDecimal();
                    cuota.Alcantarillado = cuotaAlcantarillado.ToDecimal();
                    cuota.CargoFijo = cuotaCargoFijo.ToDecimal();
                    cuota.IGV = cuotaIgv.ToDecimal();
                    cuota.ConsumoAguaTotal = cuotaConsumoTotalAgua.ToDecimal();



                }
                context.SaveChanges();
                PostMessage(MessageType.Success);
            }

            catch { PostMessage(MessageType.Error); }
            return RedirectToAction("EditMasivoCuota", new { EdificioId = viewModel.EdificioId });

        }

        [AppAuthorize(AppRol.Propietario)]
        public ActionResult DownloadReporteDepartamento(Int32 departamentoId, Int32 unidadTiempoId)
        {
            try
            {

                //<Chequear si se ha subido correcion
                var departamento = context.Departamento.FirstOrDefault(X => X.DepartamentoId == departamentoId);
                var cantDepartamento = context.Departamento.Count(x => x.EdificioId == departamento.EdificioId);
                var unidadTiempo = context.UnidadTiempo.FirstOrDefault(X => X.UnidadTiempoId == unidadTiempoId);
                if (departamento != null && unidadTiempo != null)
                {
                    var correcion = context.ArchivoCorrecionEdificio.FirstOrDefault(X => X.Tipo.Contains(ConstantHelpers.TipoArchivo.Recibo + "/" + departamento.Numero) && X.EdificioId == departamento.EdificioId && X.UnidadTiempoId == unidadTiempoId);
                    if (correcion != null)
                    {
                        byte[] fileBytes = System.IO.File.ReadAllBytes(Path.Combine(Server.MapPath("~/Resources/Files/Corregidos"), correcion.RutaArchivo));
                        string fileName = "Estado de cuenta " + unidadTiempo.Descripcion + ".pdf";
                        return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                    }
                }
                //Chequear>

                ReporteLogic reporteLogic = new ReporteLogic();
                reporteLogic.Server = Server;
                reporteLogic.context = context;

                var cuotaDepa = context.Cuota.FirstOrDefault(X => X.DepartamentoId == departamentoId && X.UnidadTiempoId == unidadTiempoId);
                if (cuotaDepa == null)
                {
                    PostMessage(MessageType.Error, "No se encontró recibo para este periodo.");
                    return RedirectToAction("LstDetailEstadoCuenta");
                }
                var presupuestoMes = context.Cuota.Where(X => X.Departamento.EdificioId == cuotaDepa.Departamento.EdificioId && X.UnidadTiempoId == unidadTiempoId).ToList().Sum(X => X.Total);
                long? NumeroRecibo = 0;
                var nroRecibo = departamento.UnidadTiempoReciboDepartamento.FirstOrDefault(x => x.UnidadTiempoId == unidadTiempoId
                    && x.DepartamentoId == departamento.DepartamentoId);
                if (nroRecibo != null)
                {
                    NumeroRecibo = nroRecibo.NumeroRecibo;
                }
                else
                {
                    var ultimoNumeroRecibo = departamento.UnidadTiempoReciboDepartamento.Where(x => x.DepartamentoId == departamento.DepartamentoId).Max(x => x.NumeroRecibo);
                    var ultimoRecibo = departamento.UnidadTiempoReciboDepartamento.FirstOrDefault(x => x.NumeroRecibo == ultimoNumeroRecibo);
                    var diferenciaMes = ultimoRecibo.UnidadTiempo.Orden - unidadTiempo.Orden;

                    if (unidadTiempo.Orden > ultimoRecibo.UnidadTiempo.Orden)
                    {
                        NumeroRecibo = ultimoRecibo.NumeroRecibo + (cantDepartamento * Math.Abs(diferenciaMes.Value));

                        var utRecibo = new UnidadTiempoReciboDepartamento();
                        utRecibo.DepartamentoId = departamento.DepartamentoId;
                        utRecibo.UnidadTiempoId = unidadTiempo.UnidadTiempoId;
                        utRecibo.NumeroRecibo = NumeroRecibo.Value;
                        context.UnidadTiempoReciboDepartamento.Add(utRecibo);
                    }
                    else
                    {
                        NumeroRecibo = ultimoRecibo.NumeroRecibo - (cantDepartamento * Math.Abs(diferenciaMes.Value));

                        var utRecibo = new UnidadTiempoReciboDepartamento();
                        utRecibo.DepartamentoId = departamento.DepartamentoId;
                        utRecibo.UnidadTiempoId = unidadTiempo.UnidadTiempoId;
                        utRecibo.NumeroRecibo = NumeroRecibo.Value;
                        context.UnidadTiempoReciboDepartamento.Add(utRecibo);
                    }
                }

                reporteLogic.GetReport(cuotaDepa, cuotaDepa.FechaEmision ?? DateTime.Now, cuotaDepa.FechaVencimiento ?? DateTime.Now, presupuestoMes, cuotaDepa.Departamento.DepartamentoM2 ?? (decimal)0, context.UnidadTiempo.FirstOrDefault(X => X.UnidadTiempoId == unidadTiempoId), null, null, NumeroRecibo);


                MemoryStream outputMemoryStream = reporteLogic.getFirstReport();
                if (outputMemoryStream == null)
                {
                    PostMessage(MessageType.Error, "Ocurrió un error. Por favor inténtelo más tarde");
                    return RedirectToAction("LstDetailEstadoCuenta");
                }

                return File(outputMemoryStream, "application/pdf", "Reporte Departamento.pdf");
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error, "Ocurrió un error. Por favor inténtelo más tarde");
                return RedirectToAction("LstDetailEstadoCuenta");
            }
        }

        [AppAuthorize(AppRol.Propietario)]
        public ActionResult DownloadReporteGeneral(Int32 departamentoId, Int32 unidadTiempoId)
        {
            try
            {
                ReporteLogic reporteLogic = new ReporteLogic();
                reporteLogic.Server = Server;
                reporteLogic.context = context;

                var departamento = context.Departamento.Where(x => x.DepartamentoId == departamentoId).FirstOrDefault();
                DateTime? fechaEmision = null;
                DateTime? fechaVencimiento = null;

                List<Cuota> listaCuota = new List<Cuota>();
                var departamentos = departamento.Edificio.Departamento.ToList();
                foreach (var depa in departamentos)
                {
                    Cuota cuota = context.Cuota.FirstOrDefault(x => x.DepartamentoId == depa.DepartamentoId && x.UnidadTiempoId == unidadTiempoId && x.UnidadTiempo.Estado == ConstantHelpers.EstadoActivo);
                    if (cuota == null) continue;
                    if (!fechaEmision.HasValue) fechaEmision = cuota.FechaEmision;
                    if (!fechaVencimiento.HasValue) fechaVencimiento = cuota.FechaVencimiento;
                    //listaCuota.Add(cuota);
                    if (cuota.EsExtraordinaria.HasValue && cuota.EsExtraordinaria.Value)
                    {
                        var validacionExtra = listaCuota.FirstOrDefault(x => x.DepartamentoId == cuota.DepartamentoId);
                        if (validacionExtra != null && cuota.FechaPagado != null)
                        {
                            listaCuota.Remove(validacionExtra);

                            if (cuota.UnidadTiempo.Mes == validacionExtra.UnidadTiempo.Mes)
                            {
                                validacionExtra.CuotaExtraordinaria += cuota.CuotaExtraordinaria;
                                validacionExtra.Total += cuota.CuotaExtraordinaria ?? 0;
                            }
                            else if (cuota.UnidadTiempo.Mes != validacionExtra.UnidadTiempo.Mes)
                            {
                                validacionExtra.Total += cuota.CuotaExtraordinaria ?? 0;
                            }

                            listaCuota.Add(validacionExtra);
                        }
                        else
                        {
                            listaCuota.Add(cuota);
                        }
                    }
                    else
                    {
                        listaCuota.Add(cuota);
                    }
                }
                if (!fechaEmision.HasValue || !fechaVencimiento.HasValue || listaCuota.Count == 0)
                {
                    PostMessage(MessageType.Error, "Los reportes aún no han sido generados por el administrador.");
                    return RedirectToAction("LstDetailEstadoCuenta");
                }

                UnidadTiempo unidad = context.UnidadTiempo.Where(x => x.UnidadTiempoId == unidadTiempoId).FirstOrDefault();

                reporteLogic.GetReportTable(listaCuota, unidad.Descripcion);

                MemoryStream outputMemoryStream = reporteLogic.getFirstReport();
                if (outputMemoryStream == null)
                {
                    PostMessage(MessageType.Error, "Ocurrió un error. Por favor inténtelo más tarde");
                    return RedirectToAction("LstDetailEstadoCuenta");
                }

                return File(outputMemoryStream, "application/pdf", "Reporte General.pdf");
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error, "Ocurrió un error. Por favor inténtelo más tarde");
                return RedirectToAction("LstDetailEstadoCuenta");
            }
        }
        [AppAuthorize(AppRol.Propietario)]
        public ActionResult DownloadReporteGeneral2(Int32 departamentoId, Int32 unidadTiempoId)
        {
            try
            {
                ReporteLogic reporteLogic = new ReporteLogic();
                reporteLogic.Server = Server;
                reporteLogic.context = context;

                var departamento = context.Departamento.Where(x => x.DepartamentoId == departamentoId).FirstOrDefault();
                DateTime? fechaEmision = null;
                DateTime? fechaVencimiento = null;

                List<Cuota> listaCuota = new List<Cuota>();
                var departamentos = departamento.Edificio.Departamento.ToList();
                foreach (var depa in departamentos)
                {
                    Cuota cuota = context.Cuota.FirstOrDefault(x => x.DepartamentoId == depa.DepartamentoId && x.UnidadTiempoId == unidadTiempoId && x.UnidadTiempo.Estado == ConstantHelpers.EstadoActivo);
                    if (cuota == null) continue;
                    if (!fechaEmision.HasValue) fechaEmision = cuota.FechaEmision;
                    if (!fechaVencimiento.HasValue) fechaVencimiento = cuota.FechaVencimiento;
                    //listaCuota.Add(cuota);
                    if (cuota.EsExtraordinaria.HasValue && cuota.EsExtraordinaria.Value)
                    {
                        var validacionExtra = listaCuota.FirstOrDefault(x => x.DepartamentoId == cuota.DepartamentoId);
                        if (validacionExtra != null && cuota.FechaPagado != null)
                        {
                            listaCuota.Remove(validacionExtra);

                            if (cuota.UnidadTiempo.Mes == validacionExtra.UnidadTiempo.Mes)
                            {
                                validacionExtra.CuotaExtraordinaria += cuota.CuotaExtraordinaria;
                                validacionExtra.Total += cuota.CuotaExtraordinaria ?? 0;
                            }
                            else if (cuota.UnidadTiempo.Mes != validacionExtra.UnidadTiempo.Mes)
                            {
                                validacionExtra.Total += cuota.CuotaExtraordinaria ?? 0;
                            }

                            listaCuota.Add(validacionExtra);
                        }
                        else
                        {
                            listaCuota.Add(cuota);
                        }
                    }
                    else
                    {
                        listaCuota.Add(cuota);
                    }
                }

                UnidadTiempo unidad = context.UnidadTiempo.Where(x => x.UnidadTiempoId == unidadTiempoId).FirstOrDefault();

                reporteLogic.GetReportTable(listaCuota, unidad.Descripcion);

                MemoryStream outputMemoryStream = reporteLogic.getFirstReport();
                if (outputMemoryStream == null)
                {
                    PostMessage(MessageType.Error, "Ocurrió un error. Por favor inténtelo más tarde");
                    return RedirectToAction("LstDetailEstadoCuenta");
                }

                return File(outputMemoryStream, "application/pdf", "Reporte General.pdf");
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error, "Ocurrió un error. Por favor inténtelo más tarde");
                return RedirectToAction("LstDetailEstadoCuenta");
            }
        }
    }
}
