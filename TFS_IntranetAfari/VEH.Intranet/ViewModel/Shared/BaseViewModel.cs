using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
namespace VEH.Intranet.ViewModel.Shared
{
    public class BaseViewModel
    {
        public List<MenuPropietarioEdificio> elementosMenu { get; set; }
        public Boolean ObligacionesLaborales { get; set; }
        public Boolean Equipos { get; set; }
        public Int32 EdificioId { get; set; }
        public List<Anuncio> LstAnuncio { get; set; } = new List<Anuncio>();
        public void baseFill(CargarDatosContext datacontext)
        {
            EdificioId = datacontext.session.GetEdificioId();
            elementosMenu = datacontext.context.MenuPropietarioEdificio.Where(X => X.EdificioId ==EdificioId  && X.Estado=="ACT").ToList();
            ObligacionesLaborales = datacontext.context.DatoEdificio.Any(X => X.Tipo.Contains("Obligaciones") && X.EdificioId == EdificioId);
            Equipos = datacontext.context.DatoEdificio.Any(X => X.Tipo.Contains("Equipo") && X.EdificioId == EdificioId);
            LstAnuncio = datacontext.context.Anuncio.Where( x => x.Estado == ConstantHelpers.EstadoActivo).OrderByDescending( x => x.Prioridad).ToList();
        }
    }
}