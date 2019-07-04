using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.Filters;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Building;
using System.Data;
using System.IO;
using Excel;
using VEH.Intranet.ViewModel.Employee;
using VEH.Intranet.Logic;
using static VEH.Intranet.ViewModel.Building.EnviarEmailInformativoViewModel;

namespace VEH.Intranet.Controllers
{
    [AppAuthorize(AppRol.Administrador)]
    public class BuildingController : BaseController
    {
        public ActionResult DownloadCronogramaMantenimiento(Int32 EdificioId, Int32? Anio)
        {
            try
            {
                if (!Anio.HasValue)
                {
                    Anio = DateTime.Now.Year;
                }

                ReporteLogic reporteLogic = new ReporteLogic();
                reporteLogic.Server = Server;
                reporteLogic.context = context;

                var edificioNombre = context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId).Nombre;
                var titulo = "Cronograma de Mantenimientos\n " + edificioNombre + "\nAño " + Anio.ToString();

                List<Cronograma> LstCronograma = context.Cronograma.Where(x => x.EdificioId == EdificioId
           && x.Estado == ConstantHelpers.EstadoActivo && x.Anio == Anio).OrderBy(x => x.Orden).ToList();             
                    
                MemoryStream outputMemoryStream = reporteLogic.GetReportMantenimiento(titulo, LstCronograma, edificioNombre + " " + Anio.ToString());
                if (outputMemoryStream == null)
                {
                    PostMessage(MessageType.Error, "Ocurrió un error. Por favor inténtelo más tarde");
                    if(Session.GetRol() == AppRol.Administrador)
                        return RedirectToAction("LstCronogramaMantenimiento", new { EdificioId = EdificioId, Anio = Anio });
                    else
                        return RedirectToAction("LstCronogramaMantenimientoProp", new { EdificioId = EdificioId, Anio = Anio });
                }

                return File(outputMemoryStream, "application/octet-stream", "Cronograma de Mantenimientos " + edificioNombre + " " + Anio + ".zip");
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error, "Ocurrió un error. Por favor inténtelo más tarde " + ex.Message + (ex.InnerException != null ? ex.InnerException.Message : string.Empty));
                if (Session.GetRol() == AppRol.Administrador)
                    return RedirectToAction("LstCronogramaMantenimiento", new { EdificioId = EdificioId, Anio = Anio });
                else
                    return RedirectToAction("LstCronogramaMantenimientoProp", new { EdificioId = EdificioId, Anio = Anio });
            }
        }
        public ActionResult _AddEditEdificio(Int32 EdificioId)
        {
            var model = new AddEditEdificioViewModel();
            model.EdificioId = EdificioId;
            model.Fill(CargarDatosContext());
            return View(model);
        }
        public ActionResult LstItemActa(Int32? Anio, Int32? EdificioId)
        {
            var model = new LstItemActaViewModel();
            model.Fill(CargarDatosContext(), Anio, EdificioId);
            return View(model);
        }
        [AppAuthorize(AppRol.Propietario)]
        public ActionResult LstItemActaPropietario(Int32? Anio, Int32? EdificioId)
        {
            var model = new LstItemActaViewModel();
            model.Fill(CargarDatosContext(), Anio, EdificioId);
            return View(model);
        }
        public ActionResult AddEditItemActa(Int32? ItemActaId, Int32 EdificioId)
        {
            var model = new AddEditItemActaViewModel();
            model.Fill(CargarDatosContext(), ItemActaId, EdificioId);
            return View(model);
        }
        [HttpPost]
        public ActionResult AddEditItemActa(AddEditItemActaViewModel model)
        {
            try
            {
                ItemActa item = null;
                if (model.ItemActaId.HasValue)
                {
                    item = context.ItemActa.FirstOrDefault(x => x.ItemActaId == model.ItemActaId);
                }
                else
                {
                    item = new ItemActa();
                    item.Estado = ConstantHelpers.EstadoActivo;
                    item.Fecha = DateTime.Now;
                    context.ItemActa.Add(item);
                }

                if (model.Ruta != null && model.Ruta.ContentLength != 0)
                {
                    string _rutaArchivoserv = Server.MapPath("~");
                    string _rutaArchivodir = _rutaArchivoserv + Path.Combine("/Resources/Files", "Actas");

                    string _nombrearc = model.Ruta.FileName;
                    _rutaArchivodir = Path.Combine(_rutaArchivodir, _nombrearc);

                    //_nuevo.NormasConvivencia = _nombrearc;
                    item.Ruta = _nombrearc;

                    model.Ruta.SaveAs(_rutaArchivodir);
                }

                item.Nombre = model.Nombre;
                item.EdificioId = model.EdificioId;
                item.UnidadTiempoId = model.UnidadTiempoId;
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error);
                model.Fill(CargarDatosContext(), model.ItemActaId, model.EdificioId);
                return View(model);
            }
            PostMessage(MessageType.Success);
            return RedirectToAction("LstItemActa");
        }
        public ActionResult DeleteItemActa(Int32 ItemActaId, Int32 EdificioId)
        {
            var item = context.ItemActa.FirstOrDefault(x => x.ItemActaId == ItemActaId);
            item.Estado = ConstantHelpers.EstadoEliminado;
            context.SaveChanges();
            return RedirectToAction("LstItemActa", new { EdificioId = EdificioId });
        }


        [ViewParameter("TipoInmueble", "fa fa-tag")]
        public ActionResult LstTipoInmuebles()
        {
            var ViewModel = new LstTipoInmueblesViewModel();
            ViewModel.Fill(CargarDatosContext());
            return View(ViewModel);
        }
        [ViewParameter("TipoInmueble", "fa fa-tag")]
        public ActionResult EliminarTipoInmueble(Int32 TipoInmuebleId)
        {
            try
            {
                var tipo = context.TipoInmueble.FirstOrDefault(x => x.TipoInmuebleId == TipoInmuebleId);
                tipo.Estado = ConstantHelpers.EstadoInactivo;
                context.SaveChanges();

                PostMessage(MessageType.Success);
                return RedirectToAction("LstTipoInmuebles", "Building");
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error, "Ocurrió un error. Por favor inténtelo más tarde");
                return RedirectToAction("LstTipoInmuebles", "Building");
            }
        }
        [ViewParameter("TipoInmueble", "fa fa-tag")]
        public ActionResult AddEditTipoInmueble(Int32? TipoInmuebleId)
        {
            var ViewModel = new AddEditTipoInmuebleViewModel();
            ViewModel.Fill(CargarDatosContext(), TipoInmuebleId);
            return View(ViewModel);
        }
        [HttpPost]
        [ViewParameter("TipoInmueble", "fa fa-tag")]
        public ActionResult AddEditTipoInmueble(AddEditTipoInmuebleViewModel model)
        {
            try
            {
                TipoInmueble tipo = null;
                if (model.TipoInmuebleId.HasValue)
                {
                    tipo = context.TipoInmueble.FirstOrDefault(x => x.TipoInmuebleId == model.TipoInmuebleId);
                }
                else
                {
                    tipo = new TipoInmueble();
                    tipo.Estado = ConstantHelpers.EstadoActivo;
                    context.TipoInmueble.Add(tipo);
                }

                tipo.Nombre = model.Nombre;
                tipo.Acronimo = model.Acronimo;

                context.SaveChanges();
                PostMessage(MessageType.Success);
                return RedirectToAction("LstTipoInmuebles", "Building");
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error, "Ocurrió un error. Por favor inténtelo más tarde");
                return RedirectToAction("LstTipoInmuebles", "Building");
            }
        }

        [ViewParameter("Inspeccion", "fa fa-book")]
        public ActionResult LstInspecciones(Int32? EdificioId)
        {
            LstInspeccionesViewModel ViewModel = new LstInspeccionesViewModel();
            ViewModel.EdificioId = EdificioId.Value;
            ViewModel.Fill(CargarDatosContext(), EdificioId.Value);
            return View(ViewModel);
        }
        [HttpPost]
        public ActionResult LstInspecciones(LstInspeccionesViewModel model, FormCollection frm)
        {
            if (!ModelState.IsValid)
            {
                model.Fill(CargarDatosContext(), model.EdificioId);
                TryUpdateModel(model);
                return View(model);
            }
            try
            {
                ReporteLogic reporteLogic = new ReporteLogic();
                reporteLogic.Server = Server;
                reporteLogic.context = context;

                var edificio = context.Edificio.FirstOrDefault(x => x.EdificioId == model.EdificioId);

                List<Inspeccion> LstInspecciones;
                if (model.FechaInicio.Equals(DateTime.MinValue))
                    LstInspecciones = context.Inspeccion.Where(x => x.EdificioId == model.EdificioId).ToList();
                else
                    LstInspecciones = context.Inspeccion.Where(x => x.EdificioId == model.EdificioId && model.FechaInicio <= x.Fecha && x.Fecha <= model.FechaFin).ToList();
                //Llenamos la estadistica de las preguntas por dia, remplazando la ultima hora del dia, si hubieran mas de una por dia
                //Es decir, puedes corregir algun mal envio de reporte durante el edia, se registraran todas, pero solo se tomara la ultima para el reporte
                Dictionary<int, Dictionary<DateTime, int>> LstHistorialPreguntas = new Dictionary<int, Dictionary<DateTime, int>>(); for (int i = 1; i <= 36; i++) LstHistorialPreguntas[i] = new Dictionary<DateTime, int>();
                foreach (var x in LstInspecciones)
                {
                    DateTime key = LstHistorialPreguntas[1].Keys.FirstOrDefault(y => y.Date.CompareTo(x.Fecha.Date) == 0);

                    if (key != DateTime.MinValue) //mismo dia, remplazamos todo-tomamos el ultimo
                    {
                        //  var Inspeccion = context.Inspeccion.FirstOrDefault( z => (z.Fecha.Date.CompareTo(x.Fecha.Date) == 0));
                        LstHistorialPreguntas[1][key] = x.Pregunta1;
                        LstHistorialPreguntas[2][key] = x.Pregunta2;
                        LstHistorialPreguntas[3][key] = x.Pregunta3;
                        LstHistorialPreguntas[4][key] = x.Pregunta4;
                        LstHistorialPreguntas[5][key] = x.Pregunta5;
                        LstHistorialPreguntas[6][key] = x.Pregunta6;
                        LstHistorialPreguntas[7][key] = x.Pregunta7;
                        LstHistorialPreguntas[8][key] = x.Pregunta8;
                        LstHistorialPreguntas[9][key] = x.Pregunta9;
                        LstHistorialPreguntas[10][key] = x.Pregunta10;
                        LstHistorialPreguntas[11][key] = x.Pregunta11;
                        LstHistorialPreguntas[12][key] = x.Pregunta12;
                        LstHistorialPreguntas[13][key] = x.Pregunta13;
                        LstHistorialPreguntas[14][key] = x.Pregunta14;
                        LstHistorialPreguntas[15][key] = x.Pregunta15;
                        LstHistorialPreguntas[16][key] = x.Pregunta16;
                        LstHistorialPreguntas[17][key] = x.Pregunta17;
                        LstHistorialPreguntas[18][key] = x.Pregunta18;
                        LstHistorialPreguntas[19][key] = x.Pregunta19;
                        LstHistorialPreguntas[20][key] = x.Pregunta20;
                        LstHistorialPreguntas[21][key] = x.Pregunta21;
                        LstHistorialPreguntas[22][key] = x.Pregunta22;
                        LstHistorialPreguntas[23][key] = x.Pregunta23;
                        LstHistorialPreguntas[24][key] = x.Pregunta24;
                        LstHistorialPreguntas[25][key] = x.Pregunta25;
                        LstHistorialPreguntas[26][key] = x.Pregunta26;
                        LstHistorialPreguntas[27][key] = x.Pregunta27;
                        LstHistorialPreguntas[28][key] = x.Pregunta28;
                        LstHistorialPreguntas[29][key] = x.Pregunta29;
                        LstHistorialPreguntas[30][key] = x.Pregunta30;
                        LstHistorialPreguntas[31][key] = x.Pregunta31;
                        LstHistorialPreguntas[32][key] = x.Pregunta32;
                        LstHistorialPreguntas[33][key] = x.Pregunta33;
                        LstHistorialPreguntas[34][key] = x.Pregunta34;
                        LstHistorialPreguntas[35][key] = x.Pregunta35;
                        LstHistorialPreguntas[36][key] = x.Pregunta36;
                    }
                    else
                    {
                        LstHistorialPreguntas[1].Add(x.Fecha, x.Pregunta1);
                        LstHistorialPreguntas[2].Add(x.Fecha, x.Pregunta2);
                        LstHistorialPreguntas[3].Add(x.Fecha, x.Pregunta3);
                        LstHistorialPreguntas[4].Add(x.Fecha, x.Pregunta4);
                        LstHistorialPreguntas[5].Add(x.Fecha, x.Pregunta5);
                        LstHistorialPreguntas[6].Add(x.Fecha, x.Pregunta6);
                        LstHistorialPreguntas[7].Add(x.Fecha, x.Pregunta7);
                        LstHistorialPreguntas[8].Add(x.Fecha, x.Pregunta8);
                        LstHistorialPreguntas[9].Add(x.Fecha, x.Pregunta9);
                        LstHistorialPreguntas[10].Add(x.Fecha, x.Pregunta10);
                        LstHistorialPreguntas[11].Add(x.Fecha, x.Pregunta11);
                        LstHistorialPreguntas[12].Add(x.Fecha, x.Pregunta12);
                        LstHistorialPreguntas[13].Add(x.Fecha, x.Pregunta13);
                        LstHistorialPreguntas[14].Add(x.Fecha, x.Pregunta14);
                        LstHistorialPreguntas[15].Add(x.Fecha, x.Pregunta15);
                        LstHistorialPreguntas[16].Add(x.Fecha, x.Pregunta16);
                        LstHistorialPreguntas[17].Add(x.Fecha, x.Pregunta17);
                        LstHistorialPreguntas[18].Add(x.Fecha, x.Pregunta18);
                        LstHistorialPreguntas[19].Add(x.Fecha, x.Pregunta19);
                        LstHistorialPreguntas[20].Add(x.Fecha, x.Pregunta20);
                        LstHistorialPreguntas[21].Add(x.Fecha, x.Pregunta21);
                        LstHistorialPreguntas[22].Add(x.Fecha, x.Pregunta22);
                        LstHistorialPreguntas[23].Add(x.Fecha, x.Pregunta23);
                        LstHistorialPreguntas[24].Add(x.Fecha, x.Pregunta24);
                        LstHistorialPreguntas[25].Add(x.Fecha, x.Pregunta25);
                        LstHistorialPreguntas[26].Add(x.Fecha, x.Pregunta26);
                        LstHistorialPreguntas[27].Add(x.Fecha, x.Pregunta27);
                        LstHistorialPreguntas[28].Add(x.Fecha, x.Pregunta28);
                        LstHistorialPreguntas[29].Add(x.Fecha, x.Pregunta29);
                        LstHistorialPreguntas[30].Add(x.Fecha, x.Pregunta30);
                        LstHistorialPreguntas[31].Add(x.Fecha, x.Pregunta31);
                        LstHistorialPreguntas[32].Add(x.Fecha, x.Pregunta32);
                        LstHistorialPreguntas[33].Add(x.Fecha, x.Pregunta33);
                        LstHistorialPreguntas[34].Add(x.Fecha, x.Pregunta34);
                        LstHistorialPreguntas[35].Add(x.Fecha, x.Pregunta35);
                        LstHistorialPreguntas[36].Add(x.Fecha, x.Pregunta36);
                    }
                }

                MemoryStream outputMemoryStream = reporteLogic.GetReportInspecciones("Reporte Inspecciones " + model.FechaInicio.ToShortDateString() + " - " + model.FechaFin.ToShortDateString(), LstHistorialPreguntas, model.EdificioId);


                if (outputMemoryStream == null)
                {
                    PostMessage(MessageType.Error, "Ocurrió un error. Por favor inténtelo más tarde");
                    return RedirectToAction("LstEdificio");
                }

                return File(outputMemoryStream, "application/octet-stream", "Reporte Inspecciones " + edificio.Nombre + ".zip");
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error, "Ocurrió un error. Por favor inténtelo más tarde");
                return RedirectToAction("LstEdificio");
            }
        }

        [AppAuthorize(AppRol.Propietario)]
        [ViewParameter("Edificio", "fa fa-list-ol")]
        public ActionResult Normas()
        {
            NormasViewModel model = new NormasViewModel();
            model.Fill(CargarDatosContext());
            return View(model);
        }
        [ViewParameter("Edificio", "fa fa-building-o")]
        public ActionResult LstOpcionEdificio(Int32 EdificioId)
        {
            LstOpcionEdificioViewModel ViewModel = new LstOpcionEdificioViewModel();
            ViewModel.Fill(CargarDatosContext(), EdificioId);
            return View(ViewModel);
        }

        [ViewParameter("Edificio", "fa fa-building-o")]
        public ActionResult LstEdificio(Int32? np, String Nombre)
        {
            LstEdificioViewModel ViewModel = new LstEdificioViewModel();
            ViewModel.Fill(CargarDatosContext(), np, Nombre);
            return View(ViewModel);
        }

        [ViewParameter("Edificio", "fa fa-building-o")]
        public ActionResult AddEditEdificio(Int32? EdificioId)
        {
            AddEditEdificioViewModel ViewModel = new AddEditEdificioViewModel();
            ViewModel.EdificioId = EdificioId;
            ViewModel.Fill(CargarDatosContext());
            ViewModel.LstComboDepartamento = new UbigeoHelper().ListaComboDepartamentos();
            return View(ViewModel);
        }
        [ViewParameter("Inspeccion", "fa fa-book")]
        public ActionResult AddEditInspeccion(Int32? InspeccionId)
        {
            AddEditInspeccionViewModel ViewModel = new AddEditInspeccionViewModel();
            ViewModel.InspeccionId = InspeccionId;
            ViewModel.Fill(CargarDatosContext());
            return View(ViewModel);
        }
        [HttpPost,
        ValidateInput(false)]
        public ActionResult AddEditInspeccion(AddEditInspeccionViewModel model, FormCollection frm)
        {
            if (!ModelState.IsValid)
            {
                model.Fill(CargarDatosContext());
                TryUpdateModel(model);
                return View(model);
            }
            try
            {
                using (var TransactionScope = new TransactionScope())
                {
                    if (!model.InspeccionId.HasValue)
                    {
                        Inspeccion _nuevo = new Inspeccion();
                        _nuevo.Nombre = model.Nombre;
                        _nuevo.Fecha = DateTime.Now;
                        _nuevo.Pregunta1 = model.Pregunta1;
                        _nuevo.Pregunta2 = model.Pregunta2;
                        _nuevo.Pregunta3 = model.Pregunta3;
                        _nuevo.Pregunta4 = model.Pregunta4;
                        _nuevo.Pregunta5 = model.Pregunta5;
                        _nuevo.Pregunta6 = model.Pregunta6;
                        _nuevo.Pregunta7 = model.Pregunta7;
                        _nuevo.Pregunta8 = model.Pregunta8;
                        _nuevo.Pregunta9 = model.Pregunta9;
                        _nuevo.Pregunta10 = model.Pregunta10;
                        _nuevo.Pregunta11 = model.Pregunta11;
                        _nuevo.Pregunta12 = model.Pregunta12;
                        _nuevo.Pregunta13 = model.Pregunta13;
                        _nuevo.Pregunta14 = model.Pregunta14;
                        _nuevo.Pregunta15 = model.Pregunta15;
                        _nuevo.Pregunta16 = model.Pregunta16;
                        _nuevo.Pregunta17 = model.Pregunta17;
                        _nuevo.Pregunta18 = model.Pregunta18;
                        _nuevo.Pregunta19 = model.Pregunta19;
                        _nuevo.Pregunta20 = model.Pregunta20;
                        _nuevo.Pregunta21 = model.Pregunta21;
                        _nuevo.Pregunta22 = model.Pregunta22;
                        _nuevo.Pregunta23 = model.Pregunta23;
                        _nuevo.Pregunta24 = model.Pregunta24;
                        _nuevo.Pregunta25 = model.Pregunta25;
                        _nuevo.Pregunta26 = model.Pregunta26;
                        _nuevo.Pregunta27 = model.Pregunta27;
                        _nuevo.Pregunta28 = model.Pregunta28;
                        _nuevo.Pregunta29 = model.Pregunta29;
                        _nuevo.Pregunta30 = model.Pregunta30;
                        _nuevo.Pregunta31 = model.Pregunta31;
                        _nuevo.Pregunta32 = model.Pregunta32;
                        _nuevo.Pregunta33 = model.Pregunta33;
                        _nuevo.Pregunta34 = model.Pregunta34;
                        _nuevo.Pregunta35 = model.Pregunta35;
                        _nuevo.Pregunta36 = model.Pregunta36;
                        _nuevo.Enunciado1 = model.Enunciado1;
                        _nuevo.Enunciado2 = model.Enunciado2;
                        _nuevo.Enunciado3 = model.Enunciado3;
                        _nuevo.Enunciado4 = model.Enunciado4;
                        _nuevo.Enunciado5 = model.Enunciado5;
                        _nuevo.Enunciado6 = model.Enunciado6;
                        _nuevo.Enunciado7 = model.Enunciado7;
                        _nuevo.Enunciado8 = model.Enunciado8;
                        _nuevo.Enunciado9 = model.Enunciado9;
                        _nuevo.Observaciones = model.Observaciones;
                    }
                    else
                    {
                        Inspeccion _editado = context.Inspeccion.FirstOrDefault(x => x.InspeccionId == model.InspeccionId.Value);
                        if (_editado != null)
                        {
                            _editado.Nombre = model.Nombre;
                            _editado.Fecha = DateTime.Now;
                            _editado.Pregunta1 = model.Pregunta1;
                            _editado.Pregunta2 = model.Pregunta2;
                            _editado.Pregunta3 = model.Pregunta3;
                            _editado.Pregunta4 = model.Pregunta4;
                            _editado.Pregunta5 = model.Pregunta5;
                            _editado.Pregunta6 = model.Pregunta6;
                            _editado.Pregunta7 = model.Pregunta7;
                            _editado.Pregunta8 = model.Pregunta8;
                            _editado.Pregunta9 = model.Pregunta9;
                            _editado.Pregunta10 = model.Pregunta10;
                            _editado.Pregunta11 = model.Pregunta11;
                            _editado.Pregunta12 = model.Pregunta12;
                            _editado.Pregunta13 = model.Pregunta13;
                            _editado.Pregunta14 = model.Pregunta14;
                            _editado.Pregunta15 = model.Pregunta15;
                            _editado.Pregunta16 = model.Pregunta16;
                            _editado.Pregunta17 = model.Pregunta17;
                            _editado.Pregunta18 = model.Pregunta18;
                            _editado.Pregunta19 = model.Pregunta19;
                            _editado.Pregunta20 = model.Pregunta20;
                            _editado.Pregunta21 = model.Pregunta21;
                            _editado.Pregunta22 = model.Pregunta22;
                            _editado.Pregunta23 = model.Pregunta23;
                            _editado.Pregunta24 = model.Pregunta24;
                            _editado.Pregunta25 = model.Pregunta25;
                            _editado.Pregunta26 = model.Pregunta26;
                            _editado.Pregunta27 = model.Pregunta27;
                            _editado.Pregunta28 = model.Pregunta28;
                            _editado.Pregunta29 = model.Pregunta29;
                            _editado.Pregunta30 = model.Pregunta30;
                            _editado.Pregunta31 = model.Pregunta31;
                            _editado.Pregunta32 = model.Pregunta32;
                            _editado.Pregunta33 = model.Pregunta33;
                            _editado.Pregunta34 = model.Pregunta34;
                            _editado.Pregunta35 = model.Pregunta35;
                            _editado.Pregunta36 = model.Pregunta36;
                            _editado.Enunciado1 = model.Enunciado1;
                            _editado.Enunciado2 = model.Enunciado2;
                            _editado.Enunciado3 = model.Enunciado3;
                            _editado.Enunciado4 = model.Enunciado4;
                            _editado.Enunciado5 = model.Enunciado5;
                            _editado.Enunciado6 = model.Enunciado6;
                            _editado.Enunciado7 = model.Enunciado7;
                            _editado.Enunciado8 = model.Enunciado8;
                            _editado.Enunciado9 = model.Enunciado9;
                            _editado.Observaciones = model.Observaciones;

                            //context.Entry(_editado).State = System.Data.Entity.EntityState.Modified;
                        }
                    }
                    context.SaveChanges();
                    TransactionScope.Complete();
                    PostMessage(MessageType.Success);
                }

            }
            catch { InvalidarContext(); PostMessage(MessageType.Error); }
            return RedirectToAction("LstEdificio");
        }

        [HttpPost,
        ValidateInput(false)]
        public ActionResult AddEditEdificio(AddEditEdificioViewModel model, FormCollection frm)
        {
            if (!ModelState.IsValid)
            {
                model.Fill(CargarDatosContext());
                TryUpdateModel(model);
                return View(model);
            }
            if (model.NroDepartamentos <= 0)
            {
                model.LstComboDepartamento = new UbigeoHelper().ListaComboDepartamentos();
                if (model.UProvinciaId.HasValue || model.UDepartamentoId.HasValue)
                    model.LstComboProvincia = new Helpers.UbigeoHelper().ListarComboProvincias(model.UDepartamentoId.Value);
                if (model.UDistritoId.HasValue || model.UProvinciaId.HasValue)
                    model.LstComboDistrito = new Helpers.UbigeoHelper().ListarComboDistritos(model.UProvinciaId.Value);
                model.Fill(CargarDatosContext());
                TryUpdateModel(model);
                PostMessage(MessageType.Info, "El número de departamentos debe ser mayor a 0");
                return View(model);
            }
            if (model.Archivo != null)
            {
                string extension = Path.GetExtension(model.Archivo.FileName).ToLower();
                //if (extension != ".doc" && extension != ".pdf")
                //{
                //    ViewModel.Fill(CargarDatosContext());
                //    TryUpdateModel(ViewModel);
                //    PostMessage(MessageType.Info, "Solo se aceptan los formatos .doc y .pdf");
                //    return View(ViewModel);
                //}
            }
            try
            {
                using (var TransactionScope = new TransactionScope())
                {
                    if (!model.EdificioId.HasValue)
                    {
                        Edificio _nuevo = new Edificio();
                        _nuevo.Acronimo = model.Acronimo;
                        _nuevo.Nombre = model.Nombre;
                        _nuevo.Referencia = model.Referencia;
                        _nuevo.Estado = ConstantHelpers.EstadoActivo;
                        _nuevo.Direccion = model.Direccion;
                        _nuevo.MontoCuota = model.MontoCuota.Value;
                        _nuevo.UDepartamentoId = model.UDepartamentoId;
                        _nuevo.UProvinciaId = model.UProvinciaId;
                        _nuevo.UDistritoId = model.UDistritoId;
                        _nuevo.NroDepartamentos = model.NroDepartamentos;
                        _nuevo.FactorAreaComun = model.FactorAreaComun;
                        _nuevo.FactorAlcantarillado = model.FactorAlcantarillado;
                        _nuevo.FactorCargoFijo = model.FactorCargoFijo;
                        _nuevo.Identificador = model.Identificador;
                        _nuevo.TipoMora = model.TipoMora;
                        _nuevo.Orden = model.Orden;
                        _nuevo.SaldoAnteriorUnidadTiempo = model.SaldoAnteriorUnidadTiempo;
                        if (model.TipoMora == "BRU")
                            _nuevo.PMora = model.PMora;
                        else
                            _nuevo.PMora = model.PMora / 100;
                        _nuevo.MensajeMora = model.MensajeMora;
                        _nuevo.CantidadReporte = 1;
                        _nuevo.NroCuenta = model.NroCuenta;
                        _nuevo.NormasConvivencia = model.Ruta;
                        _nuevo.SaldoAcumulado = 0M;
                        _nuevo.DiaMora = model.DiaMora;
                        _nuevo.SaldoAnteriorHistorico = model.SaldoHistorico;
                        _nuevo.EmailEncargado = model.EmailEncargado;
                        _nuevo.DiaEmisionCuota = model.DiaEmisionCuota;
                        _nuevo.DesfaseRecibos = model.Desfase;
                        _nuevo.TipoInmuebleId = model.TipoInmuebleId;
                        _nuevo.Representante = model.Representante;
                        _nuevo.NombreEncargado = String.IsNullOrEmpty(model.NombreEncargado) ? " " : model.NombreEncargado;
                        _nuevo.NombrePago = String.IsNullOrEmpty(model.NombrePago) ? " " : model.NombrePago;
                        _nuevo.PresupuestoMensual = model.PresupuestoMensual;

                        if (model.Archivo != null && model.Archivo.ContentLength != 0)
                        {
                            string _rutaArchivoserv = Server.MapPath("~");
                            string _rutaArchivodir = _rutaArchivoserv + Path.Combine("/Resources/Files", _nuevo.Acronimo);
                            if (!System.IO.Directory.Exists(_rutaArchivodir))
                                Directory.CreateDirectory(_rutaArchivodir);

                            string _nombrearc = _nuevo.EdificioId + "_" + DateTime.Now.Ticks.ToString() + "_" + Path.GetExtension(model.Archivo.FileName);
                            _rutaArchivodir = Path.Combine(_rutaArchivodir, _nombrearc);

                            _nuevo.NormasConvivencia = _nombrearc;
                            model.Archivo.SaveAs(_rutaArchivodir);

                        }

                        if (model.Firma != null && model.Firma.ContentLength != 0)
                        {
                            string _rutaFirmaserv = Server.MapPath("~");
                            string _rutaFirmadir = _rutaFirmaserv + Path.Combine("/Resources/Files", _nuevo.Acronimo);
                            if (!System.IO.Directory.Exists(_rutaFirmadir))
                                Directory.CreateDirectory(_rutaFirmadir);

                            string _nombrearc = _nuevo.EdificioId + "_" + DateTime.Now.Ticks.ToString() + "_" + Path.GetExtension(model.Firma.FileName);
                            _rutaFirmadir = Path.Combine(_rutaFirmadir, _nombrearc);

                            _nuevo.Firma = _nombrearc;
                            model.Firma.SaveAs(_rutaFirmadir);

                        }

                        for (int i = 1; i <= model.NroDepartamentos; i++)
                        {
                            Departamento _departamento = new Departamento();
                            _departamento.Numero = (100 + i).ToString();
                            _departamento.Piso = 1;
                            _departamento.Estado = ConstantHelpers.EstadoActivo;
                            _departamento.LecturaAgua = 0;
                            _nuevo.Departamento.Add(_departamento);
                        }
                        context.Edificio.Add(_nuevo);
                    }
                    else
                    {
                        Edificio _editado = context.Edificio.FirstOrDefault(x => x.EdificioId == model.EdificioId.Value);
                        Edificio objEdificio = context.Edificio.FirstOrDefault(x => x.Identificador == model.Identificador);
                        if (objEdificio != null && objEdificio.EdificioId != _editado.EdificioId)
                        {
                            model.Identificador = _editado.Identificador;
                            model.Fill(CargarDatosContext());
                            TryUpdateModel(model);
                            PostMessage(MessageType.Error, "El identificador es inválido o está repetido");
                            return View(model);
                        }
                        if (_editado != null)
                        {
                            _editado.Acronimo = model.Acronimo;
                            _editado.Nombre = model.Nombre;
                            _editado.DiaMora = model.DiaMora;
                            _editado.Orden = model.Orden;
                            _editado.Referencia = model.Referencia;
                            _editado.Direccion = model.Direccion;
                            _editado.SaldoAnteriorHistorico = model.SaldoHistorico;
                            _editado.MontoCuota = model.MontoCuota.Value;
                            _editado.UDepartamentoId = model.UDepartamentoId;
                            _editado.UProvinciaId = model.UProvinciaId;
                            _editado.UDistritoId = model.UDistritoId;
                            _editado.NroDepartamentos = model.NroDepartamentos;
                            _editado.FactorAreaComun = model.FactorAreaComun;
                            _editado.FactorAlcantarillado = model.FactorAlcantarillado;
                            _editado.FactorCargoFijo = model.FactorCargoFijo;
                            //_editado.PMora = model.PMora;
                            _editado.SaldoAnteriorUnidadTiempo = model.SaldoAnteriorUnidadTiempo;
                            if (model.TipoMora == "BRU")
                                _editado.PMora = model.PMora;
                            else
                                _editado.PMora = model.PMora / 100;

                            _editado.Identificador = model.Identificador;
                            _editado.NroCuenta = model.NroCuenta;
                            _editado.SaldoAcumulado = 0M;
                            _editado.TipoMora = model.TipoMora;
                            _editado.TipoInmuebleId = model.TipoInmuebleId;
                            _editado.MensajeMora = model.MensajeMora;
                            _editado.DiaEmisionCuota = model.DiaEmisionCuota;
                            _editado.DesfaseRecibos = model.Desfase;
                            _editado.EmailEncargado = model.EmailEncargado;
                            _editado.Representante = model.Representante;
                            _editado.NombreEncargado = String.IsNullOrEmpty(model.NombreEncargado) ? " " : model.NombreEncargado;
                            _editado.NombrePago = String.IsNullOrEmpty(model.NombrePago) ? " " : model.NombrePago;
                            _editado.PresupuestoMensual = model.PresupuestoMensual;

                            if (model.Archivo != null && model.Archivo.ContentLength != 0)
                            {
                                string _rutaarchivoserv = Server.MapPath("~");
                                string _rutaarchivodir = _rutaarchivoserv + Path.Combine("/Resources/Files", _editado.Acronimo);
                                if (!System.IO.Directory.Exists(_rutaarchivodir))
                                    Directory.CreateDirectory(_rutaarchivodir);

                                string _nombrearc = _editado.EdificioId + "_" + DateTime.Now.Ticks.ToString() + "_" + Path.GetExtension(model.Archivo.FileName);
                                _rutaarchivodir = Path.Combine(_rutaarchivodir, _nombrearc);

                                _editado.NormasConvivencia = _nombrearc;
                                model.Archivo.SaveAs(_rutaarchivodir);

                            }

                            if (model.Firma != null && model.Firma.ContentLength != 0)
                            {
                                string _rutaFirmaserv = Server.MapPath("~");
                                string _rutaFirmadir = _rutaFirmaserv + Path.Combine("/Resources/Files", _editado.Acronimo);
                                if (!System.IO.Directory.Exists(_rutaFirmadir))
                                    Directory.CreateDirectory(_rutaFirmadir);

                                string _nombrearc = _editado.EdificioId + "_" + DateTime.Now.Ticks.ToString() + "_" + Path.GetExtension(model.Firma.FileName);
                                _rutaFirmadir = Path.Combine(_rutaFirmadir, _nombrearc);

                                _editado.Firma = _nombrearc;
                                model.Firma.SaveAs(_rutaFirmadir);

                            }



                            context.Entry(_editado).State = System.Data.Entity.EntityState.Modified;
                        }
                    }
                    context.SaveChanges();
                    TransactionScope.Complete();
                    PostMessage(MessageType.Success);
                }

            }
            catch { InvalidarContext(); PostMessage(MessageType.Error); }
            return RedirectToAction("LstEdificio");
        }

        public JsonResult VerificarIdentificadorEdificio(string identificador)
        {
            return Json(new AddEditEdificioViewModel().VerificarIdentificadorEdificio(CargarDatosContext(), identificador), JsonRequestBehavior.AllowGet);
        }

        public ActionResult _ActDeactEdificio(Int32 EdificioId, string Estado)
        {
            ViewBag.EdificioId = EdificioId;
            ViewBag.Estado = Estado;
            return PartialView();
        }

        [HttpPost]
        public ActionResult ActDeactEdificio(Int32 EdificioId)
        {
            try
            {
                Edificio edificio = context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);
                if (edificio != null)
                {
                    edificio.Estado = edificio.Estado == ConstantHelpers.EstadoInactivo ? ConstantHelpers.EstadoActivo : ConstantHelpers.EstadoInactivo;
                    context.Entry(edificio).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                    PostMessage(MessageType.Success);
                }
            }
            catch { PostMessage(MessageType.Error); }
            return RedirectToAction("LstEdificio");
        }

        public JsonResult ListarComboProvincias(Int32 UDepartamentoId)
        {
            return Json(new UbigeoHelper().ListarComboProvincias(UDepartamentoId), JsonRequestBehavior.AllowGet);
        }

        public JsonResult ListarComboDistritos(Int32 UProvinciaId)
        {
            return Json(new UbigeoHelper().ListarComboDistritos(UProvinciaId), JsonRequestBehavior.AllowGet);
        }

        [AppAuthorize(AppRol.Administrador)]
        public ActionResult CargaMasiva()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CargaMasiva(CargaMasivaViewModel VM)
        {
            if (VM.Archivo != null)
            {
                try
                {

                    DataSet ds = new DataSet();
                    if (VM.Archivo != null && VM.Archivo.ContentLength > 0)
                    {
                        using (var transaction = new TransactionScope())
                        {

                            String fileExtension = Path.GetExtension(VM.Archivo.FileName);

                            IExcelDataReader excelReader = null;

                            if (fileExtension == ".xls")
                                excelReader = ExcelReaderFactory.CreateBinaryReader(VM.Archivo.InputStream);
                            else if (fileExtension == ".xlsx")
                                excelReader = ExcelReaderFactory.CreateOpenXmlReader(VM.Archivo.InputStream);

                            DataSet result = excelReader.AsDataSet();

                            for (int i = 0; i < result.Tables.Count; i++)
                            {
                                DataTable table = result.Tables[i];
                                Edificio edificio = new Edificio();
                                edificio.Acronimo = table.Rows[0][2].ToString();
                                edificio.Nombre = table.Rows[0][1].ToString().Trim();
                                edificio.Estado = ConstantHelpers.EstadoActivo;
                                edificio.NroDepartamentos = table.Rows[2][1].ToInteger();
                                edificio.UDistritoId = table.Rows[1][1].ToInteger();
                                edificio.UProvinciaId = table.Rows[1][2].ToInteger();
                                edificio.UDepartamentoId = table.Rows[1][3].ToInteger();
                                edificio.Referencia = string.Empty;
                                edificio.Direccion = string.Empty;

                                int filaini = 5;
                                int colini = 0;

                                while (filaini < table.Rows.Count && !string.IsNullOrEmpty(table.Rows[filaini][colini].ToString()))
                                {
                                    Propietario propietario = new Propietario();
                                    propietario.Nombres = table.Rows[filaini][colini + 1].ToString().Trim();
                                    propietario.ApellidoPaterno = table.Rows[filaini][colini + 2].ToString().Trim();
                                    propietario.ApellidoMaterno = table.Rows[filaini][colini + 3].ToString().Trim();
                                    propietario.TipoDocumento = "DNI";
                                    propietario.NroDocumento = "1";
                                    propietario.Estado = ConstantHelpers.EstadoActivo;

                                    Departamento departamento = new Departamento();
                                    departamento.Numero = table.Rows[filaini][colini].ToString();
                                    departamento.Piso = table.Rows[filaini][colini].ToInteger() / 100;
                                    departamento.LecturaAgua = 0;
                                    departamento.Estado = ConstantHelpers.EstadoActivo;
                                    departamento.Propietario.Add(propietario);
                                    edificio.Departamento.Add(departamento);
                                    filaini++;
                                }
                                context.Edificio.Add(edificio);
                            }
                            context.SaveChanges();
                            transaction.Complete();
                        }
                    }
                }
                catch { InvalidarContext(); }

            }
            return RedirectToAction("LstEdificio");
        }
        [AppAuthorize(AppRol.Administrador, AppRol.Propietario)]
        public ActionResult DescargarArchivo(string ruta, string nombre, string acronimo)
        {
            var buffer = Path.Combine(Server.MapPath("~/Resources/Files"), acronimo, ruta);
            return File(buffer, "application/octet-stream", nombre + "." + ruta.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries)[1]);
        }

        [AppAuthorize(AppRol.Administrador)]
        public ActionResult DownloadReporteInspeciones(Int32 edificioId, DateTime FechaInicio, DateTime FechaFin)
        {
            try
            {
                ReporteLogic reporteLogic = new ReporteLogic();
                reporteLogic.Server = Server;
                reporteLogic.context = context;

                var edificio = context.Edificio.FirstOrDefault(x => x.EdificioId == edificioId);

                //List<Inspeccion> LstInspecciones = context.Inspeccion.Where(x => x.EdificioId == edificioId).ToList();
                List<Inspeccion> LstInspecciones = context.Inspeccion.Where(x => x.EdificioId == edificioId && x.Fecha.IsBetween(FechaInicio, FechaFin)).OrderBy(X => X.Fecha).ToList();
                //Llenamos la estadistica de las preguntas por dia, remplazando la ultima hora del dia, si hubieran mas de una por dia
                //Es decir, puedes corregir algun mal envio de reporte durante el edia, se registraran todas, pero solo se tomara la ultima para el reporte
                Dictionary<int, Dictionary<DateTime, int>> LstHistorialPreguntas = new Dictionary<int, Dictionary<DateTime, int>>(); for (int i = 1; i <= 36; i++) LstHistorialPreguntas[i] = new Dictionary<DateTime, int>();
                foreach (var x in LstInspecciones)
                {
                    DateTime key = LstHistorialPreguntas[1].Keys.FirstOrDefault(y => y.Date.CompareTo(x.Fecha.Date) == 0);

                    if (key != DateTime.MinValue) //mismo dia, remplazamos todo-tomamos el ultimo
                    {
                        //  var Inspeccion = context.Inspeccion.FirstOrDefault( z => (z.Fecha.Date.CompareTo(x.Fecha.Date) == 0));
                        LstHistorialPreguntas[1][key] = x.Pregunta1;
                        LstHistorialPreguntas[2][key] = x.Pregunta2;
                        LstHistorialPreguntas[3][key] = x.Pregunta3;
                        LstHistorialPreguntas[4][key] = x.Pregunta4;
                        LstHistorialPreguntas[5][key] = x.Pregunta5;
                        LstHistorialPreguntas[6][key] = x.Pregunta6;
                        LstHistorialPreguntas[7][key] = x.Pregunta7;
                        LstHistorialPreguntas[8][key] = x.Pregunta8;
                        LstHistorialPreguntas[9][key] = x.Pregunta9;
                        LstHistorialPreguntas[10][key] = x.Pregunta10;
                        LstHistorialPreguntas[11][key] = x.Pregunta11;
                        LstHistorialPreguntas[12][key] = x.Pregunta12;
                        LstHistorialPreguntas[13][key] = x.Pregunta13;
                        LstHistorialPreguntas[14][key] = x.Pregunta14;
                        LstHistorialPreguntas[15][key] = x.Pregunta15;
                        LstHistorialPreguntas[16][key] = x.Pregunta16;
                        LstHistorialPreguntas[17][key] = x.Pregunta17;
                        LstHistorialPreguntas[18][key] = x.Pregunta18;
                        LstHistorialPreguntas[19][key] = x.Pregunta19;
                        LstHistorialPreguntas[20][key] = x.Pregunta20;
                        LstHistorialPreguntas[21][key] = x.Pregunta21;
                        LstHistorialPreguntas[22][key] = x.Pregunta22;
                        LstHistorialPreguntas[23][key] = x.Pregunta23;
                        LstHistorialPreguntas[24][key] = x.Pregunta24;
                        LstHistorialPreguntas[25][key] = x.Pregunta25;
                        LstHistorialPreguntas[26][key] = x.Pregunta26;
                        LstHistorialPreguntas[27][key] = x.Pregunta27;
                        LstHistorialPreguntas[28][key] = x.Pregunta28;
                        LstHistorialPreguntas[29][key] = x.Pregunta29;
                        LstHistorialPreguntas[30][key] = x.Pregunta30;
                        LstHistorialPreguntas[31][key] = x.Pregunta31;
                        LstHistorialPreguntas[32][key] = x.Pregunta32;
                        LstHistorialPreguntas[33][key] = x.Pregunta33;
                        LstHistorialPreguntas[34][key] = x.Pregunta34;
                        LstHistorialPreguntas[35][key] = x.Pregunta35;
                        LstHistorialPreguntas[36][key] = x.Pregunta36;
                    }
                    else
                    {
                        LstHistorialPreguntas[1].Add(x.Fecha, x.Pregunta1);
                        LstHistorialPreguntas[2].Add(x.Fecha, x.Pregunta2);
                        LstHistorialPreguntas[3].Add(x.Fecha, x.Pregunta3);
                        LstHistorialPreguntas[4].Add(x.Fecha, x.Pregunta4);
                        LstHistorialPreguntas[5].Add(x.Fecha, x.Pregunta5);
                        LstHistorialPreguntas[6].Add(x.Fecha, x.Pregunta6);
                        LstHistorialPreguntas[7].Add(x.Fecha, x.Pregunta7);
                        LstHistorialPreguntas[8].Add(x.Fecha, x.Pregunta8);
                        LstHistorialPreguntas[9].Add(x.Fecha, x.Pregunta9);
                        LstHistorialPreguntas[10].Add(x.Fecha, x.Pregunta10);
                        LstHistorialPreguntas[11].Add(x.Fecha, x.Pregunta11);
                        LstHistorialPreguntas[12].Add(x.Fecha, x.Pregunta12);
                        LstHistorialPreguntas[13].Add(x.Fecha, x.Pregunta13);
                        LstHistorialPreguntas[14].Add(x.Fecha, x.Pregunta14);
                        LstHistorialPreguntas[15].Add(x.Fecha, x.Pregunta15);
                        LstHistorialPreguntas[16].Add(x.Fecha, x.Pregunta16);
                        LstHistorialPreguntas[17].Add(x.Fecha, x.Pregunta17);
                        LstHistorialPreguntas[18].Add(x.Fecha, x.Pregunta18);
                        LstHistorialPreguntas[19].Add(x.Fecha, x.Pregunta19);
                        LstHistorialPreguntas[20].Add(x.Fecha, x.Pregunta20);
                        LstHistorialPreguntas[21].Add(x.Fecha, x.Pregunta21);
                        LstHistorialPreguntas[22].Add(x.Fecha, x.Pregunta22);
                        LstHistorialPreguntas[23].Add(x.Fecha, x.Pregunta23);
                        LstHistorialPreguntas[24].Add(x.Fecha, x.Pregunta24);
                        LstHistorialPreguntas[25].Add(x.Fecha, x.Pregunta25);
                        LstHistorialPreguntas[26].Add(x.Fecha, x.Pregunta26);
                        LstHistorialPreguntas[27].Add(x.Fecha, x.Pregunta27);
                        LstHistorialPreguntas[28].Add(x.Fecha, x.Pregunta28);
                        LstHistorialPreguntas[29].Add(x.Fecha, x.Pregunta29);
                        LstHistorialPreguntas[30].Add(x.Fecha, x.Pregunta30);
                        LstHistorialPreguntas[31].Add(x.Fecha, x.Pregunta31);
                        LstHistorialPreguntas[32].Add(x.Fecha, x.Pregunta32);
                        LstHistorialPreguntas[33].Add(x.Fecha, x.Pregunta33);
                        LstHistorialPreguntas[34].Add(x.Fecha, x.Pregunta34);
                        LstHistorialPreguntas[35].Add(x.Fecha, x.Pregunta35);
                        LstHistorialPreguntas[36].Add(x.Fecha, x.Pregunta36);
                    }
                }

                MemoryStream outputMemoryStream = reporteLogic.GetReportInspecciones("Reporte Inspecciones " + FechaInicio.ToShortDateString() + " - " + FechaFin.ToShortDateString(), LstHistorialPreguntas, edificioId);


                if (outputMemoryStream == null)
                {
                    PostMessage(MessageType.Error, "Ocurrió un error. Por favor inténtelo más tarde");
                    return RedirectToAction("LstEdificio");
                }

                return File(outputMemoryStream, "application/octet-stream", "Reporte Inspecciones " + edificio.Nombre + ".zip");
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error, "Ocurrió un error. Por favor inténtelo más tarde");
                return RedirectToAction("LstEdificio");
            }
        }

        [AppAuthorize(AppRol.Administrador, AppRol.Propietario)]
        public ActionResult DownloadReporteDepartamentoIngresosGastos(Int32 edificioId, Int32 unidadTiempoId, int mes, int dia, int annio)
        {
            try
            {
                //<Chequear si se ha subido correcion
                var unidadTiempo = context.UnidadTiempo.FirstOrDefault(X => X.UnidadTiempoId == unidadTiempoId);
                if (unidadTiempo != null)
                {
                    var correcion = context.ArchivoCorrecionEdificio.FirstOrDefault(X => X.Tipo.Contains(ConstantHelpers.TipoArchivo.BalanceGeneral) && X.EdificioId == edificioId && X.UnidadTiempoId == unidadTiempoId);
                    if (correcion != null)
                    {
                        byte[] fileBytes = System.IO.File.ReadAllBytes(Path.Combine(Server.MapPath("~/Resources/Files/Corregidos"), correcion.RutaArchivo));
                        string fileName = "Reporte Ingresos y Gastos - " + context.Edificio.FirstOrDefault(X => X.EdificioId == edificioId).Nombre + " - " + unidadTiempo.Descripcion + ".pdf";
                        return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                    }
                }
                //Chequear>



                DateTime fechaRegistro = new DateTime(annio, mes, dia);
                ReporteLogic reporteLogic = new ReporteLogic();
                reporteLogic.Server = Server;
                reporteLogic.context = context;

                var edificio = context.Edificio.FirstOrDefault(x => x.EdificioId == edificioId);
                //Lista de cuotas anterior
                //List<Cuota> ListCuotas = new List<Cuota>();
                //var departamentos = edificio.Departamento.ToList();

                //foreach (var depa in departamentos)
                //{
                //    Cuota cuota = context.Cuota.FirstOrDefault(x => x.DepartamentoId == depa.DepartamentoId && x.UnidadTiempoId == unidadTiempoId);// && x.Estado.Equals(ConstantHelpers.EstadoActivo));
                //    if (cuota == null) continue;
                //    ListCuotas.Add(cuota);
                //}

                //Listado de cuotas contando la fecha de pagado
                List<Cuota> ListCuotas = new List<Cuota>();
                List<Int32> LstDepartamentoAdelantado = new List<Int32>();
                var contextAux = new SIVEHEntities();
                //var departamentos = context.Departamento.Where(x => x.EdificioId == edificio.EdificioId && x.Estado == ConstantHelpers.EstadoActivo);
                var departamentos = context.Departamento.Where(x => x.EdificioId == edificio.EdificioId);
                foreach (var departamento in departamentos)
                {
                    var cuotas = contextAux.Cuota.Where(X => X.DepartamentoId == departamento.DepartamentoId && X.Pagado).OrderBy(x => x.DepartamentoId).ThenBy(x => x.CuotaId).ToList();
                    if (cuotas != null && cuotas.Count > 0)
                    {
                        if (cuotas.Count == 2 && cuotas.Count(x => x.EsExtraordinaria == true) >= 1)
                        {
                            var ext = cuotas.FirstOrDefault(x => x.EsExtraordinaria == true);
                            var ord = cuotas.FirstOrDefault(x => x.EsExtraordinaria == false);

                            cuotas[0] = ord;
                            cuotas[1] = ext;
                        }

                        foreach (var cuota in cuotas)
                        {
                            if (cuota.EsAdelantado.HasValue && (cuota.EsAdelantado.Value == true))//adelantado
                            {
                                //ListCuotas.Add(cuota);
                                LstDepartamentoAdelantado.Add(cuota.DepartamentoId);
                            }
                            //Si no existe la fecha de pagado, añadir si cumple con la unidad de tiempo
                            if (!cuota.FechaPagado.HasValue && cuota.UnidadTiempoId == unidadTiempoId)
                                ListCuotas.Add(cuota);
                            else
                            { //Si existe la fecha de pagado, comprar el mes y el año , si encajan con esta unidad de tiempo, entonces son parte del reporte
                                Int32? diaMoraCuota = cuota.Departamento.Edificio.DiaMora;
                                if (cuota.UnidadTiempo.Mes == 2)
                                {
                                    diaMoraCuota = 28;
                                }

                                diaMoraCuota = diaMoraCuota.HasValue ? diaMoraCuota.Value : cuota.UnidadTiempo.Mes == 2 ? 28 : 30;

                                var fechaVencimientoCuota = new DateTime();

                                try
                                {
                                    fechaVencimientoCuota = new DateTime(cuota.UnidadTiempo.Anio, cuota.UnidadTiempo.Mes, diaMoraCuota.Value);
                                }
                                catch
                                {
                                    fechaVencimientoCuota = new DateTime(cuota.UnidadTiempo.Anio, cuota.UnidadTiempo.Mes, diaMoraCuota.Value - 1);
                                }

                                if (cuota.FechaPagado.HasValue && (cuota.FechaPagado.Value.Month == unidadTiempo.Mes && cuota.FechaPagado.Value.Year == unidadTiempo.Anio))
                                {
                                    if (cuota.EsExtraordinaria.HasValue && cuota.EsExtraordinaria.Value)
                                    {
                                        var validacionExtra = ListCuotas.FirstOrDefault(x => x.DepartamentoId == cuota.DepartamentoId);
                                        if (validacionExtra != null && cuota.FechaPagado != null)
                                        {

                                            if (cuota.UnidadTiempo.Mes == validacionExtra.UnidadTiempo.Mes)
                                            {
                                                ListCuotas.Remove(validacionExtra);
                                                validacionExtra.CuotaExtraordinaria += cuota.CuotaExtraordinaria;
                                                validacionExtra.Total += cuota.CuotaExtraordinaria ?? 0;
                                                ListCuotas.Add(validacionExtra);
                                            }
                                            else if (cuota.UnidadTiempo.Mes != validacionExtra.UnidadTiempo.Mes)
                                            {
                                                //validacionExtra.Total += cuota.CuotaExtraordinaria ?? 0;
                                                ListCuotas.Add(cuota);
                                            }

                                        }
                                        else
                                        {
                                            ListCuotas.Add(cuota);
                                        }
                                    }
                                    else
                                    {
                                        ListCuotas.Add(cuota);
                                    }
                                }
                                
                            }
                        }
                    }                    
                }

               
                ListCuotas = ListCuotas.OrderBy(x => x.DepartamentoId).ToList();


                //var presupuestoMes = ListCuotas.Sum(x => x.Monto); //Total de 
                // var totalM2 = departamentos.Sum(x => x.DepartamentoM2 ?? 0) + departamentos.Sum(x => x.EstacionamientoM2 ?? 0) + departamentos.Sum(x => x.EstacionamientoM2 ?? 0);
                unidadTiempo = context.UnidadTiempo.FirstOrDefault(x => x.UnidadTiempoId == unidadTiempoId);
                var Gasto = context.Gasto.FirstOrDefault(x => x.EdificioId == edificioId && x.UnidadTiempoId == unidadTiempoId && x.Estado.Equals(ConstantHelpers.EstadoActivo));
                var IngresoComun = context.Ingreso.FirstOrDefault(x => x.EdificioId == edificioId && x.UnidadTiempoId == unidadTiempoId && x.Estado.Equals(ConstantHelpers.EstadoActivo));
                var cantOrden = context.DetalleGasto.Where(x => x.GastoId == Gasto.GastoId && x.Estado.Equals(ConstantHelpers.EstadoActivo)).Count(x => x.Orden.HasValue == false);
                List<DetalleGasto> ListGastos = context.DetalleGasto.Where(x => x.GastoId == Gasto.GastoId && x.Estado.Equals(ConstantHelpers.EstadoActivo)).ToList();

                if (cantOrden == 0)
                {
                    ListGastos = ListGastos.OrderBy(x => x.Orden).ToList();
                }
                //  Cuota c = listaCuota.Where(x => x.DepartamentoId == departamentoId).FirstOrDefault();
                // bool exportadoAntes = false;
                Decimal saldoAnterior = 0M;


                List<DetalleIngreso> ListIngresosComunes = new List<DetalleIngreso>();
                if (IngresoComun != null)
                    ListIngresosComunes = context.DetalleIngreso.Where(X => X.IngresoId == IngresoComun.IngresoId && X.Estado.Equals(ConstantHelpers.EstadoActivo)).ToList();

                UnidadTiempo objUnidadTiempoAnterior = context.UnidadTiempo.FirstOrDefault(x => x.Orden == unidadTiempo.Orden - 1 && x.Estado.Equals(ConstantHelpers.EstadoActivo));

                var SaldoAnterior = 0;//reporteLogic.GetSaldoHasta(CargarDatosContext(), context.UnidadTiempo.First(X => X.UnidadTiempoId == unidadTiempoId), edificioId);


                List<Leyenda> LstLeyendas = context.Leyenda.Where(X => X.BalanceUnidadTiempoEdificio.EdificioId == edificioId && X.BalanceUnidadTiempoEdificio.UnidadDeTiempoId == unidadTiempoId).ToList();

                MemoryStream outputMemoryStream = null;

                if (Session.GetRol() == AppRol.Propietario)
                {
                    outputMemoryStream = reporteLogic.GetReportIngresosGastos("Ingresos y Gastos de " + unidadTiempo.Descripcion + " \n EDIFICIO " + edificio.Nombre, ListGastos, ListIngresosComunes, ListCuotas, SaldoAnterior, edificioId, unidadTiempoId, false, fechaRegistro, LstLeyendas, false, LstDepartamentoAdelantado); //Gasto.SaldoMes.Value);
                    if (outputMemoryStream == null)
                    {
                        PostMessage(MessageType.Error, "Ocurrió un error. Por favor inténtelo más tarde");
                        return RedirectToAction("LstEdificio");
                    }

                    return File(outputMemoryStream, "application/pdf", "Reporte Ingresos y Gastos - " + edificio.Nombre + " - " + unidadTiempo.Descripcion + ".pdf");
                }
                else
                {
                    outputMemoryStream = reporteLogic.GetReportIngresosGastos("Ingresos y Gastos de " + unidadTiempo.Descripcion + " \n EDIFICIO " + edificio.Nombre, ListGastos, ListIngresosComunes, ListCuotas, SaldoAnterior, edificioId, unidadTiempoId, false, fechaRegistro, LstLeyendas, true, LstDepartamentoAdelantado); //Gasto.SaldoMes.Value);
                    if (outputMemoryStream == null)
                    {
                        PostMessage(MessageType.Error, "Ocurrió un error. Por favor inténtelo más tarde");
                        return RedirectToAction("LstEdificio");
                    }

                    return File(outputMemoryStream, "application/octet-stream", "Reporte Ingresos y Gastos - " + edificio.Nombre + " - " + unidadTiempo.Descripcion + ".zip");
                }

                //reporteLogic.GetReport(c, fechaEmision.Value, fechaVencimiento.Value, presupuestoMes, totalM2);

                //MemoryStream outputMemoryStream = reporteLogic.getFirstReport();
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error, ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty) + ex.StackTrace);
                return RedirectToAction("LstEdificio");
            }
        }
        public ActionResult BalanceOverview(Int32 EdificioId, Int32? UnidadTiempoId)
        {
            try
            {
                Int32 OrdenCalculo = context.Cuota.Where(X => X.Departamento.EdificioId == EdificioId && X.Pagado).Select(X => X.UnidadTiempo).ToList().Max(X => X.Orden) ?? -1;

                if (OrdenCalculo == -1)
                {
                    OrdenCalculo = context.UnidadTiempo.First(X => X.Estado == ConstantHelpers.EstadoActivo).Orden.Value;
                }

                if (!UnidadTiempoId.HasValue)
                    UnidadTiempoId = context.UnidadTiempo.FirstOrDefault(x => x.EsActivo).UnidadTiempoId;

                var UnidadTiempo = context.UnidadTiempo.FirstOrDefault(x => x.UnidadTiempoId == UnidadTiempoId);

                List<Cuota> ListCuotas = new List<Cuota>();
                var edificio = context.Edificio.First(X => X.EdificioId == EdificioId);
                var cuotas = context.Cuota.Where(X => X.Departamento.EdificioId == edificio.EdificioId && X.Pagado);
                //foreach (var cuota in cuotas)
                //{
                //    //Si no existe la fecha de pagado, añadir si cumple con la unidad de tiempo
                //    if (!cuota.FechaPagado.HasValue && cuota.UnidadTiempoId == UnidadTiempoId)
                //        ListCuotas.Add(cuota);
                //    else
                //        //Si existe la fecha de pagado, comprar el mes y el año , si encajan con esta unidad de tiempo, entonces son parte del reporte
                //        if (cuota.FechaPagado.HasValue && (cuota.FechaPagado.Value.Month == UnidadTiempo.Mes && cuota.FechaPagado.Value.Year == UnidadTiempo.Anio))
                //    {
                //        ListCuotas.Add(cuota);
                //    }
                //}

                Decimal TotalSaldoAnterior = 0;
                Int32 Orden = UnidadTiempo.Orden ?? 0;
                TotalSaldoAnterior += context.DetalleGasto.Where(X => X.Gasto.EdificioId == EdificioId && X.Gasto.UnidadTiempo.Orden < Orden && X.Pagado && X.Estado == ConstantHelpers.EstadoActivo).ToList().Sum(X => X.Monto);
                TotalSaldoAnterior += context.DetalleIngreso.Where(X => X.Ingreso.EdificioId == EdificioId && X.Ingreso.UnidadTiempo.Orden < Orden && X.Pagado && X.Estado == ConstantHelpers.EstadoActivo).ToList().Sum(X => X.Monto);
                TotalSaldoAnterior += context.Cuota.Where(X => X.Departamento.EdificioId == EdificioId && X.UnidadTiempo.Orden < Orden && X.Pagado && X.Estado == ConstantHelpers.EstadoActivo).ToList().Sum(X => X.Total);



                Decimal TotalPagosCuotasMes = 0;//ListCuotas.Sum(X => X.Total + X.Mora);
                Decimal TotalPagosCuotasAnterior = context.Cuota.Where(X => X.Pagado && X.Departamento.EdificioId == EdificioId && X.Estado == ConstantHelpers.EstadoActivo).ToList().Sum(X => X.Total + X.Mora) - TotalPagosCuotasMes;
                Decimal TotalIngresosAdicionales = context.Ingreso.Where(X => X.EdificioId == EdificioId && X.Estado == ConstantHelpers.EstadoActivo).ToList().Sum(X => X.DetalleIngreso.ToList().Sum(Y => Y.Monto));
                Decimal TotalGasto = context.Gasto.Where(X => X.EdificioId == EdificioId && X.Estado == ConstantHelpers.EstadoActivo).ToList().Sum(X => X.DetalleGasto.ToList().Sum(Y => Y.Monto));

                var LstUnidadTiempo = context.UnidadTiempo.Where(x => x.Estado == ConstantHelpers.EstadoActivo && x.Orden <= UnidadTiempo.Orden).OrderBy(x => x.Orden).ToList();

                Decimal SaldoAnterior = 0;
                Decimal SaldoAcumulado = 0;
                foreach (var item in LstUnidadTiempo)
                {
                    ListCuotas = new List<Cuota>();
                    foreach (var cuota in cuotas)
                    {
                        //Si no existe la fecha de pagado, añadir si cumple con la unidad de tiempo
                        if (!cuota.FechaPagado.HasValue && cuota.UnidadTiempoId == item.UnidadTiempoId)
                            ListCuotas.Add(cuota);
                        else
                            //Si existe la fecha de pagado, comprar el mes y el año , si encajan con esta unidad de tiempo, entonces son parte del reporte
                            if (cuota.FechaPagado.HasValue && (cuota.FechaPagado.Value.Month == item.Mes && cuota.FechaPagado.Value.Year == item.Anio))
                        {
                            ListCuotas.Add(cuota);
                        }
                    }
                    TotalPagosCuotasMes = ListCuotas.Sum(X => X.Total + X.Mora);
                    SaldoAnterior = SaldoAcumulado;

                    if (item.UnidadTiempoId == edificio.SaldoAnteriorUnidadTiempo)
                    {
                        SaldoAnterior += edificio.SaldoAnteriorHistorico ?? 0;
                    }


                    var GastoTemp = context.Gasto.Where(X => X.UnidadTiempoId == item.UnidadTiempoId && EdificioId == X.EdificioId && X.Estado == ConstantHelpers.EstadoActivo).ToList().Sum(X => X.DetalleGasto.Where(Y => Y.Pagado == true).ToList().Sum(Y => Y.Monto));
                    var IngresoTemp = TotalPagosCuotasMes + context.Ingreso.Where(X => X.UnidadTiempoId == item.UnidadTiempoId && EdificioId == X.EdificioId && X.Estado == ConstantHelpers.EstadoActivo).ToList().Sum(X => X.DetalleIngreso.ToList().Sum(Y => Y.Monto));
                    var SaldoTemp = IngresoTemp - GastoTemp;
                    SaldoAcumulado = SaldoAnterior + SaldoTemp;
                }

                var GastosActual = context.Gasto.Where(X => X.UnidadTiempoId == UnidadTiempoId && EdificioId == X.EdificioId && X.Estado == ConstantHelpers.EstadoActivo).ToList().Sum(X => X.DetalleGasto.Where(Y => Y.Pagado == true).ToList().Sum(Y => Y.Monto));
                var IngresosActual = context.Ingreso.Where(X => X.UnidadTiempoId == UnidadTiempoId && EdificioId == X.EdificioId && X.Estado == ConstantHelpers.EstadoActivo).ToList().Sum(X => X.DetalleIngreso.ToList().Sum(Y => Y.Monto));
                var Acumulado = TotalPagosCuotasMes + TotalPagosCuotasAnterior + TotalIngresosAdicionales - TotalGasto;
                IngresosActual += TotalPagosCuotasMes;

                var SaldoActual = IngresosActual - GastosActual;
                var AcumuladoActual = Acumulado - SaldoActual;
                decimal anteriorAbsoluto = 0;
                if (edificio != null)
                    anteriorAbsoluto = (edificio.SaldoAnteriorHistorico ?? 0);
                BalanceOverviewViewModel model = new BalanceOverviewViewModel();
                model.Fill(CargarDatosContext(), EdificioId);
                model.SaldoAnterior = SaldoAnterior;//TotalSaldoAnterior;//AcumuladoActual + anteriorAbsoluto;
                model.Saldo = SaldoActual;
                model.Ingresos = IngresosActual;
                model.Acumulado = SaldoAcumulado;// TotalSaldoAnterior + SaldoActual;//Acumulado + anteriorAbsoluto;
                model.Gastos = GastosActual;
                model.EdificioId = EdificioId;
                model.UnidadTiempoId = UnidadTiempoId;
                return View(model);

            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error, "Los parametros del edificio aun no califican para un balance. Revisar y reintentar o contactar al adminsitrador.\n Razón técnica: " + ex.Message);
                return RedirectToAction("LstEdificio", "Building");

                /*
                DateTime fechaRegistro = DateTime.Now;
                ReporteLogic reporteLogic = new ReporteLogic();
                reporteLogic.Server = Server;
                reporteLogic.context = context;

                var edificio = context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);

                List<Cuota> ListCuotas = new List<Cuota>();
                var departamentos = edificio.Departamento.ToList();

                foreach (var depa in departamentos)
                {
                    Cuota cuota = context.Cuota.FirstOrDefault(x => x.DepartamentoId == depa.DepartamentoId && x.UnidadTiempoId == unidadTiempoId);// && x.Estado.Equals(ConstantHelpers.EstadoActivo));
                    if (cuota == null) continue;
                    ListCuotas.Add(cuota);
                }
                //var presupuestoMes = ListCuotas.Sum(x => x.Monto); //Total de 
                // var totalM2 = departamentos.Sum(x => x.DepartamentoM2 ?? 0) + departamentos.Sum(x => x.EstacionamientoM2 ?? 0) + departamentos.Sum(x => x.EstacionamientoM2 ?? 0);
                var unidadTiempo = context.UnidadTiempo.FirstOrDefault(x => x.UnidadTiempoId == unidadTiempoId);
                var Gasto = context.Gasto.FirstOrDefault(x => x.EdificioId == EdificioId && x.UnidadTiempoId == unidadTiempoId && x.Estado.Equals(ConstantHelpers.EstadoActivo));
                List<DetalleGasto> ListGastos = context.DetalleGasto.Where(x => x.GastoId == Gasto.GastoId && x.Estado.Equals(ConstantHelpers.EstadoActivo)).ToList();
                //  Cuota c = listaCuota.Where(x => x.DepartamentoId == departamentoId).FirstOrDefault();
                // bool exportadoAntes = false;
                Decimal saldoAnterior = 0M;


                UnidadTiempo objUnidadTiempoAnterior = context.UnidadTiempo.FirstOrDefault(x => x.Orden == unidadTiempo.Orden - 1 && x.Estado.Equals(ConstantHelpers.EstadoActivo));
                if (objUnidadTiempoAnterior == null)
                {
                    saldoAnterior = 0M;
                }
                else
                {
                    var GastoMesAnterior = context.Gasto.FirstOrDefault(x => x.EdificioId == EdificioId && x.UnidadTiempoId == objUnidadTiempoAnterior.UnidadTiempoId && x.Estado.Equals(ConstantHelpers.EstadoActivo));
                    saldoAnterior = GastoMesAnterior == null ? 0 : GastoMesAnterior.SaldoMes.HasValue ? GastoMesAnterior.SaldoMes.Value : 0;
                }


                UnidadTiempo unidadTiempoActual = context.UnidadTiempo.FirstOrDefault(x => x.UnidadTiempoId == unidadTiempoId);
                Decimal TotalIngresosTotal = 0M;
                Decimal TotalIngresosMora = 0M;
                Decimal TotalIngresosCuota = 0M;
                Decimal TotalGastos = 0M;


                List<Departamento> LstDepartamentos = new List<Departamento>();
                LstDepartamentos = context.Departamento.Where(x => x.EdificioId == EdificioId && x.Estado.Equals(ConstantHelpers.EstadoActivo)).ToList();
                List<DateTime> LstFechasEmision = new List<DateTime>();
                edificio = context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);



                UnidadTiempo unidadTiempoAnterior = unidadTiempoActual;
                List<Cuota> lstIngresos = context.Cuota.Where(X => X.Departamento.EdificioId == EdificioId && X.Estado == ConstantHelpers.EstadoCerrado).ToList();
                List<DetalleIngreso> lstIngresosComunes = context.DetalleIngreso.Where(X => X.Ingreso.UnidadTiempoId == unidadTiempoId && X.Estado == ConstantHelpers.EstadoActivo && X.Ingreso.EdificioId == EdificioId).ToList();
                List<Cuota> ListIngresosTemp = lstIngresos;
                List<Decimal> LstMontoTotalDepa = new List<decimal>();
                List<Boolean> LstEncontrado = new List<bool>();
                lstIngresos.ForEach(x => LstMontoTotalDepa.Add(0M));
                lstIngresos.ForEach(x => LstFechasEmision.Add(DateTime.MinValue));
                lstIngresos.ForEach(x => LstEncontrado.Add(false));
                for (int i = 0; i < lstIngresos.Count; i++)
                    if (lstIngresos[i].Pagado)
                    {
                        LstFechasEmision.Add(new DateTime(unidadTiempoActual.Anio, unidadTiempoActual.Mes, edificio.DiaEmisionCuota));
                        LstMontoTotalDepa[i] += ListIngresosTemp[i].Total;
                        ListIngresosTemp[i].Estado = ConstantHelpers.EstadoCerrado;
                    }
                //else
                //LstFechasEmision[i] = DateTime.MinValue;
                while (true)
                {

                    if (unidadTiempoAnterior.Orden == 1) break;

                    UnidadTiempo unidadTiempoAnteriorAnterior = unidadTiempoAnterior;
                    unidadTiempoAnterior = context.UnidadTiempo.FirstOrDefault(x => x.Orden == unidadTiempoAnterior.Orden - 1 && x.Estado.Equals(ConstantHelpers.EstadoActivo));
                    if (unidadTiempoAnterior == null) break;
                    //   CuotaComun cuotaComun = context.CuotaComun.FirstOrDefault(x => x.EdificioId == EdificioId && x.UnidadTiempoId == unidadTiempoAnterior.UnidadTiempoId);
                    // if (cuotaComun == null || cuotaComun.Pagado) break;
                    ListIngresosTemp = context.Cuota.Where(x => x.UnidadTiempo.Estado.Equals(ConstantHelpers.EstadoActivo) && x.UnidadTiempo.Mes == unidadTiempoAnterior.Mes && x.UnidadTiempo.Anio == unidadTiempoAnterior.Anio && x.Departamento.EdificioId == EdificioId && x.Estado==ConstantHelpers.EstadoCerrado).ToList();
                    for (int i = 0; i < ListIngresosTemp.Count; i++)
                        if (ListIngresosTemp[i].Estado.Equals(ConstantHelpers.EstadoCerrado))
                        {
                            if (!LstEncontrado[i]) //ACA PONER = LstDepartamentos[i].MontoMora; que se genera al guardar para no estar exportando y copiando
                            {
                                LstFechasEmision[i] = new DateTime(unidadTiempoAnteriorAnterior.Anio, unidadTiempoAnteriorAnterior.Mes, edificio.DiaEmisionCuota);
                               // LstFechasEmision[i] = DateTime.Now;
                            }
                            LstEncontrado[i] = true;
                        }
                        else if (ListIngresosTemp[i].Pagado)
                        {
                            LstMontoTotalDepa[i] += ListIngresosTemp[i].Total;
                            ListIngresosTemp[i].Estado = ConstantHelpers.EstadoCerrado;
                        }


                }
               

                foreach (var gasto in ListGastos)
                {
                  
                    TotalGastos += gasto.Monto;
                }
            
                for (int i = 0; i < lstIngresos.Count; i++)
                {
              
                    lstIngresos[i].Mora = lstIngresos[i].Departamento.OmitirMora ? 0M : lstIngresos[i].Departamento.MontoMora;

                    TotalIngresosMora += lstIngresos[i].Mora;
                    //TotalIngresosMora += cuota.Mora;
                    TotalIngresosCuota += LstMontoTotalDepa[i];
                    TotalIngresosTotal += LstMontoTotalDepa[i] + lstIngresos[i].Mora;
                }
                //foreach (var cuota in lstIngresos)
                //{
                //    DataRow rowDepartamento = ds.Tables["DSIngresos"].NewRow();
                //    rowDepartamento["Departamento"] = cuota.Departamento.Numero;
                //    //rowDepartamento["Mora"] = cuota.Mora;
                //    cuota.Mora = cuota.Departamento.OmitirMora ? 0M : cuota.Departamento.MontoMora;
                //    rowDepartamento["Mora"] = cuota.Mora;
                //    rowDepartamento["Cuota"] = cuota.Total;
                //    rowDepartamento["Total"] = cuota.Total + cuota.Mora;

                //    ds.Tables["DSIngresos"].Rows.Add(rowDepartamento);
                //    TotalIngresosMora += cuota.Mora;
                //    //TotalIngresosMora += cuota.Mora;
                //    TotalIngresosCuota += cuota.Total;
                //    TotalIngresosTotal += cuota.Total + cuota.Mora;
                //}
            

                Decimal SaldoActual = TotalIngresosTotal - TotalGastos;
               //saldoAnterior
               // Decimal SaldoAcumulado = 0;
           


                */
            }
        }
        public ActionResult EnviarEmailValidaciones(Int32 EdificioId)
        {
            var model = new EnviarEmailValidacionesViewModel();
            model.Fill(CargarDatosContext(), EdificioId, Server.MapPath("~") + "/Files/Adjunto/");
            return View(model);
        }
        [HttpPost]
        public ActionResult EnviarEmailValidaciones(EnviarEmailValidacionesViewModel model)
        {
            try
            {
                if (model.Archivo == null)
                {
                    PostMessage(MessageType.Warning, "Debe de ajuntar un archivo excel (Telecrédito).");
                    return RedirectToAction("EnviarEmailValidaciones", new { EdificioId = model.EdificioId });
                }
                EmailLogic mailLogic = new EmailLogic(this, CargarDatosContext());
                ViewModel.Templates.emailValidacionViewModel mailModel = new ViewModel.Templates.emailValidacionViewModel();
                var usuario = context.Usuario.FirstOrDefault(x => x.UsuarioId == model.UsuarioId);
                var firma = usuario != null ? usuario.Firma : String.Empty;
                var edificio = context.Edificio.First(X => X.EdificioId == model.EdificioId);
                mailModel.Mensaje = model.Mensaje;
                mailModel.Titulo = model.Asunto;
                mailModel.administrador = edificio.EmailEncargado;
                mailModel.Firma = firma ?? String.Empty;
                mailModel.Acro = edificio.Acronimo;

                String fileExtension = Path.GetExtension(model.Archivo.FileName);

                IExcelDataReader excelReader = null;

                if (fileExtension == ".xls")
                    excelReader = ExcelReaderFactory.CreateBinaryReader(model.Archivo.InputStream);
                else if (fileExtension == ".xlsx")
                    excelReader = ExcelReaderFactory.CreateOpenXmlReader(model.Archivo.InputStream);

                DataSet result = excelReader.AsDataSet();

                for (int i = 0; i < result.Tables.Count; i++)
                {
                    DataTable table = result.Tables[i];
                    for (int j = 1; j < table.Rows.Count; j++)
                    {
                        if(table.Rows[j][8] != null)
                        {
                            var beneficiario = table.Rows[j][8].ToString();
                            var referencia = table.Rows[j][9].ToString();
                            var montoOperacion = table.Rows[j][11].ToDecimal().ToString("#,##0.00");
                            mailModel.LstValidacion.Add(new ViewModel.Templates.tablaValidacion
                            {
                                Beneficiario = beneficiario,
                                Referencia = referencia,
                                MontoOperacion = montoOperacion
                            });
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                var LstListaA = new List<String>();
                var LstArchivosAdjuntos = new List<String>();
                if (model.LstAdjuntos != null)
                {
                    foreach (var arc in model.LstAdjuntos)
                    {
                        if(arc != null)
                        {
                            if (LstListaA.Contains(arc.FileName))
                            {
                                continue;
                            }
                            if (arc == null)
                            {
                                continue;
                            }
                            var Ruta = Server.MapPath("~") + "/Files/Adjunto/";
                            var Nombre = arc.FileName;
                            LstArchivosAdjuntos.Add(Ruta + Nombre);
                            arc.SaveAs(Ruta + Nombre);

                            LstListaA.Add(arc.FileName);
                        }
                        
                    }
                }

                mailLogic.SendEmail(model.Asunto, "emailValidacion", model.CorreoRemitente, usuario.Nombres + " " + usuario.Apellidos
                    , model.Destinatarios, mailModel, LstArchivosAdjuntos, model.CopiaCarbon ?? String.Empty, model.CopiaOculta ?? String.Empty);

                PostMessage(MessageType.Success);
                return RedirectToAction("EnviarEmailValidaciones", new { EdificioId = model.EdificioId });
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error);
                return RedirectToAction("EnviarEmailValidaciones", new { EdificioId = model.EdificioId });
            }
        }
        public ActionResult EnviarEmailInformativo(Int32 EdificioId)
        {
            EnviarEmailInformativoViewModel model = new EnviarEmailInformativoViewModel();
            model.Fill(CargarDatosContext(), EdificioId);
            return View(model);
        }
        [HttpPost]
        public ActionResult EnviarEmailInformativo(EnviarEmailInformativoViewModel model, FormCollection formCollection)
        {
            EmailLogic mailLogic = new EmailLogic(this, CargarDatosContext());
            ViewModel.Templates.infoViewModel mailModel = new ViewModel.Templates.infoViewModel();
            var usuarioId = Session.GetUsuarioId();
            var usuario = context.Usuario.FirstOrDefault(x => x.UsuarioId == usuarioId);
            var firma = usuario.Firma;
            var edificio = context.Edificio.First(X => X.EdificioId == model.EdificioId);
            mailModel.Mensaje = model.Mensaje;
            mailModel.Titulo = model.Asunto;
            mailModel.administrador = edificio.EmailEncargado;
            mailModel.Firma = firma ?? "";
            mailModel.Acro = edificio.Acronimo;

            if (!String.IsNullOrEmpty(model.CopiaCarbon) && model.CopiaCarbon.Length > 5)
            {
                var ccAddress = model.CopiaCarbon.Split(',');
                foreach (var ccA in ccAddress)
                {
                    if (!String.IsNullOrEmpty(ccA))
                    {
                        model.lstDestinatario.Add(new EnviarEmailInformativoViewModel.Destinatario
                        {
                            dpto = "0",
                            email = ccA,
                            nombre = ccA,
                            id = "0"
                        });
                    }

                }
            }

            var LstDestinario = new List<Destinatario>();
            var LstEmailDiferentes = new List<String>();
            for (int i = 0; i < model.lstDestinatario.Count; i++)
            {
                var arrMail = model.lstDestinatario[i].email.Split(',');
                foreach (var item in arrMail)
                {
                    if (LstEmailDiferentes.Contains(item) == false && model.check[i])
                    {
                        LstDestinario.Add(new EnviarEmailInformativoViewModel.Destinatario
                        {
                            dpto = model.lstDestinatario[i].dpto,
                            email = item,
                            nombre = model.lstDestinatario[i].nombre,
                            id = model.lstDestinatario[i].id
                        });
                        LstEmailDiferentes.Add(item);
                    }
                }
            }

            for (int i = 0; i < LstDestinario.Count(); i++)
            {

                var destinatario = LstDestinario[i];

                mailModel.destinatario = destinatario;
                try
                {
                    List<String> Archivos = new List<string>();
                    foreach (var nuevoAdjunto in model.Archivos)
                    {
                        if (nuevoAdjunto != null)
                        {
                            var fileName = Path.GetFileName(nuevoAdjunto.FileName);
                            var path = Path.Combine(Server.MapPath("~/Resources"), fileName);
                            nuevoAdjunto.SaveAs(path);

                            Archivos.Add(path);

                        }
                    }

                    var emailUsuario = Session.GetCorreo();
                    var nombreUsuario = Session.GetNombreCompleto();
                    var nombreRemitente = Session.GetNombreRemitente();


                    if (!String.IsNullOrEmpty(nombreRemitente) && nombreRemitente.Length > 1)
                    {
                        nombreUsuario = nombreRemitente;
                    }

                    mailLogic.SendEmail(model.Asunto, "info", emailUsuario, nombreUsuario, destinatario.email, mailModel, Archivos, model.CopiaCarbon);

                }
                catch (Exception ex)
                {
                    try
                    {
                        PostMessage(MessageType.Error, "Error en el envío de mensaje a : " + destinatario.email + " Información del error: " + ex.Message + (ex.InnerException != null ? " Inner: " + ex.InnerException.Message.ToString() : String.Empty));
                        return RedirectToAction("EnviarEmailInformativo", "Building", new { EdificioId = model.EdificioId });
                    }
                    catch (Exception ex2)
                    {
                        PostMessage(MessageType.Error, "Error al reportar excepción");
                        return RedirectToAction("EnviarEmailInformativo", "Building", new { EdificioId = model.EdificioId });
                    }
                }
            }
            PostMessage(MessageType.Success);
            return RedirectToAction("EnviarEmailInformativo", "Building", new { EdificioId = model.EdificioId });
        }
        public ActionResult RecalcularBalanceOverview(Int32 EdificioId)
        {
            try
            {
                // Forma simplificada, la otra estaba bien si el software no fuese tan cambiante
                Int32 OrdenCalculo = context.Cuota.Where(X => X.Departamento.EdificioId == EdificioId && X.Pagado).Select(X => X.UnidadTiempo).Max(X => X.Orden).Value;
                var UnidadTiempo = context.UnidadTiempo.First(X => X.Estado == ConstantHelpers.EstadoActivo && X.Orden.Value == OrdenCalculo);
                var UnidadTiempoId = context.UnidadTiempo.Max(X => X.Orden).Value;
                if (UnidadTiempo != null)
                    UnidadTiempoId = UnidadTiempo.UnidadTiempoId;
                Decimal TotalPagosCuotas = context.Cuota.Where(X => X.Pagado && X.Departamento.EdificioId == EdificioId).ToList().Sum(X => X.Total);
                Decimal TotalIngresosAdicionales = context.Ingreso.Where(X => X.EdificioId == EdificioId).ToList().Sum(X => X.DetalleIngreso.ToList().Sum(Y => Y.Monto));
                Decimal TotalGasto = context.Gasto.Where(X => X.EdificioId == EdificioId).ToList().Sum(X => X.DetalleGasto.ToList().Sum(Y => Y.Monto));

                var GastosActual = context.Gasto.Where(X => X.UnidadTiempoId == UnidadTiempoId && EdificioId == X.EdificioId).ToList().Sum(X => X.DetalleGasto.ToList().Sum(Y => Y.Monto));
                var IngresosActual = context.Ingreso.Where(X => X.UnidadTiempoId == UnidadTiempoId && EdificioId == X.EdificioId).ToList().Sum(X => X.DetalleIngreso.ToList().Sum(Y => Y.Monto));
                var SaldoActual = IngresosActual - GastosActual;
                var Acumulado = TotalPagosCuotas + TotalIngresosAdicionales - TotalGasto;
                var AcumuladoActual = Acumulado - SaldoActual;
                var edificio = context.Edificio.FirstOrDefault(X => X.EdificioId == EdificioId);
                decimal anteriorAbsoluto = 0;
                if (edificio != null)
                    anteriorAbsoluto = (edificio.SaldoAnteriorHistorico ?? 0);

                BalanceOverviewViewModel model = new BalanceOverviewViewModel();
                model.Saldo = SaldoActual;
                model.SaldoAnterior = AcumuladoActual + anteriorAbsoluto;
                model.Ingresos = IngresosActual;
                model.Acumulado = Acumulado + anteriorAbsoluto;
                model.Gastos = TotalGasto;
                model.EdificioId = EdificioId;

                return RedirectToAction("BalanceOverview", "Building", new { EdificioId = EdificioId });
                /*

                //Ubicar unidad de tiempo actual
                UnidadTiempo unidadTiempoActual = context.UnidadTiempo.Where(X => X.EsActivo == true && X.Estado == ConstantHelpers.EstadoActivo).FirstOrDefault();
                Int32 unidadTiempoId = unidadTiempoActual.UnidadTiempoId;
                BalanceUnidadTiempoEdificio BalanceActual = null;


                // Sin historial Saldo acumulado es 0
                Decimal SaldoAcumulado = 0;



                Int32 OrdenCalculo = context.Cuota.Where(X => X.Departamento.EdificioId == EdificioId).Select(X => X.UnidadTiempo).Min(X => X.Orden).Value;
                UnidadTiempo UnidadTiempoCalculo = context.UnidadTiempo.First(X => X.Estado == ConstantHelpers.EstadoActivo && X.Orden.Value == OrdenCalculo);

                while (BalanceActual == null)
                {
                    BalanceUnidadTiempoEdificio BalanceCalculo = context.BalanceUnidadTiempoEdificio.FirstOrDefault(X => X.UnidadDeTiempoId == UnidadTiempoCalculo.UnidadTiempoId && X.EdificioId == EdificioId);
                    if (BalanceCalculo == null)
                    {
                        BalanceCalculo = new BalanceUnidadTiempoEdificio();
                        BalanceCalculo.EdificioId = EdificioId;
                        BalanceCalculo.UnidadDeTiempoId = UnidadTiempoCalculo.UnidadTiempoId;
                        context.BalanceUnidadTiempoEdificio.Add(BalanceCalculo);
                    }

                    BalanceCalculo.IngresosTotalesMes = 0;
                    BalanceCalculo.GastosTotalesMes = 0;
                    BalanceCalculo.GastosTotalesMes = 0;
                    BalanceCalculo.FechaDeActualizacion = DateTime.Now;


                    BalanceCalculo.SaldoAcumulado = SaldoAcumulado;

                    //Calcular Ingresos
                    //Calcular Ingreso de cuotas
                    Decimal TotalCuotasPagadas = context.Cuota.Where
                        (X =>
                               X.UnidadTiempoId == UnidadTiempoCalculo.UnidadTiempoId
                            && X.Departamento.EdificioId == EdificioId
                                   //   && X.Estado == ConstantHelpers.EstadoCerrado
                                   //&& (X.Mora == 0 || X.Mora == null)
                            && X.Pagado)
                            .Select(X => X.Monto + X.Mora).DefaultIfEmpty(0).Sum();
                    //Calcular Ingresos comunes 
                    Decimal TotalIngresosComunes = context.DetalleIngreso.Where
                        (X => X.Estado == ConstantHelpers.EstadoActivo
                            && X.Ingreso.EdificioId == EdificioId
                            && X.Ingreso.UnidadTiempoId == UnidadTiempoCalculo.UnidadTiempoId)
                            .Select(X => X.Monto).DefaultIfEmpty(0).Sum();


                    //Calcular Gastos
                    //Calcular Gastos comunes
                    Decimal TotalGastos = context.DetalleGasto.Where
                        (X => X.Estado == ConstantHelpers.EstadoActivo
                            && X.Gasto.EdificioId == EdificioId
                            && X.Gasto.UnidadTiempoId == UnidadTiempoCalculo.UnidadTiempoId)
                            .Select(X => X.Monto).DefaultIfEmpty(0).Sum();

                    //Calcular Saldo
                    Decimal Saldo = TotalCuotasPagadas + TotalIngresosComunes - TotalGastos;

                    BalanceCalculo.IngresosTotalesMes = TotalIngresosComunes + TotalCuotasPagadas;
                    BalanceCalculo.GastosTotalesMes = TotalGastos;
                    BalanceCalculo.SaldoMes = Saldo;
                    BalanceCalculo.SaldoAcumulado = SaldoAcumulado;

                    SaldoAcumulado = BalanceCalculo.SaldoAcumulado + Saldo;
                    context.SaveChanges();


                    UnidadTiempoCalculo = null;

                    // Se termino de calcular
                    if (OrdenCalculo == unidadTiempoActual.Orden.Value)
                    {
                        BalanceActual = BalanceCalculo;
                        break;
                    }
                    // El software cuenta con matenimiento por 100 años :P
                    do
                    {
                        OrdenCalculo++;
                        UnidadTiempoCalculo = context.UnidadTiempo.FirstOrDefault(X => X.Orden.Value == OrdenCalculo);

                    } while (UnidadTiempoCalculo == null && OrdenCalculo < 24000 && OrdenCalculo != unidadTiempoActual.Orden.Value);
                    if (OrdenCalculo == 24000)
                    {
                        //Error , no se encontro Unidad de tiempo siguiente
                        throw new Exception("No encontro siguiente unidad de tiempo"); // SeMurioException
                    }


                }

                */




            }
            catch (Exception ex)
            {
                return null;



            }
        }

        //public ActionResult _EditBalanceOverView()
        //{
        //}
        //[HttpPost]
        //public ActionResult _EditBalanceOverView(Int32 EdificioId)
        //{
        //    BalanceOverviewViewModel model = new BalanceOverviewViewModel();
        //    model.
        //}

        public ActionResult UploadReport(Int32 EdificioId, Int32? UnidadTiempoId)
        {
            UploadReportViewModel model = new UploadReportViewModel();

            model.fill(CargarDatosContext(), EdificioId, UnidadTiempoId);

            return View(model);
        }
        [HttpPost]
        public ActionResult UploadReport(UploadReportViewModel model)
        {
            Edificio edificio = context.Edificio.First(X => X.EdificioId == model.EdificioId);
            UnidadTiempo unidad = context.UnidadTiempo.First(X => X.UnidadTiempoId == model.UnidadTiempoId.Value);
            if (model.archivoReporte != null && model.archivoReporte.ContentLength != 0)
            {
                string _rutaarchivoserv = Server.MapPath("~");
                string _rutaarchivodir = _rutaarchivoserv + Path.Combine("/Resources/Files", edificio.Acronimo);
                if (!System.IO.Directory.Exists(_rutaarchivodir))
                    Directory.CreateDirectory(_rutaarchivodir);

                string _nombrearc = "Reporte Edificio " + edificio.Nombre + " " + unidad.Descripcion + Path.GetExtension(model.archivoReporte.FileName);
                _rutaarchivodir = Path.Combine(_rutaarchivodir, _nombrearc);

                model.archivoReporte.SaveAs(_rutaarchivodir);


                ReporteEdificioUnidadTiempo reporte = new ReporteEdificioUnidadTiempo();
                reporte.Nombre = _nombrearc;
                reporte.Ruta = _rutaarchivodir;

                reporte.EdificioId = model.EdificioId;
                var repotesAnteriories = context.ReporteEdificioUnidadTiempo.Where(X => X.EdificioId == model.EdificioId && X.UnidadTiempoId == model.UnidadTiempoId.Value).ToList();
                reporte.UnidadTiempoId = model.UnidadTiempoId.Value;
                foreach (var reporteAntiguo in repotesAnteriories)
                {
                    context.ReporteEdificioUnidadTiempo.Remove(reporteAntiguo);
                }
                context.ReporteEdificioUnidadTiempo.Add(reporte);
                context.SaveChanges();
                PostMessage(MessageType.Success, "El Archivo se subió correctamente");
            }



            return RedirectToAction("LstEdificio", "Building");
        }



        public ActionResult ArchivosCorregidos(Int32 UnidadTiempoId, Int32 EdificioId)
        {
            ArchivosCorregidosViewModel model = new ArchivosCorregidosViewModel();
            model.fill(CargarDatosContext(), UnidadTiempoId, EdificioId);
            return View(model);
        }
        public ActionResult AddEditArchivoCorregido(Int32? ArchivoCorregidoId, Int32 UnidadTiempoId, Int32 EdificioId)
        {
            AddEditArchivoCorregidoViewModel model = new AddEditArchivoCorregidoViewModel();
            model.ArchivoCorregidoId = ArchivoCorregidoId; model.UnidadTiempoId = UnidadTiempoId; model.EdificioId = EdificioId;
            model.fill(CargarDatosContext(), EdificioId);
            return View(model);

        }
        [HttpPost]
        public ActionResult AddEditArchivoCorregido(AddEditArchivoCorregidoViewModel model)
        {

            var archivoCorregidoEdifico = context.ArchivoCorrecionEdificio.FirstOrDefault(X => X.ArchivoCorrecionEdificioId == (model.ArchivoCorregidoId ?? -1)) ?? new ArchivoCorrecionEdificio();
            if (archivoCorregidoEdifico.ArchivoCorrecionEdificioId == 0)
                context.ArchivoCorrecionEdificio.Add(archivoCorregidoEdifico);
            archivoCorregidoEdifico.EdificioId = model.EdificioId;
            archivoCorregidoEdifico.UnidadTiempoId = model.UnidadTiempoId;
            archivoCorregidoEdifico.Tipo = model.Tipo + (!(String.IsNullOrEmpty(model.Departamento)) ? "/" + model.Departamento : "");
            archivoCorregidoEdifico.FechaSubido = DateTime.Now;


            if (model.archivo != null)
            {
                string nombre = Guid.NewGuid().ToString().Substring(0, 6) + Path.GetExtension(model.archivo.FileName);
                string ruta = Path.Combine(Server.MapPath("~/Resources/Files/Corregidos"), nombre);
                archivoCorregidoEdifico.RutaArchivo = nombre;
                model.archivo.SaveAs(ruta);
            }

            context.SaveChanges();
            return RedirectToAction("ArchivosCorregidos", new { UnidadTiempoId = model.UnidadTiempoId, EdificioId = model.EdificioId });
        }

        public ActionResult DeleteArchivoCorregido(Int32 ArchivoCorregidoId, Int32 UnidadTiempoId, Int32 EdificioId)
        {
            var archivo = context.ArchivoCorrecionEdificio.FirstOrDefault(X => X.ArchivoCorrecionEdificioId == ArchivoCorregidoId);
            if (archivo != null)
            {
                context.ArchivoCorrecionEdificio.Remove(archivo);
                context.SaveChanges();
            }
            return RedirectToAction("ArchivosCorregidos", new { UnidadTiempoId = UnidadTiempoId, EdificioId = EdificioId });
        }
        public ActionResult DeleteNormasConvivencia(Int32 EdificioId)
        {
            try
            {
                var edificio = context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);
                var buffer = Path.Combine(Server.MapPath("~/Resources/Files"), edificio.Acronimo, edificio.NormasConvivencia);
                if (System.IO.File.Exists(buffer))
                {
                    System.IO.File.Delete(buffer);
                    edificio.NormasConvivencia = String.Empty;
                    context.SaveChanges();
                    PostMessage(MessageType.Success);
                }
                else
                {
                    PostMessage(MessageType.Warning, "No se encontró archivo");
                }
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error);
            }
            return RedirectToAction("AddEditEdificio", new { EdificioId = EdificioId });
        }
        public ActionResult DeleteEdificio(Int32 EdificioId)
        {
            try
            {
                var edificio = context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);
                edificio.Estado = ConstantHelpers.EstadoEliminado;
                context.SaveChanges();
                PostMessage(MessageType.Success);

            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error);
            }
            return RedirectToAction("AddEditEdificio", new { EdificioId = EdificioId });
        }
        //public ActionResult AsignarNumeroRecibo()
        //{
        //    try
        //    {
        //        var UnidadTiempo = context.UnidadTiempo.FirstOrDefault(x => x.EsActivo);
        //        var LstEdificios = context.Edificio.ToList();
        //        foreach (var Edificio in LstEdificios)
        //        {
        //            var IndiceCantidadReporte = Edificio.CantidadReporte;
        //            var NumeroActual = IndiceCantidadReporte + (Edificio.DesfaseRecibos.HasValue ? Edificio.DesfaseRecibos.Value : 0);

        //            foreach (var dep in Edificio.Departamento.ToList())
        //            {
        //                NumeroActual = IndiceCantidadReporte + (Edificio.DesfaseRecibos.HasValue ? Edificio.DesfaseRecibos.Value : 0);

        //                var utRecibo = new UnidadTiempoReciboDepartamento();
        //                utRecibo.DepartamentoId = dep.DepartamentoId;
        //                utRecibo.UnidadTiempoId = UnidadTiempo.UnidadTiempoId;
        //                utRecibo.NumeroRecibo = NumeroActual;
        //                context.UnidadTiempoReciboDepartamento.Add(utRecibo);
        //                context.SaveChanges();

        //                IndiceCantidadReporte++;
        //            }
        //        }
        //        PostMessage(MessageType.Success);
        //    }
        //    catch (Exception ex)
        //    {
        //        PostMessage(MessageType.Error);
        //    }
        //    return RedirectToAction("LstEdificio");
        //}
        [AppAuthorize(AppRol.Propietario, AppRol.Administrador)]

        public ActionResult LstEstadoCuentaBancario(Int32 EdificioId, Int32? Anio)
        {
            var model = new LstEstadoCuentaBancarioViewModel();
            model.Fill(CargarDatosContext(), Anio, EdificioId);
            return View(model);
        }
        public ActionResult DeleteEstadoCuentaBancario(Int32 EdificioId, Int32 EstadoCuentaBancarioId, Int32 Anio)
        {
            try
            {
                var estado = context.EstadoCuentaBancario.FirstOrDefault(x => x.EstadoCuentaBancarioId == EstadoCuentaBancarioId);
                estado.Estado = ConstantHelpers.EstadoInactivo;
                context.SaveChanges();

                PostMessage(MessageType.Success);
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error);
            }
            return RedirectToAction("LstEstadoCuentaBancario", new { Anio = Anio, EdificioId = EdificioId });
        }
        public ActionResult _AddEditEstadoCuentaBancario(Int32? EstadoCuentaBancarioId, Int32 EdificioId)
        {
            var model = new AddEditEstadoCuentaBancarioViewModel();
            model.Fill(CargarDatosContext(), EstadoCuentaBancarioId, EdificioId);
            return View(model);
        }
        [HttpPost]
        public ActionResult _AddEditEstadoCuentaBancario(AddEditEstadoCuentaBancarioViewModel model)
        {
            try
            {
                EstadoCuentaBancario estadoCuenta = null;
                if (model.EstadoCuentaBancarioId.HasValue)
                {
                    estadoCuenta = context.EstadoCuentaBancario.FirstOrDefault(x => x.EstadoCuentaBancarioId == model.EstadoCuentaBancarioId);
                }
                else
                {
                    estadoCuenta = new EstadoCuentaBancario();
                    estadoCuenta.Estado = ConstantHelpers.EstadoActivo;
                    estadoCuenta.EdificioId = model.EdiId;
                    context.EstadoCuentaBancario.Add(estadoCuenta);
                }
                estadoCuenta.UnidadTiempoId = model.UnidadTiempoId;
                estadoCuenta.Nombre = model.Nombre;
                if (model.Archivo != null && model.Archivo.ContentLength != 0)
                {
                    string _rutaFirmaserv = Server.MapPath("~") + "/Files/";
                    string _nombrearc = Guid.NewGuid().ToString().Substring(0, 4) + model.Archivo.FileName;
                    model.Archivo.SaveAs(_rutaFirmaserv + _nombrearc);
                    estadoCuenta.Ruta = _nombrearc;
                }

                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error);
            }
            return RedirectToAction("LstEstadoCuentaBancario", new { EdificioId = model.EdiId });
        }

        public ActionResult _AddEditEmailValidacion(Int32? EmailValidacionId, Int32 EdificioId)
        {
            var model = new _AddEditEmailValidacionViewModel();
            model.Fill(CargarDatosContext(), EmailValidacionId, EdificioId);
            return View(model);
        }
        [HttpPost]
        public ActionResult _AddEditEmailValidacion(_AddEditEmailValidacionViewModel model)
        {
            try
            {
                EmailValidacion email = null;
                if (model.EmailValidacionId.HasValue)
                {
                    email = context.EmailValidacion.FirstOrDefault(x => x.EmailValidacionId == model.EmailValidacionId);
                }
                else
                {
                    email = new EmailValidacion();
                    email.EdificioId = model.EdificioId;
                    context.EmailValidacion.Add(email);
                }
                email.Destinatarios = model.Destinatarios;
                email.Asunto = model.Asunto;
                email.UsuarioId = model.UsuarioId;
                email.CopiaCarbon = model.CopiaCarbon ?? String.Empty;
                email.Mensaje = model.Mensaje;
                email.CopiaOculta = model.CopiaOculta;

                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error);
            }
            return RedirectToAction("EnviarEmailValidaciones", new { EdificioId = model.EdificioId });
        }
        [AppAuthorize(AppRol.Propietario)]
        public ActionResult LstCronogramaMantenimientoProp(Int32 EdificioId, Int32? Anio)
        {
            var model = new LstCronogramaMantenimientoViewModel();
            model.Fill(CargarDatosContext(), EdificioId, Anio);
            return View(model);
        }
        public ActionResult LstCronogramaMantenimiento(Int32 EdificioId, Int32? Anio)
        {
            var model = new LstCronogramaMantenimientoViewModel();
            model.Fill(CargarDatosContext(), EdificioId, Anio);
            return View(model);
        }
        [HttpPost]
        public ActionResult ProcesarCronograma(FormCollection frm)
        {
            try
            {
                var EdificioId = frm["EdificioId"].ToInteger();
                var Anio = frm["Anio"].ToInteger();
                var LstCronograma = context.Cronograma.Where(x => x.EdificioId == EdificioId
               && x.Anio == Anio).ToList();
                foreach (var item in LstCronograma)
                {
                    foreach (var detalle in item.DetalleCronograma.ToList())
                    {
                        detalle.EsRealizado = false;
                        detalle.Dia = null;
                        detalle.EsMarcado = false;
                    }
                }
                context.SaveChanges();

                var chkEsRealizado = frm.AllKeys.Where(X => X.StartsWith("chk-")).ToList();
                foreach (var item in chkEsRealizado)
                {
                    var valor = frm[item].ToString();
                    if (!String.IsNullOrEmpty(valor))
                    {
                        var DetalleCronogramaId = item.Replace("chk-", String.Empty).ToInteger();
                        var detalle = context.DetalleCronograma.FirstOrDefault(x => x.DetalleCronogramaId == DetalleCronogramaId);
                        detalle.EsRealizado = valor == "on" ? true : false;
                        var valorDia = frm["input-" + DetalleCronogramaId.ToString()];
                        if (!String.IsNullOrEmpty(valorDia))
                        {
                            detalle.EsMarcado = true;
                        }
                    }
                }
                if (chkEsRealizado.Count == 0)
                {
                    var inputDias = frm.AllKeys.Where(X => X.StartsWith("input-")).ToList();
                    foreach (var item in inputDias)
                    {
                        var valor = frm[item].ToString();
                        if (!String.IsNullOrEmpty(valor))
                        {
                            var DetalleCronogramaId = item.Replace("input-", String.Empty).ToInteger();
                            var detalle = context.DetalleCronograma.FirstOrDefault(x => x.DetalleCronogramaId == DetalleCronogramaId);
                            //detalle.EsRealizado = valor == "on" ? true : false;
                            detalle.Dia = valor.ToInteger();

                            if (!String.IsNullOrEmpty(valor))
                            {
                                detalle.EsMarcado = true;
                            }

                        }
                    }
                }
                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error);
            }
            return RedirectToAction("LstCronogramaMantenimiento", new { EdificioId = frm["EdificioId"] });
        }
        public ActionResult _AddEditCronograma(Int32 EdificioId, Int32? CronogramaId, Int32 Anio)
        {
            var model = new _AddEditCronogramaViewModel();
            model.Fill(CargarDatosContext(), EdificioId, CronogramaId, Anio);
            return View(model);
        }
        [HttpPost]
        public ActionResult _AddEditCronograma(_AddEditCronogramaViewModel model)
        {
            try
            {
                Cronograma cronograma = null;
                if (model.CronogramaId.HasValue)
                {
                    cronograma = context.Cronograma.FirstOrDefault(x => x.CronogramaId == model.CronogramaId);
                }
                else
                {
                    cronograma = new Cronograma();
                    cronograma.EdificioId = model.EdificioId;
                    cronograma.Estado = ConstantHelpers.EstadoActivo;
                    context.Cronograma.Add(cronograma);
                    for (int i = 1; i <= 12; i++)
                    {
                        var detalle = new DetalleCronograma();
                        detalle.Cronograma = cronograma;
                        detalle.Mes = i;
                        detalle.EsRealizado = false;
                        detalle.EsMarcado = false;
                        context.DetalleCronograma.Add(detalle);
                    }
                }
                cronograma.Nombre = model.Nombre;
                cronograma.Orden = model.Orden;
                cronograma.Anio = model.Anio.Value;

                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error);
            }
            return RedirectToAction("LstCronogramaMantenimiento", new { EdificioId = model.EdificioId });
        }
        public ActionResult DeleteCronograma(Int32 EdificioId, Int32 CronogramaId)
        {
            try
            {
                var cronograma = context.Cronograma.FirstOrDefault(x => x.CronogramaId == CronogramaId);
                cronograma.Estado = ConstantHelpers.EstadoInactivo;
                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error);
            }
            return RedirectToAction("LstCronogramaMantenimiento", new { EdificioId = EdificioId });
        }
        public ActionResult _ReplicarCronograma(Int32 EdificioId, Int32 Anio)
        {
            var model = new _ReplicarCronogramaViewModel();
            model.Fill(CargarDatosContext(), EdificioId, Anio);
            return View(model);
        }
        [HttpPost]
        public ActionResult _ReplicarCronograma(_ReplicarCronogramaViewModel model)
        {
            try
            {
                var cronograma = context.Cronograma.Where(x => x.EdificioId == model.EdificioId
                && x.Anio == model.Anio);
                foreach (var item in cronograma)
                {
                    var nCronograma = new Cronograma();
                    nCronograma.Anio = model.AnioDestino;
                    nCronograma.EdificioId = item.EdificioId;
                    nCronograma.Orden = item.Orden;
                    nCronograma.Estado = item.Estado;
                    nCronograma.Nombre = item.Nombre;
                    context.Cronograma.Add(nCronograma);

                    foreach (var detalle in item.DetalleCronograma.ToList())
                    {
                        var detalleCronograma = new DetalleCronograma();
                        detalleCronograma.Cronograma = nCronograma;
                        detalleCronograma.Dia = detalle.Dia;
                        detalleCronograma.Mes = detalle.Mes;
                        //detalleCronograma.EsMarcado = detalle.EsMarcado;
                        detalleCronograma.EsRealizado = detalle.EsRealizado;
                        context.DetalleCronograma.Add(detalleCronograma);
                    }
                }
                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error);
            }
            return RedirectToAction("LstCronogramaMantenimiento", new { EdificioId = model.EdificioId });
        }
        public ActionResult _ReplicarCronogramaFuturo(Int32 EdificioId, Int32 Anio)
        {
            var model = new _ReplicarCronogramaViewModel();
            model.Fill(CargarDatosContext(), EdificioId, Anio);
            return View(model);
        }
        [HttpPost]
        public ActionResult _ReplicarCronogramaFuturo(_ReplicarCronogramaViewModel model)
        {
            try
            {
                var cronograma = context.Cronograma.Where(x => x.EdificioId == model.EdificioId
                && x.Anio == model.Anio);
                foreach (var item in cronograma)
                {
                    var nCronograma = new Cronograma();
                    nCronograma.Anio = model.AnioDestino;
                    nCronograma.EdificioId = item.EdificioId;
                    nCronograma.Orden = item.Orden;
                    nCronograma.Estado = item.Estado;
                    nCronograma.Nombre = item.Nombre;
                    context.Cronograma.Add(nCronograma);

                    foreach (var detalle in item.DetalleCronograma.ToList())
                    {
                        var detalleCronograma = new DetalleCronograma();
                        detalleCronograma.Cronograma = nCronograma;
                        detalleCronograma.Dia = detalle.Dia;
                        detalleCronograma.Mes = detalle.Mes;
                        detalleCronograma.EsMarcado = detalle.EsMarcado;
                        detalleCronograma.EsRealizado = detalle.EsRealizado;
                        context.DetalleCronograma.Add(detalleCronograma);
                    }
                }
                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error);
            }
            return RedirectToAction("LstCronogramaMantenimiento", new { EdificioId = model.EdificioId });
        }

        public ActionResult _EditarGrupoEquipos(Int32 EdificioId, String filtroTipo, String vista, String LstDatosId, String Nombre)
        {
            var model = new _EditarGrupoEquiposViewModel();
            model.Fill(CargarDatosContext(), EdificioId, filtroTipo, vista, LstDatosId, Nombre);
            return View(model);
        }
        [HttpPost]
        public ActionResult _EditarGrupoEquipos(_EditarGrupoEquiposViewModel model)
        {
            try
            {
                var DatosId = model.LstDatosId.Split(',');
                foreach (var item in DatosId)
                {
                    var DatoEdificioId = item.ToInteger();
                    var dato = context.DatoEdificio.FirstOrDefault(x => x.DatoEdificioId == DatoEdificioId);
                    dato.Tipo = "Equipo[" + model.Nombre + "]";
                }
                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error);
            }
            return RedirectToAction("ItemsGenericos", "External", new { EdificioId = model.EdificioId, filtroTipo = model.FiltroTipo, Vista = model.vista });
        }
    }
}