using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.Controllers;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Building
{
    public class LstEstadoCuentaBancarioViewModel : BaseViewModel
    {
        public Int32? Anio { get; set; }
        public Int32 EdiId { get; set; }
        public String NombreEdificio { get; set; }
        public List<SelectListItem> LstAnios { get; set; } = new List<SelectListItem>();
        public List<EstadoCuentaBancario> LstEstadoCuentaBancario { get; set; } = new List<EstadoCuentaBancario>();
        public void Fill(CargarDatosContext c, Int32? anio, Int32 edificioId)
        {
            baseFill(c);
            this.Anio = anio ?? DateTime.Now.Year;
            this.EdiId = edificioId;
            if (!this.Anio.HasValue)
                this.Anio = DateTime.Now.Year;
            NombreEdificio = c.context.Edificio.FirstOrDefault( x => x.EdificioId == this.EdiId).Nombre;
            var anioMinimo = c.context.UnidadTiempo.Min(x => x.Anio);
            var anioActual = DateTime.Now.Year;

            for (int i = 0; i <= anioActual - anioMinimo; i++)
            {
                var value = (anioActual - i).ToString();
                LstAnios.Add(new SelectListItem { Value = value, Text = value });
            }

            LstEstadoCuentaBancario = c.context.EstadoCuentaBancario.Where( x => x.EdificioId == EdiId && x.UnidadTiempo.Anio == this.Anio
            && x.Estado == ConstantHelpers.EstadoActivo).OrderByDescending( x => x.UnidadTiempoId).ToList();

        }
    }
}