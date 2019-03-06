using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.ViewModel.News;
using VEH.Intranet.Models;
using VEH.Intranet.Helpers;
using VEH.Intranet.Filters;

namespace VEH.Intranet.Controllers
{
    [ViewParameter("Edificio", "fa fa-info-circle")]
    public class NewsController : BaseController
    {
        //
        // GET: /News/

        [AppAuthorize(AppRol.Administrador)]
        public ActionResult LstNoticiaAdm(Int32? np, Int32 EdificioId)
        {
            LstNoticiaViewModel ViewModel = new LstNoticiaViewModel();
            ViewModel.EdificioId = EdificioId;
            ViewModel.Fill(CargarDatosContext(), np);
            return View(ViewModel);
        }

        [AppAuthorize(AppRol.Propietario)]
        //[ViewParameter("Noticia", "fa fa-info")]
        public ActionResult LstNoticia(Int32? np)
        {
            LstNoticiaViewModel ViewModel = new LstNoticiaViewModel();
            ViewModel.EdificioId = SessionHelpers.GetEdificioId(Session);
            ViewModel.Fill(CargarDatosContext(), np);
            return View(ViewModel);
        }

        [AppAuthorize(AppRol.Administrador)]
        public ActionResult AddEditNoticia(Int32? NoticiaId, Int32 EdificioId)
        {
            AddEditNoticiaViewModel ViewModel = new AddEditNoticiaViewModel();
            ViewModel.EdificioId = EdificioId;
            ViewModel.NoticiaId = NoticiaId;
            ViewModel.Fill(CargarDatosContext());
            return View(ViewModel);
        }

        [AppAuthorize(AppRol.Administrador)]
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult AddEditNoticia(AddEditNoticiaViewModel ViewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewModel.EdificioId = ViewModel.EdificioId;
                ViewModel.Fill(CargarDatosContext());
                return View(ViewModel);
            }
            try
            {
                if (ViewModel.NoticiaId.HasValue)
                {
                    Noticia noticia = context.Noticia.FirstOrDefault(x => x.NoticiaId == ViewModel.NoticiaId.Value);
                    noticia.Titulo = ViewModel.Titulo;
                    noticia.Sumilla = ViewModel.Sumilla;
                    noticia.Detalle = ViewModel.Detalle;
                    context.Entry(noticia).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    Noticia noticia = new Noticia();
                    noticia.Titulo = ViewModel.Titulo;
                    noticia.Sumilla = ViewModel.Sumilla;
                    noticia.Detalle = ViewModel.Detalle;
                    noticia.Estado = ConstantHelpers.EstadoActivo;
                    noticia.EdificioId = ViewModel.EdificioId;
                    noticia.Fecha = DateTime.Now;
                    context.Noticia.Add(noticia);
                }
                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch { PostMessage(MessageType.Error); }
            return RedirectToAction("LstNoticiaAdm", new { EdificioId = ViewModel.EdificioId });
        }

        [AppAuthorize(AppRol.Administrador)]
        public ActionResult _DeleteNoticia(Int32 NoticiaId)
        {
            ViewBag.NoticiaId = NoticiaId;
            return PartialView();
        }

        [AppAuthorize(AppRol.Administrador)]
        [HttpPost]
        public ActionResult DeleteNoticia(Int32 NoticiaId)
        {
            try
            {
                Noticia Noticia = context.Noticia.FirstOrDefault(x => x.NoticiaId == NoticiaId);
                Noticia.Estado = ConstantHelpers.EstadoInactivo;
                context.Entry(Noticia).State = System.Data.Entity.EntityState.Modified;
                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch { PostMessage(MessageType.Error); }
            return RedirectToAction("LstNoticia");
        }

        [ViewParameter(PageIcon: "fa fa-bookmark")]
        [AppAuthorize(AppRol.Propietario)]
        public ActionResult _DetailNoticia(Int32 NoticiaId)
        {
            ViewBag.Detalle = context.Noticia.FirstOrDefault(x=>x.NoticiaId == NoticiaId).Detalle;
            return PartialView();
        }
    }
}
