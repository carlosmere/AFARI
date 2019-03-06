using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.Controllers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.External
{
    public class LstEquiposSinCertificadoViewModel : BaseViewModel
    {
        public List<DatoEdificio> LstDatos { get; set; } = new List<DatoEdificio>();
        public List<SelectListItem> LstAnios { get; set; } = new List<SelectListItem>();
        public Int32? Anio { get; set; }
        public Int32 EdifiId { get; set; }
        public Int32 MaxOrden { get; set; }
        public void Fill(CargarDatosContext c, Int32 edificioId, Int32? anio)
        {
            baseFill(c);
            EdifiId = edificioId;
            Anio = anio;

            if (!this.Anio.HasValue)
                this.Anio = DateTime.Now.Year;

            var anioMinimo = c.context.UnidadTiempo.Min(x => x.Anio);
            var anioActual = DateTime.Now.Year;

            for (int i = 0; i <= anioActual - anioMinimo; i++)
            {
                var value = (anioActual - i).ToString();
                LstAnios.Add(new SelectListItem { Value = value, Text = value });
            }

            if (this.Anio.HasValue)
                LstDatos = c.context.DatoEdificio.Where(X => X.EdificioId == EdifiId
                && X.UnidadTiempo.Anio == this.Anio && X.Dato == X.Nombre).OrderBy(X => X.UnidadTiempo.Orden).ToList();
            else
                LstDatos = c.context.DatoEdificio.Where(X => X.EdificioId == EdifiId && X.Dato == X.Nombre).OrderBy(X => X.UnidadTiempo.Orden).ToList();

            MaxOrden = (LstDatos.Max(x => x.Orden) ?? 0) + 1;
        }
    }
}