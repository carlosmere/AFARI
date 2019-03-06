using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.Filters;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Employee;
using System.IO;
using System.Transactions;
using VEH.Intranet.Logic;
using System.Drawing.Printing;
using Spire.Xls;

namespace VEH.Intranet.Controllers
{
    [AppAuthorize(AppRol.Administrador)]
    public class EmployeeController : BaseController
    {
        //
        // GET: /Employee/
        
        #region trabajador

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Trabajador", "fa fa-briefcase")]
        public ActionResult LstTrabajadorAdmin(Int32? np, Int32? EdificioId)
        {
            LstTrabajadorViewModel ViewModel = new LstTrabajadorViewModel();
            ViewModel.EdificioId = EdificioId;
            ViewModel.Fill(CargarDatosContext(), np);
            return View(ViewModel);
        }

        [AppAuthorize(AppRol.Propietario)]
        [ViewParameter("Trabajador", "fa fa-briefcase")]
        public ActionResult LstTrabajador(Int32? np1,Int32? np2, Int32 pestania = 1)
        {
            LstTrabajadorArchivoViewModel ViewModel = new LstTrabajadorArchivoViewModel();
            ViewModel.fill(CargarDatosContext());
            Int32 EdificioId = SessionHelpers.GetEdificioId(Session);
            ViewModel.LstTrabajador = new LstTrabajadorViewModel();
            ViewModel.LstTrabajador.EdificioId = EdificioId;
            ViewModel.LstArchivo = new LstArchivoTrabajadorViewModel();
            ViewModel.LstArchivo.EdificioId = EdificioId;
            ViewModel.LstTrabajador.Fill(CargarDatosContext(), np1);
            ViewModel.LstArchivo.Fill(CargarDatosContext(), np2);
            ViewModel.Pestania = pestania;
            return View(ViewModel);
        }

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Trabajador", "fa fa-briefcase")]
        public ActionResult AddEditTrabajador(Int32? TrabajadorId)
        {
            AddEditTrabajadorViewModel ViewModel = new AddEditTrabajadorViewModel();
            ViewModel.TrabajadorId = TrabajadorId;
            ViewModel.Fill(CargarDatosContext());
            return View(ViewModel);
        }

        [HttpPost]
        public ActionResult AddEditTrabajador(AddEditTrabajadorViewModel ViewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewModel.Fill(CargarDatosContext());
                TryUpdateModel(ViewModel);
                PostMessage(MessageType.Error);
                return View(ViewModel);
            }

            if (ViewModel.FotoFile != null)
            {
                string extension = Path.GetExtension(ViewModel.FotoFile.FileName).ToLower();
                if (extension != ".jpg" && extension != ".jpeg")
                {
                    ViewModel.Fill(CargarDatosContext());
                    TryUpdateModel(ViewModel);
                    PostMessage(MessageType.Info, "Solo se aceptan los formatos de imagen .jpg y .jpeg");
                    return View(ViewModel);
                }
            }

          

