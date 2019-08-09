using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;

namespace VEH.Intranet.Controllers
{
    public class VisitaxEdificio
    {
        public String TipoInmueble { get; set; }
        public String NombreDepartamento { get; set; }
        public String NombreUsuario { get; set; }
        public Int32 CantWeb { get; set; }
        public Int32 CantApp { get; set; }
    }
    public class JsonController : BaseController
    {
        // GET: Json
        public JsonResult GetVisitasPorEdificio(String Edificio, Int32? Anio)
        {
            var LstVisitas = new List<VisitaxEdificio>();
            var edificioId = Edificio.Split('-')[0].ToInteger();
            var edificio = context.Edificio.FirstOrDefault( x => x.EdificioId == edificioId);
            if (edificio != null)
            {
                var queryAuxVisitas = context.Visita.Where(x => x.EdificioId == edificio.EdificioId).AsQueryable();

                if (Anio.HasValue)
                {
                    queryAuxVisitas = queryAuxVisitas.Where( x => x.Fecha.Year == Anio);
                }

                var auxVisitas = queryAuxVisitas.ToList();
                var auxUsuario = auxVisitas.Select( x => x.UsuarioId).ToList();
                var lstUsuario = context.Usuario.Where(x => x.Departamento.EdificioId == edificio.EdificioId && x.Estado == ConstantHelpers.EstadoActivo && auxUsuario.Contains(x.UsuarioId)).ToList();
                foreach (var usuario in lstUsuario)
                {
                    var lstVisita = auxVisitas.Where(x => x.UsuarioId == usuario.UsuarioId).ToList();
                    var cantWeb = lstVisita.Count(x => x.Tipo == "WEB");
                    var cantApp = lstVisita.Count(x => x.Tipo == "APP");
                    if (cantWeb == 0 && cantApp == 0)
                    {
                        continue;
                    }
                    LstVisitas.Add(new VisitaxEdificio {
                        NombreUsuario = usuario.Nombres + " " + usuario.Apellidos,
                        NombreDepartamento = usuario.Departamento.Numero,
                        TipoInmueble = usuario.Departamento.TipoInmueble.Acronimo,
                        CantApp = cantApp,
                        CantWeb = cantWeb
                    });
                }
            }
            return Json(LstVisitas);
        }
    }
}