using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.Filters;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Spending;
using System.IO;
using System.Transactions;

namespace VEH.Intranet.Controllers
{

    public class SpendingController : BaseController
    {
        //
        // GET: /Spending/

        #region gasto

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-shopping-cart")]
        public ActionResult LstGastoAdmin(Int32 EdificioId, Int32? np, Int32? Anio)
        {
            LstGastoViewModel ViewModel = new LstGastoViewModel();
            ViewModel.EdificioId = EdificioId;
            ViewModel.Fill(CargarDatosContext(), np, Anio);
            return View(ViewModel);
        }

        [AppAuthorize(AppRol.Propietario)]
        [ViewParameter("Gasto", "fa fa-shopping-cart")]
        public ActionResult LstGasto(Int32? GastoId, Int32? np1, Int32? np2, Int32? Anio, Int32? Mes, Int32? EdificioId, Int32 pestania = 1)
        {
            LstDetalleArchivoGastoViewModel ViewModel = new LstDetalleArchivoGastoViewModel();
            var gasto = context.Gasto.FirstOrDefault(x => x.EdificioId == EdificioId && x.UnidadTiempo.Anio == Anio && x.UnidadTiempo.Mes == Mes);
            if (gasto != null)
            {
                GastoId = gasto.GastoId;
            }


            ViewModel.LstArchivo = new LstArchivoGastoViewModel();
            ViewModel.LstDetalle = new LstDetalleGastoViewModel();
            ViewModel.Fill(CargarDatosContext(), SessionHelpers.GetEdificioId(Session));
            ViewModel.LstArchivo.EdificioId = SessionHelpers.GetEdificioId(Session);
            ViewModel.LstDetalle.EdificioId = SessionHelpers.GetEdificioId(Session);
            ViewModel.GastoId = GastoId;
            ViewModel.LstArchivo.GastoId = GastoId;
            ViewModel.LstDetalle.GastoId = GastoId;
            ViewModel.LstDetalle.Fill(CargarDatosContext(), np1);
            ViewModel.LstArchivo.Fill(CargarDatosContext(), np2, Anio);
            ViewModel.Pestania = pestania;
            if (GastoId.HasValue)
                ViewModel.UnidadTiempoId = context.Gasto.First(X => X.GastoId == GastoId).UnidadTiempoId;
            return View(ViewModel);
        }

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter(PageIcon: "fa fa-shopping-cart")]
        public ActionResult _AddEditGasto(Int32? GastoId, Int32 EdificioId)
        {
            AddEditGastoViewModel ViewModel = new AddEditGastoViewModel();
            ViewModel.GastoId = GastoId;
            ViewModel.EdificioId = EdificioId;
            ViewModel.Fill(CargarDatosContext());
            ViewModel.FillComboUnidadTiempo(CargarDatosContext());
            return PartialView(ViewModel);
        }

