using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using VEH.Intranet.Filters;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Departament;
using VEH.Intranet.Logic;
using System.IO;

namespace VEH.Intranet.Controllers
{
    [AppAuthorize(AppRol.Administrador)]
    public class DepartamentController : BaseController
    {
        [ViewParameter("Edificio", "fa fa-desktop")]
        public ActionResult LstDepartamento(Int32 EdificioId)
        {
            LstDepartamentoViewModel ViewModel = new LstDepartamentoViewModel();
            ViewModel.EdificioId = EdificioId;
            ViewModel.Fill(CargarDatosContext());
            return View(ViewModel);
        }

        [ViewParameter("Edificio", "fa fa-desktop")]
        public ActionResult AddEditDepartamento(Int32? DepartamentoId, Int32 EdificioId)
        {
            AddEditDepartamentoViewModel ViewModel = new AddEditDepartamentoViewModel();
            ViewModel.EdificioId = EdificioId;
            ViewModel.DepartamentoId = DepartamentoId;
            ViewModel.Fill(CargarDatosContext());
            return View(ViewModel);
        }

        public ActionResult DownloadRelacionPropietario(Int32 EdificioId)
        {
            try
            {
                var edificio = context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);
                ReporteLogic reporteLogic = new ReporteLogic();
                reporteLogic.Server = Server;
                reporteLogic.context = context;

                MemoryStream outputMemoryStream = reporteLogic.GetReportRelacionPropietario(EdificioId, edificio.Nombre);
                //reporteLogic.GetReport(c, fechaEmision.Value, fechaVencimiento.Value, presupuestoMes, totalM2);

                //MemoryStream outputMemoryStream = reporteLogic.getFirstReport();
                if (outputMemoryStream == null)
                {
                    PostMessage(MessageType.Error, "Ocurrió un error. Por favor inténtelo más tarde");
                    return RedirectToAction("LstDepartamento", new { EdificioId = EdificioId });
                }

                return File(outputMemoryStream, "application/octet-stream", "Relacion de Propietarios - " + edificio.Nombre + ".zip");
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error, "Ocurrió un error. Por favor inténtelo más tarde");
                return RedirectToAction("LstDepartamento", new { EdificioId = EdificioId });
            }
        }
        [ViewParameter("Edificio", "fa fa-desktop")]
        public ActionResult ViewDepartamento(Int32 DepartamentoId, Int32 EdificioId)
        {
            AddEditDepartamentoViewModel ViewModel = new AddEditDepartamentoViewModel();
            ViewModel.EdificioId = EdificioId;
            ViewModel.DepartamentoId = DepartamentoId;
            ViewModel.Fill(CargarDatosContext());
            return View(ViewModel);
        }

        [HttpPost]
        public ActionResult AddEditDepartamento(AddEditDepartamentoViewModel ViewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewModel.Fill(CargarDatosContext());
                TryUpdateModel(ViewModel);
                return View(ViewModel);
            }

            try
            {
                if (!ViewModel.DepartamentoId.HasValue)
                {
                    Departamento _nuevo = new Departamento();
                    _nuevo.EdificioId = ViewModel.EdificioId.ToInteger();
                    _nuevo.Numero = ViewModel.Numero;

                    try
                    {
                        _nuevo.Piso = Int32.Parse(ViewModel.Numero) / 100;
                    }
                    catch (Exception ex)
                    {
                        _nuevo.Piso = 0;
                    }

                    _nuevo.NombrePropietario = ViewModel.NombreReciboDepartamento == "P" ? true : false;
                    _nuevo.NombreRecibo = _nuevo.NombrePropietario ? "PROPIETARIO" : "INQUILINO";

                    _nuevo.LecturaAgua = ViewModel.LecturaAgua;
                    _nuevo.Estado = ConstantHelpers.EstadoActivo;
                    _nuevo.FactorGasto = ViewModel.FactorGasto;
                    _nuevo.Estacionamiento = ViewModel.Estacionamiento;
                    _nuevo.Deposito = ViewModel.Deposito;
                    _nuevo.DepartamentoM2 = ViewModel.DepartamentoM2;
                    _nuevo.EstacionamientoM2 = ViewModel.EstacionamientoM2;
                    _nuevo.DepositoM2 = ViewModel.DepositoM2;
                    _nuevo.PDistribucion = ViewModel.PDistribucion;
                    _nuevo.MontoMora = 0M;
                    _nuevo.TotalM2 = ViewModel.AreaTotalM2;
                    _nuevo.OmitirMora = false;
                    _nuevo.CuotaDefault = ViewModel.CuotaDefault;
                    _nuevo.TipoInmuebleId = ViewModel.TipoInmuebleId;
                    //_nuevo.NombreRecibo = ViewModel.NombreRecibo;
                    _nuevo.AlertaMora = ViewModel.AlertaMoraDrop == "1" ? true : false;
                    context.Departamento.Add(_nuevo);

                    var numeroRecibo = context.UnidadTiempoReciboDepartamento.Where(x => x.Departamento.EdificioId == ViewModel.EdificioId).Max( x => x.NumeroRecibo);
                    var unidadTiempoId = context.UnidadTiempoReciboDepartamento.FirstOrDefault(x => x.NumeroRecibo == numeroRecibo).UnidadTiempoId;
                    

                    var utRecibo = new UnidadTiempoReciboDepartamento();
                    utRecibo.NumeroRecibo = numeroRecibo + 1;
                    utRecibo.UnidadTiempoId = unidadTiempoId;
                    utRecibo.Departamento = _nuevo;
                    context.UnidadTiempoReciboDepartamento.Add(utRecibo);
                }
                else
                {
                    Departamento _editado = context.Departamento.FirstOrDefault(x => x.DepartamentoId == ViewModel.DepartamentoId.Value);
                    if (_editado != null)
                    {
                        _editado.Numero = ViewModel.Numero;
                        try
                        {
                            _editado.Piso = Int32.Parse(ViewModel.Numero) / 100;
                        }
                        catch (Exception ex)
                        {
                            _editado.Piso = 0;
                        }
                        _editado.NombrePropietario = ViewModel.NombreReciboDepartamento == "P" ? true : false;
                        _editado.NombreRecibo = _editado.NombrePropietario ? "PROPIETARIO" : "INQUILINO";

                        _editado.LecturaAgua = ViewModel.LecturaAgua;
                        _editado.Estado = ConstantHelpers.EstadoActivo;
                        _editado.FactorGasto = ViewModel.FactorGasto;
                        _editado.Estacionamiento = ViewModel.Estacionamiento;
                        _editado.Deposito = ViewModel.Deposito;
                        _editado.CuotaDefault = ViewModel.CuotaDefault;
                        _editado.DepartamentoM2 = ViewModel.DepartamentoM2;
                        _editado.EstacionamientoM2 = ViewModel.EstacionamientoM2;
                        _editado.DepositoM2 = ViewModel.DepositoM2;
                        _editado.PDistribucion = ViewModel.PDistribucion;
                        _editado.TotalM2 = ViewModel.AreaTotalM2;
                        _editado.TipoInmuebleId = ViewModel.TipoInmuebleId;
                        //_editado.NombreRecibo = ViewModel.NombreRecibo;
                        _editado.AlertaMora = ViewModel.AlertaMoraDrop == "1" ? true : false;
                        context.Entry(_editado).State = System.Data.Entity.EntityState.Modified;
                    }
                }
                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch { PostMessage(MessageType.Error); }
            return RedirectToAction("LstDepartamento", new { DepartamentoId = ViewModel.DepartamentoId, EdificioId = ViewModel.EdificioId });
        }

        public ActionResult _ActDeactDepartamento(Int32 DepartamentoId, String Estado, Int32 EdificioId)
        {
            ViewBag.DepartamentoId = DepartamentoId;
            ViewBag.Estado = Estado;
            ViewBag.EdificioId = EdificioId;
            return PartialView();
        }

        [HttpPost]
        public ActionResult ActDeactDepartamento(Int32 DepartamentoId, Int32 EdificioId)
        {
            try
            {
                Departamento Departamento = context.Departamento.FirstOrDefault(x => x.DepartamentoId == DepartamentoId && x.EdificioId == EdificioId);
                if (Departamento != null)
                {
                    Departamento.Estado = Departamento.Estado == ConstantHelpers.EstadoInactivo ? ConstantHelpers.EstadoActivo : ConstantHelpers.EstadoInactivo;
                    context.Entry(Departamento).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                    PostMessage(MessageType.Success);
                }
            }
            catch { PostMessage(MessageType.Error); }
            return RedirectToAction("LstDepartamento", new { EdificioId = EdificioId });
        }
    }
}
