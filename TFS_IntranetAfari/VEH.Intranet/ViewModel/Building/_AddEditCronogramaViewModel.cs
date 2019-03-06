using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.Controllers;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Building
{
    public class _AddEditCronogramaViewModel :BaseViewModel
    {
        public Int32? CronogramaId { get; set; }
        public Int32 Orden { get; set; }
        public String Nombre { get; set; }
        public Int32? Anio { get; set; }
        public List<SelectListItem> LstAnios { get; set; } = new List<SelectListItem>();
        public void Fill(CargarDatosContext c, Int32 edificioId, Int32? cronogramaId, Int32? anio)
        {
            baseFill(c);
            this.EdificioId = edificioId;            
            this.CronogramaId = cronogramaId;
            this.Anio = anio ?? DateTime.Now.Year;
            try
            {
                this.Orden = c.context.Cronograma.Where(x => x.EdificioId == this.EdificioId && x.Anio == this.Anio).Max(x => x.Orden) + 1;
            }
            catch (Exception ex)
            {
                this.Orden = 1;
            }
            
            if (this.CronogramaId.HasValue)
            {
                var cronograma = c.context.Cronograma.FirstOrDefault( x => x.CronogramaId == this.CronogramaId);
                this.Orden = cronograma.Orden;
                this.Nombre = cronograma.Nombre;
                this.Anio = cronograma.Anio;
            }
            //var anioMinimo = c.context.UnidadTiempo.Min(x => x.Anio);
            var anioMinimo = 2017;
            var anioActual = DateTime.Now.Year;

            for (int i = 0; i <= anioActual - anioMinimo; i++)
            {
                var value = (anioActual - i).ToString();
                LstAnios.Add(new SelectListItem { Value = value, Text = value });
            }
        }
    }
}