        [HttpPost]
        public ActionResult AddEditGasto(AddEditGastoViewModel ViewModel)
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
                if (ViewModel.GastoId.HasValue)
                {
                    var unidadDeTiempo = context.UnidadTiempo.FirstOrDefault(x => x.UnidadTiempoId == ViewModel.UnidadTiempoId);
                    if (context.Gasto.Any(x => x.GastoId != ViewModel.GastoId && x.EdificioId == ViewModel.EdificioId && x.Estado.Equals(ConstantHelpers.EstadoActivo) &&
                                                                                       x.UnidadTiempo.Mes == unidadDeTiempo.Mes &&
                                                                                       x.UnidadTiempo.Anio == unidadDeTiempo.Anio))
                    {
                        //Ya existe, retornar error de Already exist
                        PostMessage(MessageType.AExist);
                        return RedirectToAction("LstGastoAdmin", new { EdificioId = ViewModel.EdificioId });
                    }

                    Gasto gasto = context.Gasto.FirstOrDefault(x => x.GastoId == ViewModel.GastoId.Value);
                    gasto.UnidadTiempoId = ViewModel.UnidadTiempoId;
                    gasto.CuentasPorCobrarO = ViewModel.CuentasPorCobrarO;
                    gasto.CuentasPorCobrarE = ViewModel.CuentasPorCobrarE;
                    context.Entry(gasto).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    var unidadDeTiempo = context.UnidadTiempo.FirstOrDefault(x => x.UnidadTiempoId == ViewModel.UnidadTiempoId);
                    if (context.Gasto.Any(x => x.EdificioId == ViewModel.EdificioId && x.Estado.Equals(ConstantHelpers.EstadoActivo) &&
                                                                                       x.UnidadTiempo.Mes == unidadDeTiempo.Mes &&
                                                                                       x.UnidadTiempo.Anio == unidadDeTiempo.Anio))
                    {
                        //Ya existe, retornar error de Already exist
                        PostMessage(MessageType.AExist);
                        return RedirectToAction("LstGastoAdmin", new { EdificioId = ViewModel.EdificioId });
                    }

                    Gasto gasto = new Gasto();
                    gasto.EdificioId = ViewModel.EdificioId;
                    gasto.UnidadTiempoId = ViewModel.UnidadTiempoId;
                    gasto.FechaRegistro = DateTime.Now;
                    gasto.Estado = ConstantHelpers.EstadoActivo;
                    gasto.CuentasPorCobrarO = ViewModel.CuentasPorCobrarO;
                    gasto.CuentasPorCobrarE = ViewModel.CuentasPorCobrarE;

                    context.Gasto.Add(gasto);
                    context.SaveChanges();

                    //Copia de los gastos de la unidad de tiempo antigua del mismo edificio

                    Gasto tempGasto = context.Gasto.OrderByDescending(X => X.FechaRegistro).FirstOrDefault(x => x.EdificioId == gasto.EdificioId && (x.UnidadTiempoId != gasto.UnidadTiempoId && x.UnidadTiempo.Estado.Equals(ConstantHelpers.EstadoActivo)));
                    if (tempGasto != null)
                    {
                        context.DetalleGasto.Where(x => x.GastoId == tempGasto.GastoId).ToList().ForEach(x =>
                        {
                            x.GastoId = gasto.GastoId;
                            x.Monto = 0;
                            context.DetalleGasto.Add(x);
                        }
                        );
                    }



                }
                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch { PostMessage(MessageType.Error); }
            return RedirectToAction("LstGastoAdmin", new { EdificioId = ViewModel.EdificioId });
        }

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter(PageIcon: "fa fa-trash-o")]
        public ActionResult _DeleteGasto(Int32 GastoId, Int32 EdificioId)
        {
            ViewBag.GastoId = GastoId;
            ViewBag.EdificioId = EdificioId;
            return PartialView();
        }

        [HttpPost]
        public ActionResult DeleteGasto(Int32 GastoId, Int32 EdificioId)
        {
            try
            {
                Gasto gasto = context.Gasto.FirstOrDefault(x => x.GastoId == GastoId);
                gasto.Estado = ConstantHelpers.EstadoInactivo;
                context.Entry(gasto).State = System.Data.Entity.EntityState.Modified;
                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch { PostMessage(MessageType.Error); }
            return RedirectToAction("LstGastoAdmin", new { EdificioId = EdificioId });
        }
        #endregion

        #region detalle gasto

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-shopping-cart")]
        public ActionResult LstDetalleGastoAdmin(Int32 GastoId, Int32 EdificioId, Int32? np)
        {
            LstDetalleGastoViewModel ViewModel = new LstDetalleGastoViewModel();
            ViewModel.GastoId = GastoId;
            ViewModel.EdificioId = EdificioId;
            ViewModel.Fill(CargarDatosContext(), np);
            return View(ViewModel);
        }

        [AppAuthorize(AppRol.Propietario)]
        [ViewParameter("Edificio", "fa fa-shopping-cart")]
        public ActionResult LstDetalleGasto(Int32 GastoId, Int32? np)
        {
            LstDetalleGastoViewModel ViewModel = new LstDetalleGastoViewModel();
            ViewModel.GastoId = GastoId;
            ViewModel.EdificioId = SessionHelpers.GetEdificioId(Session);
            ViewModel.Fill(CargarDatosContext(), np);
            return View(ViewModel);
        }

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-shopping-cart")]
        public ActionResult AddEditDetalleGasto(Int32? DetalleGastoId, Int32 GastoId, Int32 EdificioId)
        {
            AddEditDetalleGastoViewModel ViewModel = new AddEditDetalleGastoViewModel();
            ViewModel.DetalleGastoId = DetalleGastoId;
            ViewModel.GastoId = GastoId;
            ViewModel.EdificioId = EdificioId;
            ViewModel.Fill(CargarDatosContext());
            return View(ViewModel);
        }

        [HttpPost]
        public ActionResult AddEditDetalleGasto(AddEditDetalleGastoViewModel ViewModel, FormCollection formCollection)
        {
            if (!ModelState.IsValid)
            {
                ViewModel.Fill(CargarDatosContext());
                TryUpdateModel(ViewModel);
                return View(ViewModel);
            }
            try
            {
                DetalleGasto _detallegasto = null;
                if (ViewModel.DetalleGastoId.HasValue)
                {
                    _detallegasto = context.DetalleGasto.FirstOrDefault(x => x.DetalleGastoId == ViewModel.DetalleGastoId.Value);

                    context.Entry(_detallegasto).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    _detallegasto = new DetalleGasto();

                    _detallegasto.GastoId = ViewModel.GastoId;
                    _detallegasto.Estado = ConstantHelpers.EstadoActivo;
                    _detallegasto.FechaRegistro = DateTime.Now;

                    context.DetalleGasto.Add(_detallegasto);
                }

                _detallegasto.Concepto = ViewModel.Concepto;

                var montosAdicionales = ViewModel.MontoAdicional.Replace("-", "+-").Split('+');
                Decimal SumMontoAdicional = 0;
                foreach (var ma in montosAdicionales)
                {
                    if (!String.IsNullOrEmpty(ma))
                    {
                        SumMontoAdicional += ma.ToDecimal();
                    }
                }


                _detallegasto.Monto = ViewModel.Monto.ToDecimal() + SumMontoAdicional;
                _detallegasto.Pagado = ViewModel.Pagado;
                _detallegasto.Ordinario = ViewModel.Ordinario;
                _detallegasto.Orden = ViewModel.Orden;

                var camposAdicionales = formCollection.AllKeys.Where(X => X.StartsWith("campo")).Select(X => X.Substring(5)).ToList();

                String detalle = "";
                foreach (var campo in camposAdicionales)
                {
                    Decimal d = 0;
                    try
                    {
                        var frmCampo = formCollection["detalle" + campo];
                        if (!String.IsNullOrEmpty(frmCampo))
                        {
                            var arraySuma = frmCampo.Split('+');
                            foreach (var digito in arraySuma)
                            {
                                if (!String.IsNullOrEmpty(digito))
                                {
                                    d += digito.ToDecimal();
                                }
                            }
                        }
                        //d = formCollection["detalle" + campo].ToDecimal();
                    }
                    catch (Exception ex)
                    {
                        d = 0;
                    }
                    detalle += campo + ";" + d.ToString("#,##0.00") + "|";
                }
                _detallegasto.Detalle = detalle;

                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch { PostMessage(MessageType.Error); }
            return RedirectToAction("LstDetalleGastoAdmin", new { GastoId = ViewModel.GastoId, EdificioId = ViewModel.EdificioId });
        }
        [HttpPost]
        public ActionResult GuardarOrdenDetalleGasto(FormCollection frm, Int32 GastoId, Int32 EdificioId)
        {
            try
            {
                var LstDetalleGasto = frm.AllKeys.Where(X => X.StartsWith("id-")).ToList();
                for (int i = 1; i <= LstDetalleGasto.Count; i++)
                {
                    var Id = LstDetalleGasto[i - 1].Replace("id-", String.Empty).ToInteger();
                    var detalleGasto = context.DetalleGasto.FirstOrDefault(x => x.DetalleGastoId == Id);
                    detalleGasto.Orden = i;
                }

                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error);
            }
            return RedirectToAction("LstDetalleGastoAdmin", new { GastoId = GastoId, EdificioId = EdificioId });
        }

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter(PageIcon: "fa fa-trash-o")]
        public ActionResult _DeleteDetalleGasto(Int32 DetalleGastoId, Int32 GastoId, Int32 EdificioId)
        {
            ViewBag.DetalleGastoId = DetalleGastoId;
            ViewBag.GastoId = GastoId;
            ViewBag.EdificioId = EdificioId;
            return PartialView();
        }

        [HttpPost]
        public ActionResult DeleteDetalleGasto(Int32 DetalleGastoId, Int32 GastoId, Int32 EdificioId)
        {
            try
            {
                DetalleGasto _detallegasto = context.DetalleGasto.FirstOrDefault(x => x.DetalleGastoId == DetalleGastoId);
                context.DetalleGasto.Remove(_detallegasto);
                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch { }
            return RedirectToAction("LstDetalleGastoAdmin", new { GastoId = GastoId, EdificioId = EdificioId });
        }
        #endregion

        #region archivos gasto

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-file")]
        public ActionResult LstArchivoGastoAdmin(Int32 GastoId, Int32 EdificioId, Int32? np, Int32? UnidadTiempoId)
        {
            LstArchivoGastoViewModel ViewModel = new LstArchivoGastoViewModel();
            ViewModel.GastoId = GastoId;
            ViewModel.EdificioId = EdificioId;
            ViewModel.Fill(CargarDatosContext(), np, UnidadTiempoId);
            return View(ViewModel);
        }

        [AppAuthorize(AppRol.Propietario)]
        [ViewParameter("Edificio", "fa fa-file")]
        public ActionResult LstArchivoGasto(Int32 GastoId, Int32? np, Int32? UnidadTiempoId)
        {
            LstArchivoGastoViewModel ViewModel = new LstArchivoGastoViewModel();
            ViewModel.GastoId = GastoId;
            ViewModel.EdificioId = SessionHelpers.GetEdificioId(Session);
            ViewModel.Fill(CargarDatosContext(), np, UnidadTiempoId);
            return View(ViewModel);
        }

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-file")]
        public ActionResult AddEditArchivoGasto(Int32? ArchivoGastoId, Int32 GastoId, Int32 EdificioId)
        {
            AddEditArchivoGastoViewModel ViewModel = new AddEditArchivoGastoViewModel();
            ViewModel.ArchivoGastoId = ArchivoGastoId;
            ViewModel.GastoId = GastoId;
            ViewModel.EdificioId = EdificioId;
            ViewModel.Fill(CargarDatosContext());
            return View(ViewModel);
        }

        [HttpPost]
        public ActionResult AddEditArchivoGasto(AddEditArchivoGastoViewModel ViewModel)
        {
            try
            {
                using (var transacionscope = new TransactionScope())
                {
                    Gasto gasto = context.Gasto.FirstOrDefault(x => x.GastoId == ViewModel.GastoId);
                    string _rutaarchivoserv = Server.MapPath("~") + "\\";
                    Edificio edificio = context.Edificio.FirstOrDefault(x => x.EdificioId == ViewModel.EdificioId);
                    if (ViewModel.ArchivoGastoId.HasValue)
                    {
                        ArchivoGasto _Archivo = context.ArchivoGasto.FirstOrDefault(x => x.ArchivoGastoId == ViewModel.ArchivoGastoId.Value);
                        var _rutaarchivodir = _rutaarchivoserv + Path.Combine("Resources\\Files", edificio.Acronimo);
                        var auxDir = _rutaarchivodir;
                        _rutaarchivodir = Path.Combine(_rutaarchivodir, _Archivo.Ruta);
                        if (System.IO.File.Exists(_rutaarchivodir))
                        {
                            var extension = _Archivo.Ruta.Split('.');
                            var cantPuntos = extension.Count();
                            var nuevoNombre = ViewModel.Nombre + "." + extension[cantPuntos - 1];
                            System.IO.File.Move(_rutaarchivodir, Path.Combine(auxDir, nuevoNombre));
                            _Archivo.Nombre = nuevoNombre;//ViewModel.Nombre;
                            _Archivo.Ruta = nuevoNombre;//ViewModel.Nombre; //ViewModel.Nombre;
                        }
                        else
                        {
                            PostMessage(MessageType.Warning,"No se encontró ruta " + _rutaarchivodir);
                            _Archivo.Nombre = ViewModel.Nombre; //ViewModel.Nombre;
                        }
                    }
                    else
                    {
                        foreach (var file in ViewModel.Archivo)
                        {
                            ArchivoGasto _Archivo = new ArchivoGasto();
                            _Archivo.Nombre = ViewModel.DescripcionUnidadMedida + "_" + file.FileName; //ViewModel.Nombre;

                            string _rutaarchivodir = _rutaarchivoserv + Path.Combine("/Resources/Files", edificio.Acronimo);
                            if (!System.IO.Directory.Exists(_rutaarchivodir))
                                Directory.CreateDirectory(_rutaarchivodir);

                            string _nombrearc = ViewModel.DescripcionUnidadMedida + "_" +  file.FileName;// + Path.GetExtension(file.FileName);

                            _rutaarchivodir = Path.Combine(_rutaarchivodir, _nombrearc);
                            _Archivo.Ruta = _nombrearc;
                            _Archivo.Estado = ConstantHelpers.EstadoActivo;
                            _Archivo.FechaRegistro = DateTime.Now;
                            _Archivo.GastoId = ViewModel.GastoId;
                            file.SaveAs(_rutaarchivodir);
                            context.ArchivoGasto.Add(_Archivo);
                        }
                    }
                    context.SaveChanges();
                    transacionscope.Complete();
                    PostMessage(MessageType.Success);
                }
            }
            catch { InvalidarContext(); PostMessage(MessageType.Error); }
            return RedirectToAction("LstArchivoGastoAdmin", new { GastoId = ViewModel.GastoId, EdificioId = ViewModel.EdificioId });
        }

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter(PageIcon: "fa fa-trash-o")]
        public ActionResult _DeleteArchivoGasto(Int32 ArchivoGastoId, Int32 GastoId, Int32 EdificioId)
        {
            ViewBag.ArchivoGastoId = ArchivoGastoId;
            ViewBag.GastoId = GastoId;
            ViewBag.EdificioId = EdificioId;
            return PartialView();
        }

        [HttpPost]
        public ActionResult DeleteArchivoGasto(Int32 ArchivoGastoId, Int32 GastoId, Int32 EdificioId)
        {
            try
            {
                ArchivoGasto _Archivo = context.ArchivoGasto.FirstOrDefault(x => x.ArchivoGastoId == ArchivoGastoId);
                _Archivo.Estado = ConstantHelpers.EstadoInactivo;
                context.Entry(_Archivo).State = System.Data.Entity.EntityState.Modified;
                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch { }
            return RedirectToAction("LstArchivoGastoAdmin", new { GastoId = GastoId, EdificioId = EdificioId });
        }

        [AppAuthorize(AppRol.Administrador, AppRol.Propietario)]
        public ActionResult DescargarArchivo(string ruta, string nombre, string acronimo)
        {

            var buffer = Path.Combine(Server.MapPath("~/Resources/Files"), acronimo, ruta);
            return File(buffer, "application/octet-stream", nombre + "." + ruta.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries)[1]);
        }
        #endregion


    }
}
