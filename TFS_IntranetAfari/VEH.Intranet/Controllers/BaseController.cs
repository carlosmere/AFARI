using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.Models;
using VEH.Intranet.Helpers;

namespace VEH.Intranet.Controllers
{
    public class BaseController : Controller
    {
        public SIVEHEntities context;
        private CargarDatosContext cargarDatosContext;

        public BaseController()
        {
            context = new SIVEHEntities();

        }

        public void InvalidarContext()
        {
            context = new SIVEHEntities();
        }

        public CargarDatosContext CargarDatosContext()
        {
            if (cargarDatosContext == null)
            {
                cargarDatosContext = new CargarDatosContext { context = context, session = Session };
            }

            return cargarDatosContext;
        }

        public void PostMessage(FlashMessage Message)
        {
            if (TempData["FlashMessages"] == null)
                TempData["FlashMessages"] = new List<FlashMessage>();

            ((List<FlashMessage>)TempData["FlashMessages"]).Add(Message);
        }

        public void PostMessage(MessageType Type)
        {
            String Body = "";

            switch (Type)
            {
                case MessageType.Error: Body = "Ha ocurrido un error al procesar la solicitud."; break;
                case MessageType.Info: Body = ""; break;
                case MessageType.Success: Body = "Los datos se guardaron exitosamente."; break;
                case MessageType.Warning: Body = "."; break;
                case MessageType.AExist: Body = "Ya existen los datos."; break;
            }
            PostMessage(Type, Body);
        }

        public void PostMessage(MessageType Type, String Title, String Body)
        {
            PostMessage(new FlashMessage { Title = Title, Body = Body, Type = Type });
        }

        public void PostMessage(MessageType Type, String Body)
        {
            String Title = "";

            if (Type == MessageType.AExist)
                Type = MessageType.Warning;
            switch (Type)
            {
                case MessageType.Error: Title = "¡Error!"; break;
                case MessageType.Info: Title = "Ojo."; break;
                case MessageType.Success: Title = "¡Éxito!"; break;
                case MessageType.Warning: Title = "¡Atención!"; break;
            }

            PostMessage(new FlashMessage { Title = Title, Body = Body, Type = Type });
        }

        public ActionResult Error(Exception ex)
        {
            return View("Error", ex);
        }

        public ActionResult RedirectToActionPartialView(String actionName)
        {
            return RedirectToActionPartialView(actionName, null, null);
        }

        public ActionResult RedirectToActionPartialView(String actionName, object routeValues)
        {
            return RedirectToActionPartialView(actionName, null, routeValues);
        }

        public ActionResult RedirectToActionPartialView(String actionName, String controllerName)
        {
            return RedirectToActionPartialView(actionName, controllerName, null);
        }

        public ActionResult RedirectToActionPartialView(String actionName, String controllerName, object routeValues)
        {
            var url = Url.Action(actionName, controllerName, routeValues);
            return Content("<script> window.location = '" + url + "'</script>");
        }
    }

    public class CargarDatosContext
    {
        public SIVEHEntities context { get; set; }
        public HttpSessionStateBase session { get; set; }
    }
}
