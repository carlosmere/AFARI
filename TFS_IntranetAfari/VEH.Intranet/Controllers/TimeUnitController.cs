using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.ViewModel.TimeUnit;
using VEH.Intranet.Models;
using VEH.Intranet.Helpers;
using VEH.Intranet.Filters;

namespace VEH.Intranet.Controllers
{
    [AppAuthorize(AppRol.Administrador)]
    public class TimeUnitController : BaseController
    {
        //
        // GET: /TimeUnit/

        [ViewParameter("UnidadTiempo", "fa fa-clock-o")]
        public ActionResult LstUnidadDeTiempo()
        {
            LstUnidadDeTiempoViewModel ViewModel = new LstUnidadDeTiempoViewModel();
            ViewModel.Fill(CargarDatosContext());
            return View(ViewModel);
        }

        [ViewParameter("UnidadTiempo", "fa fa-clock-o")]
        public ActionResult AddEditUnidadDeTiempo(Int32? UnidadDeTiempoId)
        {
            AddEditUnidadDeTiempoViewModel ViewModel = new AddEditUnidadDeTiempoViewModel();
            ViewModel.UnidadTiempoId = UnidadDeTiempoId;
            ViewModel.Fill(CargarDatosContext());
            return View(ViewModel);
        }

        [HttpPost]
        public ActionResult AddEditUnidadDeTiempo(AddEditUnidadDeTiempoViewModel ViewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewModel.Fill(CargarDatosContext());
                TryUpdateModel(ViewModel);
                return View(ViewModel);
            }

            try
            {
                UnidadTiempo unidaddetiempo = null;
                
                if (ViewModel.UnidadTiempoId.HasValue)
                {
                    if (context.UnidadTiempo.Any(x => x.Estado.Equals("ACT") && x.UnidadTiempoId != ViewModel.UnidadTiempoId && x.Mes == ViewModel.Mes && x.Anio == ViewModel.Anio))
                    {
                        //Ya existe, retornar error de Already exist
                        PostMessage(MessageType.AExist);
                        return RedirectToAction("LstUnidadDeTiempo");
                    }


                    unidaddetiempo = context.UnidadTiempo.FirstOrDefault(x => x.UnidadTiempoId == ViewModel.UnidadTiempoId);
                    unidaddetiempo.Descripcion = ConstantHelpers.ObtenerMesPorValorId(ViewModel.Mes.ToString()).ToUpper() + " " + ViewModel.Anio;
                    unidaddetiempo.Anio = ViewModel.Anio.ToInteger();
                    unidaddetiempo.Mes = ViewModel.Mes;
                    
                    if(ViewModel.Orden != null && ViewModel.Orden <= 0 )
                    {
                        PostMessage(MessageType.Error); 
                        return RedirectToAction("LstUnidadDeTiempo");
                    }
                    unidaddetiempo.Orden = ViewModel.Orden;
                    
                    context.Entry(unidaddetiempo).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    if (context.UnidadTiempo.Any(x => x.Estado.Equals("ACT") && x.Mes == ViewModel.Mes && x.Anio == ViewModel.Anio))
                    {
                        //Ya existe, retornar error de Already exist
                        PostMessage(MessageType.AExist);
                        return RedirectToAction("LstUnidadDeTiempo");
                    }

                    unidaddetiempo = new UnidadTiempo();
                    unidaddetiempo.Descripcion = ConstantHelpers.ObtenerMesPorValorId(ViewModel.Mes.ToString()).ToUpper() + " " + ViewModel.Anio;
                    unidaddetiempo.Anio = ViewModel.Anio.ToInteger();
                    unidaddetiempo.Mes = ViewModel.Mes;
                    unidaddetiempo.Estado = ConstantHelpers.EstadoActivo;
                    unidaddetiempo.Orden = context.UnidadTiempo.Where(x => x.Estado.Equals(ConstantHelpers.EstadoActivo)).Count() + 1;
                    context.UnidadTiempo.Add(unidaddetiempo);
                }
                List<UnidadTiempo> lstUnidadTiempo = context.UnidadTiempo.ToList();
                if  (ViewModel.EsActivo)
                {                 
                    foreach (UnidadTiempo objUnidadTiempo in lstUnidadTiempo)
                        objUnidadTiempo.EsActivo = false;

                    unidaddetiempo.EsActivo = ViewModel.EsActivo;
                }
                //if(unidaddetiempo.Anio == lstUnidadTiempo.Max( x => x.Anio) + 1)
                //{
                //    try
                //    {
                //        var anioAnterior = unidaddetiempo.Anio - 1;
                //        var LstEquipos = context.DatoEdificio.Where(X => X.Tipo.Contains("Equipo") && X.AplicaMantenimiento == true && X.UnidadTiempo.Anio == anioAnterior).ToList();
                //        foreach (var item in LstEquipos)
                //        {
                //            var equipo = new DatoEdificio();
                //            equipo.EdificioId = item.EdificioId;
                //            equipo.Tipo = item.Tipo;
                //            equipo.Dato = item.Dato;
                //            equipo.Nombre = item.Nombre;
                //            equipo.UnidadTiempo = unidaddetiempo;
                //            equipo.AplicaMantenimiento = true;
                //            context.DatoEdificio.Add(equipo);
                //        }
                //        var LstDatos = context.DatoEdificio.Where(X => X.Tipo.Contains("Crono") && X.UnidadTiempo.Anio == anioAnterior).ToList();
                //        foreach (var item in LstDatos)
                //        {
                //            var crono = new DatoEdificio();
                //            crono.EdificioId = item.EdificioId;
                //            crono.Tipo = item.Tipo;
                //            crono.Dato = item.Dato;
                //            crono.Nombre = item.Nombre;
                //            crono.UnidadTiempo = unidaddetiempo;
                //            crono.AplicaMantenimiento = true;
                //            context.DatoEdificio.Add(crono);
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        PostMessage(MessageType.Warning, "No se pudo replicar el cronograma del año pasado, error: " + ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty));
                //    }
                //
                //}
                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch { PostMessage(MessageType.Error); }
            return RedirectToAction("LstUnidadDeTiempo");
        }

        public ActionResult _DeleteUnidadDeTiempo(Int32 UnidadTiempoId)
        {
            ViewBag.UnidadTiempoId = UnidadTiempoId;
            return PartialView();
        }

        [HttpPost]
        public ActionResult DeleteUnidadDeTiempo(Int32 UnidadTiempoId)
        {
            try
            {
                UnidadTiempo unidadtiempo = context.UnidadTiempo.FirstOrDefault(x => x.UnidadTiempoId == UnidadTiempoId);
                if (unidadtiempo.EsActivo)
                {
                    unidadtiempo.EsActivo = false;
                    //Si estaba como activo ponemos a otro por defecto como activo
                    UnidadTiempo unidadtiempoNuevoActivo = context.UnidadTiempo.Where(x => x.Estado.Equals("ACT")).First();
                    if (unidadtiempoNuevoActivo != null)
                    {
                        unidadtiempoNuevoActivo.EsActivo = true;
                    }
                }
                unidadtiempo.Estado = ConstantHelpers.EstadoInactivo;

                context.UnidadTiempo.Where(x => x.Orden > unidadtiempo.Orden && x.Estado.Equals(ConstantHelpers.EstadoActivo)).ToList().ForEach(x => x.Orden = x.Orden - 1);


                context.Entry(unidadtiempo).State = System.Data.Entity.EntityState.Modified;
                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch { PostMessage(MessageType.Error); }
            return RedirectToAction("LstUnidadDeTiempo");
        }
    }
}
