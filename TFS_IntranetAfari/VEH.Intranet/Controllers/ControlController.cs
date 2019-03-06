using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.Filters;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Control;
using System.Data;
using System.IO;
using Excel;
using VEH.Intranet.ViewModel.Employee;
using VEH.Intranet.Logic;

namespace VEH.Intranet.Controllers
{
    [AppAuthorize(AppRol.Administrador)]
    public class ControlController : BaseController
    {
        // GET: Control
        public ActionResult LstPermisoEdifico()
        {
            var model = new LstPermisoEdificoViewModel();
            model.Fill(CargarDatosContext());
            return View(model);
        }
        public ActionResult AddEditUsuarioPermiso(Int32 UsuarioId)
        {
            var model = new AddEditUsuarioPermisoViewModel();
            model.Fill(CargarDatosContext(), UsuarioId);
            return View(model);
        }
        public ActionResult EliminarEdificioPermiso(Int32 UsuarioId,Int32 EdificioId)
        {
            try
            {
                var permiso = context.PermisoEdificio.FirstOrDefault(x => x.UsuarioId == UsuarioId && x.EdificioId == EdificioId);
                context.PermisoEdificio.Remove(permiso);
                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error);
            }
            return RedirectToAction("AddEditUsuarioPermiso", new { UsuarioId = UsuarioId });
        }
        public ActionResult AddEdificioPermiso(Int32 UsuarioId)
        {
            var model = new AddEdificioPermisoViewModel();
            model.Fill(CargarDatosContext(),UsuarioId);
            return View(model);
        }
        [HttpPost]
        public ActionResult AddEdificioPermiso(AddEdificioPermisoViewModel model)
        {
            try
            {
                var permiso = new PermisoEdificio();
                permiso.UsuarioId = model.UsuarioId;
                permiso.EdificioId = model.EdificioId;
                context.PermisoEdificio.Add(permiso);
                context.SaveChanges();

                PostMessage(MessageType.Success);
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error);
            }
            return RedirectToAction("AddEditUsuarioPermiso",new { UsuarioId = model.UsuarioId});
        }
        public ActionResult LstAnuncio()
        {
            var model = new LstAnuncioViewModel();
            model.Fill(CargarDatosContext());
            return View(model);
        }
        public ActionResult ActivarAnuncio(Int32 AnuncioId)
        {
            var anuncio = context.Anuncio.FirstOrDefault( x => x.AnuncioId == AnuncioId);
            if (anuncio.Estado == ConstantHelpers.EstadoActivo)
            {
                anuncio.Estado = ConstantHelpers.EstadoInactivo;
            }
            else
            {
                anuncio.Estado = ConstantHelpers.EstadoActivo;
            }
            context.SaveChanges();
            return RedirectToAction("LstAnuncio","Control");
        }
        public ActionResult DeleteAnuncio(Int32 AnuncioId)
        {
            var anuncio = context.Anuncio.FirstOrDefault(x => x.AnuncioId == AnuncioId);
            anuncio.Estado = ConstantHelpers.EstadoEliminado;

            context.SaveChanges();
            return RedirectToAction("LstAnuncio");
        }
        public ActionResult AddEditAnuncio(Int32? AnuncioId)
        {
            var model = new AddEditAnuncioViewModel();
            model.Fill(CargarDatosContext(), AnuncioId);
            return View(model);
        }
        [HttpPost]
        public ActionResult AddEditAnuncio(AddEditAnuncioViewModel model)
        {
            try
            {
                Anuncio item = null;
                if (model.AnuncioId.HasValue)
                {
                    item = context.Anuncio.FirstOrDefault(x => x.AnuncioId == model.AnuncioId);
                }
                else
                {
                    item = new Anuncio();
                    item.Estado = ConstantHelpers.EstadoActivo;
                    item.UsuarioId = Session.GetUsuarioId();
                    context.Anuncio.Add(item);
                }

                if (model.Archivo != null && model.Archivo.ContentLength != 0)
                {
                    string _rutaArchivoserv = Server.MapPath("~");
                    string _rutaArchivodir = _rutaArchivoserv + Path.Combine("/Resources/Files", "Anuncio");

                    string _nombrearc = Guid.NewGuid().ToString().Substring(0,5) + model.Archivo.FileName;
                    _rutaArchivodir = Path.Combine(_rutaArchivodir, _nombrearc);

                    item.Ruta = _nombrearc;

                    model.Archivo.SaveAs(_rutaArchivodir);
                }

                item.Nombre = model.Nombre;
                item.Url = model.Url;
                item.Prioridad = model.Prioridad;
                item.Descripcion = model.Descripcion;
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error);
                model.Fill(CargarDatosContext(), model.AnuncioId);
                return View(model);
            }
            PostMessage(MessageType.Success);
            return RedirectToAction("LstAnuncio");
        }
    }
}