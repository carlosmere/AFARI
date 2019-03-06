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
    public class LstCronogramaMantenimientoViewModel : BaseViewModel
    {
        public Int32 EdificioId { get; set; }
        public Int32? Anio { get; set; }
        public String NombreEdificio { get; set; }
        public List<Cronograma> LstCronograma { get; set; } = new List<Cronograma>();
        public List<SelectListItem> LstAnios { get; set; } = new List<SelectListItem>();
        public void Fill(CargarDatosContext c, Int32 edificioId, Int32? anio)
        {
            baseFill(c);
            this.EdificioId = edificioId;
            NombreEdificio = c.context.Edificio.FirstOrDefault(x => x.EdificioId == this.EdificioId).Nombre;
            this.Anio = anio ?? DateTime.Now.Year;
            LstCronograma = c.context.Cronograma.Where( x => x.EdificioId == this.EdificioId
            && x.Estado == ConstantHelpers.EstadoActivo && x.Anio == this.Anio).OrderBy( x => x.Orden).ToList();

            var anioMinimo = c.context.UnidadTiempo.Min(x => x.Anio);
            var anioActual = c.context.Cronograma.Where( x => x.Estado == ConstantHelpers.EstadoActivo).Max( x => x.Anio);//DateTime.Now.Year;

            for (int i = 0; i <= anioActual - anioMinimo; i++)
            {
                var value = (anioActual - i).ToString();
                LstAnios.Add(new SelectListItem { Value = value, Text = value });
            }
        }
    }
}