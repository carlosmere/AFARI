using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Helpers;
using System.Web.Mvc;
using System.Web.Routing;
using VEH.Intranet.Logic;

namespace VEH.Intranet.Filters
{
    public enum PermisoInsuficienteResultado
    {
        Modal,
        Vista
    }

    #region Templates

    public class PermisoAccesoDocumentoVisitaAttribute : ActionFilterAttribute
        {
            private String _parametero;
            private PermisoInsuficienteResultado _vistaPermisoInsuficiente;
            private PermisoAccesoLogic.TipoPermiso _tipoPermiso;

            public PermisoAccesoDocumentoVisitaAttribute(String parametero, PermisoAccesoLogic.TipoPermiso tipoPermiso, PermisoInsuficienteResultado vistaPermisoInsuficiente = PermisoInsuficienteResultado.Vista)
            {
                _parametero = parametero;
                _tipoPermiso = tipoPermiso;
                _vistaPermisoInsuficiente = vistaPermisoInsuficiente;
            }

            public override void OnActionExecuting(ActionExecutingContext filterContext)
            {
                /*var authorized = false;

                try
                {
                    var usuarioId = filterContext.HttpContext.Session.GetUsuarioId();
                    var documentoVisitaId = filterContext.RequestContext.HttpContext.Request.Unvalidated[_parametero].ToInteger();

                    var permisoLogic = new PermisoAccesoLogic();
                    var permiso = permisoLogic.GetPermisoDocumentoVisita(usuarioId, documentoVisitaId);

                    if (permisoLogic.TienePermisoSuficiente(permiso, _tipoPermiso))
                        authorized = true;
                    
                    if (documentoVisitaId == 0)
                        authorized = true;
                }
                catch (Exception ex)
                {
                    authorized = false;
                }

                if (!authorized)
                {
                    var viewName = "";
                    switch (_vistaPermisoInsuficiente)
                    {
                        case PermisoInsuficienteResultado.Vista: viewName = "PermisoInsuficiente"; break;
                        case PermisoInsuficienteResultado.Modal: viewName = "_PermisoInsuficienteModal"; break;
                    }

                    filterContext.Result = new ViewResult() { ViewName = viewName };
                }*/
            }
        } 
  
    #endregion
}