using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.Filters;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Income;
using System.IO;
using System.Transactions;

namespace VEH.Intranet.Controllers
{

    public class IncomeController : BaseController
    {
        //
        // GET: /Income/

        #region ingreso

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-shopping-cart")]
        public ActionResult LstIngresoAdmin(Int32 EdificioId, Int32? np, Int32? Anio)
        {
            LstIngresoViewModel ViewModel = new LstIngresoViewModel();
            ViewModel.EdificioId = EdificioId;
            ViewModel.Fill(CargarDatosContext(), np, Anio);
            return View(ViewModel);
        }

        [AppAuthorize(AppRol.Propietario)]
        [ViewParameter("Ingreso", "fa fa-shopping-cart")]
        public ActionResult LstIngreso(Int32? IngresoId, Int32? np1, Int32? np2, Int32 pestania = 1)
        {
            LstDetalleArchivoIngresoViewModel ViewModel = new LstDetalleArchivoIngresoViewModel();
            ViewModel.LstArchivo = new LstArchivoIngresoViewModel();
            ViewModel.LstDetalle = new LstDetalleIngresoViewModel();
            ViewModel.Fill(CargarDatosContext(), SessionHelpers.GetEdificioId(Session));
            ViewModel.LstArchivo.EdificioId = SessionHelpers.GetEdificioId(Session);
            ViewModel.LstDetalle.EdificioId = SessionHelpers.GetEdificioId(Session);
            ViewModel.IngresoId = IngresoId;
            ViewModel.LstArchivo.IngresoId = IngresoId;
            ViewModel.LstDetalle.IngresoId = IngresoId;           
            ViewModel.LstDetalle.Fill(CargarDatosContext(), np1);
            ViewModel.LstArchivo.Fill(CargarDatosContext(), np2);
            ViewModel.Pestania = pestania;
            return View(ViewModel);
        }

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter(PageIcon: "fa fa-shopping-cart")]
        public ActionResult _AddEditIngreso(Int32? IngresoId, Int32 EdificioId)
        {
            AddEditIngresoViewModel ViewModel = new AddEditIngresoViewModel();
            ViewModel.IngresoId = IngresoId;
            ViewModel.EdificioId = EdificioId;
            ViewModel.Fill(CargarDatosContext());
            ViewModel.FillComboUnidadTiempo(CargarDatosContext());
            return PartialView(ViewModel);
        }