            try
            {
                using (var transaction = new TransactionScope())
                {
                    if (ViewModel.TrabajadorId.HasValue)
                    {
                        Trabajador trabajador = context.Trabajador.FirstOrDefault(x => x.TrabajadorId == ViewModel.TrabajadorId.Value);
                        trabajador.Nombres = ViewModel.Nombres;
                        trabajador.Apellidos = ViewModel.Apellidos;
                        if (ViewModel.AFP != 0)
                            trabajador.AFPId = ViewModel.AFP;
                        else trabajador.AFPId = null;
                        trabajador.DNI = ViewModel.DNI;
                        trabajador.FechaNacimiento = ViewModel.FechaNacimiento.ToDateTime();

                        if (ViewModel.FotoFile != null)
                        {
                            string ruta = Path.Combine(Server.MapPath("~/Resources/Fotos"), Path.GetFileNameWithoutExtension(ViewModel.FotoFile.FileName) + Path.GetExtension(ViewModel.FotoFile.FileName));
                            trabajador.Foto = Path.GetFileName(ViewModel.FotoFile.FileName);
                            if (!System.IO.Directory.Exists(Path.Combine(Server.MapPath("~/Resources/Fotos"))))
                                Directory.CreateDirectory(Path.Combine(Server.MapPath("~/Resources/Fotos")));
                            ViewModel.FotoFile.SaveAs(ruta);

                            ruta = Path.Combine(Server.MapPath("~/Resources/Files"), Path.GetFileNameWithoutExtension(ViewModel.FotoFile.FileName) + Path.GetExtension(ViewModel.FotoFile.FileName));

                            ViewModel.FotoFile.SaveAs(ruta);
                        }

                        
                        if (ViewModel.AntecedenteFile != null)
                        {
                            string nombre =  Guid.NewGuid().ToString().Substring(0,6) + Path.GetExtension(ViewModel.AntecedenteFile.FileName);
                            string ruta = Path.Combine(Server.MapPath("~/Resources/Files"), nombre);
                            trabajador.AntecedentesPoliciales = nombre;
                            if (!System.IO.Directory.Exists(Path.Combine(Server.MapPath("~/Resources/Files"))))
                                Directory.CreateDirectory(Path.Combine(Server.MapPath("~/Resources/Files")));
                            ViewModel.AntecedenteFile.SaveAs(ruta);
                        }

                        if (ViewModel.PartidaFile != null)
                        {
                            string nombre = Guid.NewGuid().ToString().Substring(0, 6) + Path.GetExtension(ViewModel.PartidaFile.FileName);

                            string ruta = Path.Combine(Server.MapPath("~/Resources/Files"), nombre);
                            trabajador.PartidaNacimiento = nombre;
                            if (!System.IO.Directory.Exists(Path.Combine(Server.MapPath("~/Resources/Files"))))
                                Directory.CreateDirectory(Path.Combine(Server.MapPath("~/Resources/Files")));
                            ViewModel.PartidaFile.SaveAs(ruta);
                        }

                        trabajador.Cargo = ViewModel.Cargo;
                        trabajador.CUSSP ="";
                        trabajador.EdificioId = ViewModel.EdificioId;
                        if (ViewModel.Comision.HasValue)
                            trabajador.Comision = 0;
                        trabajador.FechaIngreso = ViewModel.FechaIngreso;
                        trabajador.Modalidad = "";
                        trabajador.SueldoBase = 0;
                        trabajador.MontoHoras25 = 0;
                        trabajador.MontoHoras35 =0;
                        trabajador.MontoFeriado = 0;
                        trabajador.AdelantoQuincena = 0;
                        trabajador.ComisionFlujo = "";
                
                        context.Entry(trabajador).State = System.Data.Entity.EntityState.Modified;
                    }
                    else
                    {
                        DetalleQuincena detalle = new DetalleQuincena();
                        detalle.BonoPorMovilidad = true;
                        detalle.Bonificacion = true;
                        detalle.Seguro = true;
                        detalle.TotalQuincena = true;
                        context.DetalleQuincena.Add(detalle);
                        context.SaveChanges();

                        DetalleMensualidad detalleMensualidad = new DetalleMensualidad();
                        detalleMensualidad.CTS = true;
                        detalleMensualidad.Essalud = true;
                        context.DetalleMensualidad.Add(detalleMensualidad);
                        context.SaveChanges();


                        Trabajador trabajador = new Trabajador();
                        trabajador.EdificioId = ViewModel.EdificioId;
                        trabajador.Estado = ConstantHelpers.EstadoActivo;
                        trabajador.Nombres = ViewModel.Nombres;
                        trabajador.Apellidos = ViewModel.Apellidos;
                        if (ViewModel.AFP != 0)
                            trabajador.AFPId = ViewModel.AFP;
                        else trabajador.AFPId = null;
                        trabajador.EdificioId = ViewModel.EdificioId;
                        trabajador.DNI = ViewModel.DNI;
                        trabajador.FechaNacimiento = ViewModel.FechaNacimiento.ToDateTime();

                        if (ViewModel.FotoFile != null)
                        {
                            string ruta = Path.Combine(Server.MapPath("~/Resources/Fotos"), Path.GetFileNameWithoutExtension(ViewModel.FotoFile.FileName) + Path.GetExtension(ViewModel.FotoFile.FileName));
                            trabajador.Foto = Path.GetFileName(ViewModel.FotoFile.FileName);
                            if (!System.IO.Directory.Exists(Path.Combine(Server.MapPath("~/Resources/Fotos"))))
                                Directory.CreateDirectory(Path.Combine(Server.MapPath("~/Resources/Fotos")));
                            ViewModel.FotoFile.SaveAs(ruta);
                        }

                        if (ViewModel.AntecedenteFile != null)
                        {
                            string ruta = Path.Combine(Server.MapPath("~/Resources/Files"), Path.GetFileNameWithoutExtension(ViewModel.AntecedenteFile.FileName) + Path.GetExtension(ViewModel.AntecedenteFile.FileName));
                            trabajador.AntecedentesPoliciales = Path.GetFileName(ViewModel.AntecedenteFile.FileName);
                            if (!System.IO.Directory.Exists(Path.Combine(Server.MapPath("~/Resources/Files"))))
                                Directory.CreateDirectory(Path.Combine(Server.MapPath("~/Resources/Files")));
                            ViewModel.AntecedenteFile.SaveAs(ruta);
                        }

                        if (ViewModel.PartidaFile != null)
                        {
                            string ruta = Path.Combine(Server.MapPath("~/Resources/Files"), Path.GetFileNameWithoutExtension(ViewModel.PartidaFile.FileName) + Path.GetExtension(ViewModel.PartidaFile.FileName));
                            trabajador.PartidaNacimiento = Path.GetFileName(ViewModel.PartidaFile.FileName);
                            if (!System.IO.Directory.Exists(Path.Combine(Server.MapPath("~/Resources/Files"))))
                                Directory.CreateDirectory(Path.Combine(Server.MapPath("~/Resources/Files")));
                            ViewModel.PartidaFile.SaveAs(ruta);
                        }

                        trabajador.Cargo = ViewModel.Cargo;
                        trabajador.CUSSP = ViewModel.CUSSP;
                        if (ViewModel.Comision.HasValue)
                            trabajador.Comision = ViewModel.Comision.ToDecimal();
                        trabajador.FechaIngreso = ViewModel.FechaIngreso;
                        trabajador.Modalidad = "REG";
                        trabajador.SueldoBase = 0;
                        trabajador.MontoHoras25 = ViewModel.MontoHoras25.ToDecimal();
                        trabajador.MontoHoras35 = ViewModel.MontoHoras35.ToDecimal();
                        trabajador.MontoFeriado = ViewModel.MontoFeriado.ToDecimal();
                        trabajador.AdelantoQuincena = null;
                        trabajador.ComisionFlujo = "REG";
                        trabajador.DetalleQuincenaId = null;
                        trabajador.DetalleMensualidadId = null;
                        context.Trabajador.Add(trabajador);
                    }
                    context.SaveChanges();
                    transaction.Complete();
                    PostMessage(MessageType.Success);
                }
            }
            catch
            {
                InvalidarContext();
                PostMessage(MessageType.Error);
            }
            return RedirectToAction("LstTrabajadorAdmin");
        }

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter(PageIcon: "fa fa-trash-o")]
        public ActionResult _DeleteTrabajador(Int32 TrabajadorId, Int32 EdificioId)
        {
            ViewBag.TrabajadorId = TrabajadorId;
            ViewBag.EdificioId = EdificioId;
            return PartialView();
        }

        [HttpPost]
        public ActionResult DeleteTrabajador(Int32 TrabajadorId, Int32 EdificioId)
        {
            try
            {
                Trabajador trabajador = context.Trabajador.FirstOrDefault(x => x.TrabajadorId == TrabajadorId);
                trabajador.Estado = ConstantHelpers.EstadoInactivo;
                context.Entry(trabajador).State = System.Data.Entity.EntityState.Modified;
                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch { PostMessage(MessageType.Error); }
            return RedirectToAction("LstTrabajadorAdmin");
        }

        #endregion

        #region archivo trabajador


        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-file")]
        public ActionResult LstArchivoTrabajador(Int32? np,Int32 EdificioId)
        {
            LstArchivoTrabajadorViewModel ViewModel = new LstArchivoTrabajadorViewModel();
            ViewModel.EdificioId = EdificioId;
            ViewModel.Fill(CargarDatosContext(), np);
            return View(ViewModel);
        }

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-file")]
        public ActionResult AddEditArchivoTrabajador(Int32? ArchivoTrabajadorId, Int32 EdificioId)
        {
            AddEditArchivoTrabajadorViewModel ViewModel = new AddEditArchivoTrabajadorViewModel();
            ViewModel.ArchivoTrabajadorId = ArchivoTrabajadorId;
            ViewModel.EdificioId = EdificioId;
            ViewModel.FillComboUnidadTiempo(CargarDatosContext());
            ViewModel.Fill(CargarDatosContext());
            return View(ViewModel);
        }

        [HttpPost]
        public ActionResult AddEditArchivoTrabajador(AddEditArchivoTrabajadorViewModel ViewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewModel.FillComboUnidadTiempo(CargarDatosContext());
                ViewModel.Fill(CargarDatosContext());
                TryUpdateModel(ViewModel);
                return View(ViewModel);
            }
            try
            {
                using (var transacionscope = new TransactionScope())
                {
                    string _rutaarchivoserv = Server.MapPath("~");
                    Edificio edificio = context.Edificio.FirstOrDefault(x => x.EdificioId == ViewModel.EdificioId);
                    ViewModel.DescripcionUnidadTiempo = context.UnidadTiempo.FirstOrDefault(x => x.UnidadTiempoId == ViewModel.UnidadTiempoId).Descripcion.ToUpper();

                    if (ViewModel.ArchivoTrabajadorId.HasValue)
                    {
                        ArchivoTrabajador _Archivo = context.ArchivoTrabajador.FirstOrDefault(x => x.ArchivoTrabajadorId == ViewModel.ArchivoTrabajadorId.Value);
                        _Archivo.Nombre = ViewModel.Nombre;
                        _Archivo.UnidadTiempoId = ViewModel.UnidadTiempoId;

                        string _rutaarchivodir = _rutaarchivoserv + Path.Combine("Resources/Files", edificio.Acronimo);
                        if (!System.IO.Directory.Exists(_rutaarchivodir))
                            Directory.CreateDirectory(_rutaarchivodir);
                        _rutaarchivodir = Path.Combine(_rutaarchivodir, ViewModel.DescripcionUnidadTiempo);
                        if (!System.IO.Directory.Exists(_rutaarchivodir))
                            Directory.CreateDirectory(_rutaarchivodir);

                        string _nombrearc = edificio.EdificioId + "_" + DateTime.Now.Ticks.ToString() + Path.GetExtension(ViewModel.Archivo.FileName);
                        _rutaarchivodir = Path.Combine(_rutaarchivodir, _nombrearc);

                        _Archivo.Ruta = _nombrearc;
                        ViewModel.Archivo.SaveAs(_rutaarchivodir);
                        context.Entry(_Archivo).State = System.Data.Entity.EntityState.Modified;
                    }
                    else
                    {
                        ArchivoTrabajador _Archivo = new ArchivoTrabajador();
                        _Archivo.Nombre = ViewModel.Nombre;

                        string _rutaarchivodir = _rutaarchivoserv + Path.Combine("Resources/Files", edificio.Acronimo);
                        if (!System.IO.Directory.Exists(_rutaarchivodir))
                            Directory.CreateDirectory(_rutaarchivodir);
                        _rutaarchivodir = Path.Combine(_rutaarchivodir, ViewModel.DescripcionUnidadTiempo);
                        if (!System.IO.Directory.Exists(_rutaarchivodir))
                            Directory.CreateDirectory(_rutaarchivodir);

                        string _nombrearc = edificio.EdificioId + "_" + DateTime.Now.Ticks.ToString() + Path.GetExtension(ViewModel.Archivo.FileName);
                        _rutaarchivodir = Path.Combine(_rutaarchivodir, _nombrearc);
                        _Archivo.Ruta = _nombrearc;

                        _Archivo.UnidadTiempoId = ViewModel.UnidadTiempoId;
                        _Archivo.Estado = ConstantHelpers.EstadoActivo;
                        _Archivo.EdificioId = ViewModel.EdificioId;
                        _Archivo.FechaRegistro = DateTime.Now;
                        ViewModel.Archivo.SaveAs(_rutaarchivodir);
                        context.ArchivoTrabajador.Add(_Archivo);
                    }
                    context.SaveChanges();
                    transacionscope.Complete();
                    PostMessage(MessageType.Success);
                }
            }
            catch { InvalidarContext(); PostMessage(MessageType.Error); }
            return RedirectToAction("LstArchivoTrabajador", new { EdificioId = ViewModel.EdificioId });
        }

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter(PageIcon: "fa fa-trash-o")]
        public ActionResult _DeleteArchivoTrabajador(Int32 ArchivoTrabajadorId,Int32 EdificioId)
        {
            ViewBag.ArchivoTrabajadorId = ArchivoTrabajadorId;
            ViewBag.EdificioId = EdificioId;
            return PartialView();
        }

        [HttpPost]
        public ActionResult DeleteArchivoTrabajador(Int32 ArchivoTrabajadorId, Int32 EdificioId)
        {
            try
            {
                ArchivoTrabajador _Archivo = context.ArchivoTrabajador.FirstOrDefault(x => x.ArchivoTrabajadorId == ArchivoTrabajadorId);
                _Archivo.Estado = ConstantHelpers.EstadoInactivo;
                context.Entry(_Archivo).State = System.Data.Entity.EntityState.Modified;
                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch { }
            return RedirectToAction("LstArchivoTrabajador", new { EdificioId = EdificioId });
        }

        [AppAuthorize(AppRol.Administrador, AppRol.Propietario)]
        public ActionResult DescargarArchivo(string ruta, string nombre, string acronimo, string unidadtiempo)
        {
            var buffer = Path.Combine(Server.MapPath("~/Resources/Files"), acronimo, unidadtiempo, ruta);
            return File(buffer, "application/octet-stream", "Planilla " + nombre + " - " + unidadtiempo + "." + ruta.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries)[1]);
        }

        [AppAuthorize(AppRol.Administrador)]
        public ActionResult DescargarArchivoAdmin(string ruta, string nombre)
        {
            var buffer = Path.Combine(Server.MapPath("~/Resources/Files"),  ruta);
            return File(buffer, "application/octet-stream", nombre  + "." + ruta.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries)[1]);
        }
        #endregion

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-user")]
        public ActionResult EditPlanilla(Int32 EdificioId, Int32? UnidadTiempoId)
        {
            EditPlanillaViewModel model = new EditPlanillaViewModel();
            model.Fill(CargarDatosContext(), EdificioId, UnidadTiempoId);
            return View(model);
        }

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-user")]
        [HttpPost]
        public ActionResult EditPlanilla(EditPlanillaViewModel model, FormCollection formCollection)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.Fill(CargarDatosContext(), model.EdificioId, model.UnidadTiempoId);
                    TryUpdateModel(model);
                    PostMessage(MessageType.Error, i18n.ValidationStrings.DatosIncorrectos);
                    return View(model);
                }
                using (TransactionScope transaction = new TransactionScope())
                {
                    bool eraNull = false;
                    PlanillaR planillaR = context.PlanillaR.FirstOrDefault(x => x.EdificioId == model.EdificioId && x.UnidadTiempoId == model.UnidadTiempoId);
                    if (planillaR == null)
                    {
                        planillaR = new PlanillaR();
                        planillaR.UnidadTiempoId = model.UnidadTiempoId.Value;
                        planillaR.EdificioId = model.EdificioId;
                        eraNull = true;
                    }

                    if (model.Archivo != null && model.Archivo.ContentLength != 0)
                    {
                        Edificio objEdificio = context.Edificio.FirstOrDefault(x => x.EdificioId == model.EdificioId);
                        string _rutaarchivoserv = Server.MapPath("~");
                        string _rutaarchivodir = _rutaarchivoserv + Path.Combine("Resources/Files", objEdificio.Acronimo);
                        if (!System.IO.Directory.Exists(_rutaarchivodir))
                            Directory.CreateDirectory(_rutaarchivodir);
                        UnidadTiempo objUnidadTiempo = context.UnidadTiempo.FirstOrDefault(x => x.UnidadTiempoId == model.UnidadTiempoId);
                        _rutaarchivodir = Path.Combine(_rutaarchivodir, objUnidadTiempo.Descripcion);
                        if (!System.IO.Directory.Exists(_rutaarchivodir))
                            Directory.CreateDirectory(_rutaarchivodir);

                        string _nombrearcIni = model.EdificioId + "_" + DateTime.Now.Ticks.ToString() + "_";
                        string _nombrearc = _nombrearcIni + Path.GetExtension(model.Archivo.FileName);
                        //string _rutaPDF = _rutaarchivodir;
                        _rutaarchivodir = Path.Combine(_rutaarchivodir, _nombrearc);

                      //ACA guardar la ruta!!! PD: tambien guardar el PDF que se convirtio
                        //_editado.NormasConvivencia = _nombrearc;
                        model.Archivo.SaveAs(_rutaarchivodir);

                       // string _nombrearcParaPDF = _nombrearcIni + "PDF.pdf";

                        //Workbook excelGuardado = new Workbook();
                        //excelGuardado.LoadFromFile(_rutaarchivodir);
                        //_rutaPDF = Path.Combine(_rutaPDF, _nombrearcParaPDF);
                        //excelGuardado.SaveToFile(_rutaPDF, FileFormat.PDF);

                        
                        planillaR.RutaExcel = _nombrearc;
                       // planillaR.RutaPDF = _nombrearcParaPDF; //CAMBIAR AQUI POR EL CONVERTIDO!
                        //aca guardar el pdf
                        //var filename = Guid.NewGuid().ToString().Substring(0, 8) + "_" + Path.GetFileName(model.Archivo.FileName);

                        //var path = Path.Combine(Server.MapPath("~/Resources/Files/Normas"), filename);
                        //if (!System.IO.Directory.Exists(Path.Combine(Server.MapPath("~/Resources/Files/Normas"))))
                        //    Directory.CreateDirectory(Path.Combine(Server.MapPath("~/Resources/Files/Normas")));
                        //var file = System.IO.File.Create(path);

                        //_editado.NormasConvivencia= path;
                    }
                    if (model.ArchivoPublico != null && model.ArchivoPublico.ContentLength != 0)
                    {
                        Edificio objEdificio = context.Edificio.FirstOrDefault(x => x.EdificioId == model.EdificioId);
                        string _rutaarchivoserv = Server.MapPath("~");
                        string _rutaarchivodir = _rutaarchivoserv + Path.Combine("Resources/Files", objEdificio.Acronimo);
                        if (!System.IO.Directory.Exists(_rutaarchivodir))
                            Directory.CreateDirectory(_rutaarchivodir);
                        UnidadTiempo objUnidadTiempo = context.UnidadTiempo.FirstOrDefault(x => x.UnidadTiempoId == model.UnidadTiempoId);
                        _rutaarchivodir = Path.Combine(_rutaarchivodir, objUnidadTiempo.Descripcion);
                        if (!System.IO.Directory.Exists(_rutaarchivodir))
                            Directory.CreateDirectory(_rutaarchivodir);

                        string _nombrearcIni = model.EdificioId + "_" + DateTime.Now.Ticks.ToString() + "_";
                        string _nombrearc = _nombrearcIni + Path.GetExtension(model.ArchivoPublico.FileName);
                        //string _rutaPDF = _rutaarchivodir;
                        _rutaarchivodir = Path.Combine(_rutaarchivodir, _nombrearc);

                        model.ArchivoPublico.SaveAs(_rutaarchivodir);
                        planillaR.RutaPDF = _nombrearc; 
                    }
                    if(eraNull)
                    {
                        context.PlanillaR.Add(planillaR);
                    }


                    //Ya no va lo siguiente
                    //var LstTrabajadores = context.Trabajador.Where(x => x.EdificioId == model.EdificioId && x.Estado == ConstantHelpers.EstadoActivo).ToList();

                    //foreach (var trabajador in LstTrabajadores)
                    //{
                    //    var horasExtras25 = formCollection["planilla-horas-extras-25-" + trabajador.TrabajadorId];
                    //    var montoHorasExtras25 = formCollection["planilla-adicional-25-" + trabajador.TrabajadorId];
                    //    var horasExtras35 = formCollection["planilla-horas-extras-35-" + trabajador.TrabajadorId];
                    //    var montoHorasExtras35 = formCollection["planilla-adicional-35-" + trabajador.TrabajadorId];
                    //    var feriados = formCollection["planilla-feriados-" + trabajador.TrabajadorId];
                    //    var montoFeriados = formCollection["planilla-monto-feriados-" + trabajador.TrabajadorId];
                    //    var descuentoAusencia = formCollection["planilla-descuentos-" + trabajador.TrabajadorId];
                    //    var aumentoReemplazo = formCollection["planilla-reemplazo-" + trabajador.TrabajadorId];
                    //    var totalMes = formCollection["planilla-total-mes-" + trabajador.TrabajadorId];
                    //    var adelantoQuincena = formCollection["planilla-adelanto-quincena-" + trabajador.TrabajadorId];
                    //    var segundaQuincena = formCollection["planilla-segunda-quincena-" + trabajador.TrabajadorId];
                    //    var essalud = formCollection["planilla-essalud-" + trabajador.TrabajadorId];
                    //    Decimal totalDescuentos = 0, aporteObligatorio = 0, primaSeguro = 0, comisionAFP = 0;
                    //    if (trabajador.AFPId != null)
                    //    {
                    //        foreach (var item in context.ComisionAFP.Where(x => x.AFPId == trabajador.AFPId).ToList())
                    //        {
                    //            var desc = totalMes.ToDecimal() * item.Comision / 100;
                    //            totalDescuentos += desc;
                    //            if (item.TipoDescuento.Detalle.ToUpper().Contains("APORTE")) aporteObligatorio = desc;
                    //            if (item.TipoDescuento.Detalle.ToUpper().Contains("PRIMA")) primaSeguro = desc;
                    //            if (item.TipoDescuento.Detalle.ToUpper().Contains("COMISION") && item.TipoDescuento.Acronimo == trabajador.ComisionFlujo) comisionAFP = desc;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        var descuento = context.ComisionAFP.Where(x => x.TipoDescuento.Acronimo == "ONP").FirstOrDefault();
                    //        if (descuento != null)
                    //        {
                    //            totalDescuentos = totalMes.ToDecimal() * descuento.Comision / 100;
                    //        }
                    //    }

                    //    var sueldoNeto = totalMes.ToDecimal() - totalDescuentos;
                    //    var segundaQuincenaNeto = sueldoNeto - adelantoQuincena.ToDecimal();
                    //    var gratificacionesMes = totalMes.ToDecimal() * 109 / 600;
                    //    var CTSMes = 0;
                    //    var reemplazoVacaciones = 0;

                    //    Planilla planilla;
                    //    bool editar = true;
                    //    planilla = context.Planilla.Where(x => x.TrabajadorId == trabajador.TrabajadorId && x.UnidadTiempoId == model.UnidadTiempoId).FirstOrDefault();
                    //    if (planilla == null)
                    //    {
                    //        editar = false;
                    //        planilla = new Planilla();
                    //    }

                    //    planilla.TrabajadorId = trabajador.TrabajadorId;
                    //    planilla.UnidadTiempoId = model.UnidadTiempoId.Value;

                    //    planilla.HorasExtras = horasExtras25.ToDecimal() + horasExtras35.ToDecimal();
                    //    planilla.Feriado = feriados.ToDecimal();
                    //    planilla.AdelantoQuincena = adelantoQuincena.ToDecimal();
                    //    planilla.SegundaQuincena = segundaQuincena.ToDecimal();
                    //    planilla.ESSALUD = essalud.ToDecimal();
                    //    planilla.AporteObligatorio = aporteObligatorio;
                    //    planilla.PrimaSeguro = primaSeguro;
                    //    planilla.ComisionAFP = comisionAFP;
                    //    planilla.TotalDescuentos = totalDescuentos;
                    //    planilla.SueldoTotalNeto = sueldoNeto;
                    //    planilla.SegundaQuincenaNeto = segundaQuincenaNeto;
                    //    planilla.CTSMes = CTSMes;
                    //    planilla.ReemplazoVacaciones = reemplazoVacaciones;
                    //    planilla.HorasExtras25 = horasExtras25.ToDecimal();
                    //    planilla.HorasExtras35 = horasExtras35.ToDecimal();
                    //    planilla.MontoFeriados = montoFeriados.ToDecimal();
                    //    planilla.DescuenoAusencia = descuentoAusencia.ToDecimal();
                    //    planilla.AumentoReemplazo = aumentoReemplazo.ToDecimal();
                    //    planilla.TotalMes = totalMes.ToDecimal();
                    //    planilla.MontoHorasExtras25 = montoHorasExtras25.ToDecimal();
                    //    planilla.MontoHorasExtras35 = montoHorasExtras35.ToDecimal();
                    //    planilla.GratificacionesMes = gratificacionesMes.ToDecimal();

                    //    if (!editar) context.Planilla.Add(planilla);                        
                    //}

                    PostMessage(MessageType.Success, "Guardado Correctamente");
                    context.SaveChanges();
                    transaction.Complete();
                    return RedirectToAction("LstEdificio", "Building");
                }
            }
            catch (Exception ex)
            {
                model.Fill(CargarDatosContext(), model.EdificioId, model.UnidadTiempoId);
                TryUpdateModel(model);
                PostMessage(MessageType.Error, "Ocurrió un error, por favor inténtelo más tarde");
                return View(model);
            }
        }

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-user")]
        public ActionResult EditPlanillaQuincena(Int32 EdificioId, Int32? UnidadTiempoId)
        {
            EditPlanillaQuincenaViewModel model = new EditPlanillaQuincenaViewModel();
            model.Fill(CargarDatosContext(), EdificioId, UnidadTiempoId);
            return View(model);
        }
        
        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-user")]
        [HttpPost]
        public ActionResult EditPlanillaQuincena(EditPlanillaQuincenaViewModel model, FormCollection formCollection)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.Fill(CargarDatosContext(), model.EdificioId, model.UnidadTiempoId);
                    TryUpdateModel(model);
                    PostMessage(MessageType.Error, i18n.ValidationStrings.DatosIncorrectos);
                    return View(model);
                }
                using (TransactionScope transaction = new TransactionScope())
                {
                    var LstTrabajadores = context.Trabajador.Where(x => x.EdificioId == model.EdificioId && x.Estado == ConstantHelpers.EstadoActivo).ToList();

                    foreach (var trabajador in LstTrabajadores)
                    {
                        DetalleQuincena detalle = context.DetalleQuincena.Find(trabajador.DetalleQuincenaId);

                        var movilidad = formCollection["planilla-bonus-movilidad-" + trabajador.TrabajadorId];
                        var bonificacion = formCollection["planilla-bonificacion-" + trabajador.TrabajadorId];
                        var quincena = formCollection["planilla-total-quincena-" + trabajador.TrabajadorId];
                        var seguro = formCollection["planilla-seguro-" + trabajador.TrabajadorId];

                        bool esNuevo = false;
                        PlanillaQuincena planilla = context.PlanillaQuincena.Where(x => x.TrabajadorId == trabajador.TrabajadorId && x.UnidadTiempoId == model.UnidadTiempoId).FirstOrDefault();
                        if (planilla == null)
                        {
                            planilla = new PlanillaQuincena();
                            esNuevo = true;
                        }
                        planilla.TrabajadorId = trabajador.TrabajadorId;
                        planilla.UnidadTiempoId = model.UnidadTiempoId.Value;
                        if(detalle.BonoPorMovilidad)
                            planilla.BonoPorMovilidad = movilidad.ToDecimal();
                        if (detalle.Bonificacion)
                        planilla.Bonificacion = bonificacion.ToDecimal();
                        planilla.TotalQuincena = quincena.ToDecimal();
                        if (detalle.Seguro)
                            planilla.Seguro = seguro.ToDecimal();

                        if (esNuevo) context.PlanillaQuincena.Add(planilla);
                    }
                    PostMessage(MessageType.Success, "Guardado Correctamente");
                    context.SaveChanges();
                    transaction.Complete();
                    return RedirectToAction("LstEdificio", "Building");
                }
            }
            catch (Exception ex)
            {
                model.Fill(CargarDatosContext(), model.EdificioId, model.UnidadTiempoId);
                TryUpdateModel(model);
                PostMessage(MessageType.Error, "Ocurrió un error, por favor inténtelo más tarde");
                return View(model);
            }
            return View();
        }

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-user")]
        public ActionResult ExportPlanillaMensual(Int32 UnidadTiempoId, Int32 EdificioId)
        {
            try
            {
                ReporteLogic reportLogic = new ReporteLogic();
                reportLogic.Server = Server;
                reportLogic.context = context;

                List<Planilla> lista = context.Planilla.Where(x => x.UnidadTiempoId == UnidadTiempoId && x.Trabajador.EdificioId == EdificioId).ToList();
                UnidadTiempo unidadTiempo = context.UnidadTiempo.Find(UnidadTiempoId);
                Edificio edificio = context.Edificio.Find(EdificioId);
                String Titulo = "REPORTE MENSUAL - " + edificio.Nombre + " - " + unidadTiempo.Descripcion;

                MemoryStream outputMemoryStream = reportLogic.GetReportMensual(Titulo, lista);
                return File(outputMemoryStream, "application/vnd.ms-excel", Titulo + ".xls");
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error, "Ocurrió un error, por favor inténtelo más tarde");
                return RedirectToAction("EditPlanilla", new { EdificioId = EdificioId, UnidadTiempoId = UnidadTiempoId });
            }
        }

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-user")]
        public ActionResult ExportPlantillaQuincena(Int32 UnidadTiempoId, Int32 EdificioId)
        {
            try
            {
                ReporteLogic reportLogic = new ReporteLogic();
                reportLogic.Server = Server;
                reportLogic.context = context;

                List<PlanillaQuincena> lista = context.PlanillaQuincena.Where(x => x.UnidadTiempoId == UnidadTiempoId && x.Trabajador.EdificioId == EdificioId).ToList();
                UnidadTiempo unidadTiempo = context.UnidadTiempo.Find(UnidadTiempoId);
                Edificio edificio = context.Edificio.Find(EdificioId);
                String Titulo = "REPORTE QUINCENAL - " + edificio.Nombre + " - " + unidadTiempo.Descripcion;

                MemoryStream outputMemoryStream = reportLogic.GetReportQuincena(Titulo, unidadTiempo.Descripcion, lista);
                //return File(outputMemoryStream, "application/pdf", Titulo + ".pdf");
                return File(outputMemoryStream, "application/octet-stream", Titulo + ".zip");
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error, "Ocurrió un error, por favor inténtelo más tarde");
                return RedirectToAction("EditPlanillaQuincena", new { EdificioId = EdificioId, UnidadTiempoId = UnidadTiempoId });
            }
        }

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-user")]
        public ActionResult DetallePlanilla(Int32 PlanillaId, Int32 EdificioId)
        {
            DetallePlanillaViewModel model = new DetallePlanillaViewModel();
            model.Fill(CargarDatosContext(), PlanillaId, EdificioId);
            return View(model);
        }

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-user")]
        [HttpPost]
        public ActionResult DetallePlanilla(DetallePlanillaViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.Fill(CargarDatosContext(), model.PlanillaId, model.EdificioId);
                    TryUpdateModel(model);
                    PostMessage(MessageType.Error, i18n.ValidationStrings.DatosIncorrectos);
                    return View(model);
                }
                
                Planilla planilla = context.Planilla.Find(model.PlanillaId);
                planilla.TotalDescuentos = model.TotalDescuentos;
                planilla.SueldoTotalNeto = model.SueldoTotalNeto;
                planilla.SegundaQuincenaNeto = model.SegundaQuincenaNeto;
                planilla.GratificacionesMes = model.GratificacionesMes;
                if (planilla.Trabajador.DetalleMensualidad.CTS == false) planilla.CTSMes = 0;
                else planilla.CTSMes = model.CTSMes;
                planilla.ReemplazoVacaciones = model.ReemplazoVacaciones;
                context.SaveChanges();

                PostMessage(MessageType.Success, "Guardado Correctamente");
                return RedirectToAction("EditPlanilla", new { model.EdificioId, model.UnidadTiempoId });
            }
            catch (Exception ex)
            {
                model.Fill(CargarDatosContext(), model.PlanillaId, model.EdificioId);
                TryUpdateModel(model);
                PostMessage(MessageType.Error, i18n.ValidationStrings.DatosIncorrectos);
                return View(model);
            }
        }

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("AFP", "fa fa-money")]
        public ActionResult LstAFP()
        {
            LstAFPViewModel model = new LstAFPViewModel();
            model.Fill(CargarDatosContext());
            return View(model);
        }

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("AFP", "fa fa-money")]
        [HttpPost]
        public ActionResult LstAFP(LstAFPViewModel model, FormCollection form)
        {
            try
            {
                var LstAFP = context.AFP.OrderBy(x => x.Nombre).ToList();
                var LstTipoDescuento = context.TipoDescuento.OrderBy(x => x.Detalle).ToList();
                using (TransactionScope ts = new TransactionScope())
                {
                    foreach (var afp in LstAFP)
                    {
                        foreach (var descuento in LstTipoDescuento)
                        {
                            var comisionAFP = context.ComisionAFP.Where(x => x.AFPId == afp.AFPId && x.TipoDescuentoId == descuento.TipoDescuentoId).FirstOrDefault();
                            if (comisionAFP == null) continue;
                            var comision = form["comision-" + afp.AFPId + "-" + descuento.TipoDescuentoId];
                            comisionAFP.Comision = comision.ToDecimal();
                        }
                    }
                    context.SaveChanges();
                    ts.Complete();
                    PostMessage(MessageType.Success, "Datos Guardados Correctamente");
                    return RedirectToAction("LstAFP");
                }
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error, "Ocurrió un error. Por favor inténtelo más tarde");
                return RedirectToAction("LstAFP");
            }
        }

        public ActionResult _EditCamposQuincena(Int32 TrabajadorId)
        {
            EditCamposQuincenaViewModel model = new EditCamposQuincenaViewModel();
            model.Fill(CargarDatosContext(), TrabajadorId);
            return View("_EditCamposQuincena", model);
        }

        [HttpPost]
        public ActionResult EditCamposQuincena(EditCamposQuincenaViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    PostMessage(MessageType.Error, i18n.ValidationStrings.DatosIncorrectos);
                    return RedirectToAction("AddEditTrabajador", "Employee", new { TrabajadorId = model.TrabajadorId});
                }

                DetalleQuincena detalle = context.DetalleQuincena.Find(model.DetalleQuincenaId);
                detalle.BonoPorMovilidad = model.tieneMovilidad;
                detalle.Bonificacion = model.tieneBonificacion;
                detalle.Seguro = model.tieneSeguro;

                context.SaveChanges();
                PostMessage(MessageType.Success, "Datos guardados correctamente");
                return RedirectToAction("AddEditTrabajador", "Employee", new { TrabajadorId = model.TrabajadorId });
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error, "Ocurrió un error. Inténtelo más tarde");
                return RedirectToAction("AddEditTrabajador", "Employee", new { TrabajadorId = model.TrabajadorId });
            }
        }

        public ActionResult _EditCamposMensualidad(Int32 TrabajadorId)
        {
            EditCamposMensualidadViewModel model = new EditCamposMensualidadViewModel();
            model.Fill(CargarDatosContext(), TrabajadorId);
            return View("_EditCamposMensualidad", model);
        }

        [HttpPost]
        public ActionResult EditCamposMensualidad(EditCamposMensualidadViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    PostMessage(MessageType.Error, i18n.ValidationStrings.DatosIncorrectos);
                    return RedirectToAction("AddEditTrabajador", "Employee", new { TrabajadorId = model.TrabajadorId });
                }

                DetalleMensualidad detalle = context.DetalleMensualidad.Find(model.DetalleMensualidadId);
                detalle.Essalud = model.tieneEssalud;
                detalle.CTS = model.tieneCTS;

                context.SaveChanges();
                PostMessage(MessageType.Success, "Datos guardados correctamente");
                return RedirectToAction("AddEditTrabajador", "Employee", new { TrabajadorId = model.TrabajadorId });
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error, "Ocurrió un error. Inténtelo más tarde");
                return RedirectToAction("AddEditTrabajador", "Employee", new { TrabajadorId = model.TrabajadorId });
            }
        }

        
    }
}
