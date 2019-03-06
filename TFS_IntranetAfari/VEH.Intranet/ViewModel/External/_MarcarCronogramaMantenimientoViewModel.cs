using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.Controllers;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.External
{
    public class _MarcarCronogramaMantenimientoViewModel : BaseViewModel
    {
        public Int32 EdificioId { get; set; }
        public List<DatoEdificio> LstDatos { get; set; } = new List<DatoEdificio>();
        public List<String> LstEquipos { get; set; } = new List<String>();
        public Int32? Anio { get; set; }
        public String LstMarcados { get; set; }
        public void Fill(CargarDatosContext datacontext, Int32 EdificioId, Int32? Anio)
        {
            baseFill(datacontext);
            this.EdificioId = EdificioId;

            var anioMinimo = datacontext.context.UnidadTiempo.Min(x => x.Anio);
            var anioActual = DateTime.Now.Year;

            this.Anio = Anio ?? anioActual;

            if (this.Anio.HasValue)
            {
                LstEquipos = datacontext.context.DatoEdificio.Where(X => X.EdificioId == EdificioId && X.Tipo.Contains("Equipo") && X.AplicaMantenimiento == true && X.UnidadTiempo.Anio == this.Anio).OrderBy( X => X.Orden).ToList().Select(X => X.Tipo).Distinct().ToList();
                LstDatos = datacontext.context.DatoEdificio.Where(X => X.EdificioId == EdificioId && X.Tipo.Contains("Crono") && X.UnidadTiempo.Anio == this.Anio).OrderBy( x => x.Orden).ToList();
            }
        }
    }
}