        [HttpPost]
        public ActionResult AddEditIngreso(AddEditIngresoViewModel ViewModel)
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
                if (ViewModel.IngresoId.HasValue)
                {
                    var unidadDeTiempo = context.UnidadTiempo.FirstOrDefault(x => x.UnidadTiempoId == ViewModel.UnidadTiempoId);
                    if (context.Ingreso.Any(x => x.IngresoId != ViewModel.IngresoId && x.EdificioId == ViewModel.EdificioId && x.Estado.Equals(ConstantHelpers.EstadoActivo) &&
                                                                                       x.UnidadTiempo.Mes == unidadDeTiempo.Mes &&
                                                                                       x.UnidadTiempo.Anio == unidadDeTiempo.Anio))
                    {
                        //Ya existe, retornar error de Already exist
                        PostMessage(MessageType.AExist);
                        return RedirectToAction("LstIngresoAdmin", new { EdificioId = ViewModel.EdificioId });
                    }

                    Ingreso ingreso = context.Ingreso.FirstOrDefault(x => x.IngresoId == ViewModel.IngresoId.Value);
                    ingreso.UnidadTiempoId = ViewModel.UnidadTiempoId;
                    context.Entry(ingreso).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    var unidadDeTiempo = context.UnidadTiempo.FirstOrDefault(x => x.UnidadTiempoId == ViewModel.UnidadTiempoId);
                    if (context.Ingreso.Any(x => x.EdificioId == ViewModel.EdificioId && x.Estado.Equals(ConstantHelpers.EstadoActivo) &&
                                                                                       x.UnidadTiempo.Mes == unidadDeTiempo.Mes &&
                                                                                       x.UnidadTiempo.Anio == unidadDeTiempo.Anio))
                    {
                        //Ya existe, retornar error de Already exist
                        PostMessage(MessageType.AExist);
                        return RedirectToAction("LstIngresoAdmin", new { EdificioId = ViewModel.EdificioId });
                    }

                    Ingreso ingreso = new Ingreso();
                    ingreso.EdificioId = ViewModel.EdificioId;
                    ingreso.UnidadTiempoId = ViewModel.UnidadTiempoId;
                    ingreso.FechaRegistro = DateTime.Now;
                    ingreso.Estado = ConstantHelpers.EstadoActivo;

                    context.Ingreso.Add(ingreso);
                    context.SaveChanges();
                    
                    //Copia de los ingresos de la unidad de tiempo antigua del mismo edificio
                    
                    Ingreso tempIngreso = context.Ingreso.FirstOrDefault(x => x.EdificioId == ingreso.EdificioId && (x.UnidadTiempoId != ingreso.UnidadTiempoId && x.UnidadTiempo.Estado.Equals(ConstantHelpers.EstadoActivo) ));
                    if (tempIngreso != null)
                    {
                        //List<DetalleIngreso> LstDetalleIngreso = 
                        context.DetalleIngreso.Where(x => x.IngresoId == tempIngreso.IngresoId).ToList().ForEach(x =>
                        {
                            x.IngresoId = ingreso.IngresoId;
                            context.DetalleIngreso.Add(x);
                        }
                        );
                    }
                    

                    
                }
                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch { PostMessage(MessageType.Error); }
            return RedirectToAction("LstIngresoAdmin", new { EdificioId = ViewModel.EdificioId });
        }

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter(PageIcon: "fa fa-trash-o")]
        public ActionResult _DeleteIngreso(Int32 IngresoId, Int32 EdificioId)
        {
            ViewBag.IngresoId = IngresoId;
            ViewBag.EdificioId = EdificioId;
            return PartialView();
        }

        [HttpPost]
        public ActionResult DeleteIngreso(Int32 IngresoId, Int32 EdificioId)
        {
            try
            {
                Ingreso ingreso = context.Ingreso.FirstOrDefault(x => x.IngresoId == IngresoId);
                ingreso.Estado = ConstantHelpers.EstadoInactivo;
                context.Entry(ingreso).State = System.Data.Entity.EntityState.Modified;
                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch { PostMessage(MessageType.Error); }
            return RedirectToAction("LstIngresoAdmin", new { EdificioId = EdificioId });
        }
        #endregion

        #region detalle ingreso

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-shopping-cart")]
        public ActionResult LstDetalleIngresoAdmin(Int32 IngresoId, Int32 EdificioId, Int32? np)
        {
            LstDetalleIngresoViewModel ViewModel = new LstDetalleIngresoViewModel();
            ViewModel.IngresoId = IngresoId;
            ViewModel.EdificioId = EdificioId;
            ViewModel.Fill(CargarDatosContext(), np);
            return View(ViewModel);
        }

        [AppAuthorize(AppRol.Propietario)]
        [ViewParameter("Edificio", "fa fa-shopping-cart")]
        public ActionResult LstDetalleIngreso(Int32 IngresoId, Int32? np)
        {
            LstDetalleIngresoViewModel ViewModel = new LstDetalleIngresoViewModel();
            ViewModel.IngresoId = IngresoId;
            ViewModel.EdificioId = SessionHelpers.GetEdificioId(Session);
            ViewModel.Fill(CargarDatosContext(), np);
            return View(ViewModel);
        }

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-shopping-cart")]
        public ActionResult AddEditDetalleIngreso(Int32? DetalleIngresoId, Int32 IngresoId, Int32 EdificioId)
        {
            AddEditDetalleIngresoViewModel ViewModel = new AddEditDetalleIngresoViewModel();
            ViewModel.DetalleIngresoId = DetalleIngresoId;
            ViewModel.IngresoId = IngresoId;
            ViewModel.EdificioId = EdificioId;
            ViewModel.Fill(CargarDatosContext());
            return View(ViewModel);
        }

        [HttpPost]
        public ActionResult AddEditDetalleIngreso(AddEditDetalleIngresoViewModel ViewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewModel.Fill(CargarDatosContext());
                TryUpdateModel(ViewModel);
                return View(ViewModel);
            }
            try
            {
                if (ViewModel.DetalleIngresoId.HasValue)
                {
                    DetalleIngreso _detalleingreso = context.DetalleIngreso.FirstOrDefault(x => x.DetalleIngresoId == ViewModel.DetalleIngresoId.Value);
                    _detalleingreso.Concepto = ViewModel.Concepto;
                    _detalleingreso.Monto = ViewModel.Monto.ToDecimal() + ViewModel.MontoAdicional.ToDecimal();
                    _detalleingreso.Pagado = ViewModel.Pagado;                    
                    context.Entry(_detalleingreso).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    DetalleIngreso _detalleingreso = new DetalleIngreso();
                    _detalleingreso.Concepto = ViewModel.Concepto;
                    _detalleingreso.Monto = ViewModel.Monto.ToDecimal() + ViewModel.MontoAdicional.ToDecimal(); //RENZO agregado
                    _detalleingreso.IngresoId = ViewModel.IngresoId;
                    _detalleingreso.Estado = ConstantHelpers.EstadoActivo;
                    _detalleingreso.FechaRegistro = DateTime.Now;
                    _detalleingreso.Pagado = ViewModel.Pagado;
                    context.DetalleIngreso.Add(_detalleingreso);
                }
                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch { PostMessage(MessageType.Error); }
            return RedirectToAction("LstDetalleIngresoAdmin", new { IngresoId = ViewModel.IngresoId, EdificioId = ViewModel.EdificioId });
        }

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter(PageIcon: "fa fa-trash-o")]
        public ActionResult _DeleteDetalleIngreso(Int32 DetalleIngresoId, Int32 IngresoId, Int32 EdificioId)
        {
            ViewBag.DetalleIngresoId = DetalleIngresoId;
            ViewBag.IngresoId = IngresoId;
            ViewBag.EdificioId = EdificioId;
            return PartialView();
        }

        [HttpPost]
        public ActionResult DeleteDetalleIngreso(Int32 DetalleIngresoId, Int32 IngresoId, Int32 EdificioId)
        {
            try
            {
                DetalleIngreso _detalleingreso = context.DetalleIngreso.FirstOrDefault(x => x.DetalleIngresoId == DetalleIngresoId);
                _detalleingreso.Estado = ConstantHelpers.EstadoInactivo;
                context.Entry(_detalleingreso).State = System.Data.Entity.EntityState.Modified;
                context.DetalleIngreso.Remove(_detalleingreso);
                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch { }
            return RedirectToAction("LstDetalleIngresoAdmin", new { IngresoId = IngresoId, EdificioId = EdificioId });
        }
        #endregion

        #region archivos ingreso

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-file")]
        public ActionResult LstArchivoIngresoAdmin(Int32 IngresoId, Int32 EdificioId, Int32? np)
        {
            LstArchivoIngresoViewModel ViewModel = new LstArchivoIngresoViewModel();
            ViewModel.IngresoId = IngresoId;
            ViewModel.EdificioId = EdificioId;
            ViewModel.Fill(CargarDatosContext(), np);
            return View(ViewModel);
        }

        [AppAuthorize(AppRol.Propietario)]
        [ViewParameter("Edificio", "fa fa-file")]
        public ActionResult LstArchivoIngreso(Int32 IngresoId, Int32? np)
        {
            LstArchivoIngresoViewModel ViewModel = new LstArchivoIngresoViewModel();
            ViewModel.IngresoId = IngresoId;
            ViewModel.EdificioId = SessionHelpers.GetEdificioId(Session);
            ViewModel.Fill(CargarDatosContext(), np);
            return View(ViewModel);
        }

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-file")]
        public ActionResult AddEditArchivoIngreso(Int32? ArchivoIngresoId, Int32 IngresoId, Int32 EdificioId)
        {
            AddEditArchivoIngresoViewModel ViewModel = new AddEditArchivoIngresoViewModel();
            ViewModel.ArchivoIngresoId = ArchivoIngresoId;
            ViewModel.IngresoId = IngresoId;
            ViewModel.EdificioId = EdificioId;
            ViewModel.Fill(CargarDatosContext());
            return View(ViewModel);
        }

        [HttpPost]
        public ActionResult AddEditArchivoIngreso(AddEditArchivoIngresoViewModel ViewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewModel.Fill(CargarDatosContext());
                TryUpdateModel(ViewModel);
                return View(ViewModel);
            }
            try
            {
                using (var transacionscope = new TransactionScope())
                {
                    Ingreso ingreso = context.Ingreso.FirstOrDefault(x => x.IngresoId == ViewModel.IngresoId);
                    string _rutaarchivoserv = Server.MapPath("~");
                    Edificio edificio = context.Edificio.FirstOrDefault(x => x.EdificioId == ViewModel.EdificioId);
                    if (ViewModel.ArchivoIngresoId.HasValue)
                    {
                        ArchivoIngreso _Archivo = context.ArchivoIngreso.FirstOrDefault(x => x.ArchivoIngresoId == ViewModel.ArchivoIngresoId.Value);
                        _Archivo.Nombre = ViewModel.Archivo.FileName; //ViewModel.Nombre;

                        string _rutaarchivodir = _rutaarchivoserv + Path.Combine("Resources/Files", edificio.Acronimo);
                        if (!System.IO.Directory.Exists(_rutaarchivodir))
                            Directory.CreateDirectory(_rutaarchivodir);
                        _rutaarchivodir = Path.Combine(_rutaarchivodir, ingreso.UnidadTiempo.Descripcion);
                        if (!System.IO.Directory.Exists(_rutaarchivodir))
                            Directory.CreateDirectory(_rutaarchivodir);

                        string _nombrearc = edificio.EdificioId + "_" + DateTime.Now.Ticks.ToString() + "_" + _Archivo.ArchivoIngresoId + Path.GetExtension(ViewModel.Archivo.FileName);
                        _rutaarchivodir = Path.Combine(_rutaarchivodir, _nombrearc);

                        _Archivo.Ruta = _nombrearc;
                        ViewModel.Archivo.SaveAs(_rutaarchivodir);
                        context.Entry(_Archivo).State = System.Data.Entity.EntityState.Modified;
                    }
                    else
                    {
                        ArchivoIngreso _Archivo = new ArchivoIngreso();
                        _Archivo.Nombre = ViewModel.Archivo.FileName; //ViewModel.Nombre;

                        string _rutaarchivodir = _rutaarchivoserv + Path.Combine("Resources/Files", edificio.Acronimo);
                        if (!System.IO.Directory.Exists(_rutaarchivodir))
                            Directory.CreateDirectory(_rutaarchivodir);
                        _rutaarchivodir = Path.Combine(_rutaarchivodir, ingreso.UnidadTiempo.Descripcion);
                        if (!System.IO.Directory.Exists(_rutaarchivodir))
                            Directory.CreateDirectory(_rutaarchivodir);

                        string _nombrearc = edificio.EdificioId + "_" + DateTime.Now.Ticks.ToString() + "_" + _Archivo.ArchivoIngresoId + Path.GetExtension(ViewModel.Archivo.FileName);
                        _rutaarchivodir = Path.Combine(_rutaarchivodir, _nombrearc);
                        _Archivo.Ruta = _nombrearc;
                        _Archivo.Estado = ConstantHelpers.EstadoActivo;
                        _Archivo.FechaRegistro = DateTime.Now;
                        _Archivo.IngresoId = ViewModel.IngresoId;
                        ViewModel.Archivo.SaveAs(_rutaarchivodir);
                        context.ArchivoIngreso.Add(_Archivo);
                    }
                    context.SaveChanges();
                    transacionscope.Complete();
                    PostMessage(MessageType.Success);
                }
            }
            catch { InvalidarContext(); PostMessage(MessageType.Error); }
            return RedirectToAction("LstArchivoIngresoAdmin", new { IngresoId = ViewModel.IngresoId, EdificioId = ViewModel.EdificioId });
        }

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter(PageIcon: "fa fa-trash-o")]
        public ActionResult _DeleteArchivoIngreso(Int32 ArchivoIngresoId, Int32 IngresoId, Int32 EdificioId)
        {
            ViewBag.ArchivoIngresoId = ArchivoIngresoId;
            ViewBag.IngresoId = IngresoId;
            ViewBag.EdificioId = EdificioId;
            return PartialView();
        }

        [HttpPost]
        public ActionResult DeleteArchivoIngreso(Int32 ArchivoIngresoId, Int32 IngresoId, Int32 EdificioId)
        {
            try
            {
                ArchivoIngreso _Archivo = context.ArchivoIngreso.FirstOrDefault(x => x.ArchivoIngresoId == ArchivoIngresoId);
                _Archivo.Estado = ConstantHelpers.EstadoInactivo;
                context.Entry(_Archivo).State = System.Data.Entity.EntityState.Modified;
                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch { }
            return RedirectToAction("LstArchivoIngresoAdmin", new { IngresoId = IngresoId, EdificioId = EdificioId });
        }

        [AppAuthorize(AppRol.Administrador, AppRol.Propietario)]
        public ActionResult DescargarArchivo(string ruta, string nombre, string acronimo, string unidadmedida)
        {
            if (unidadmedida == null)
                unidadmedida = "";
            var buffer = Path.Combine(Server.MapPath("~/Resources/Files"), acronimo, unidadmedida, ruta);
            return File(buffer, "application/octet-stream", nombre + "." + ruta.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries)[1]);
        }
        #endregion

    }
}
