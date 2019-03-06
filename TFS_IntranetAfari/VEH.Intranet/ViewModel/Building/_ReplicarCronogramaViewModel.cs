using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.Controllers;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Building
{
    public class _ReplicarCronogramaViewModel : BaseViewModel
    {
        public Int32 AnioDestino { get; set; }
        public Int32? Anio { get; set; }
        public List<SelectListItem> LstAnios { get; set; } = new List<SelectListItem>();
        public Boolean Error { get; set; } = false;
        public void Fill(CargarDatosContext c, Int32 edificioId, Int32? anio)
        {
            baseFill(c);
            this.EdificioId = edificioId;        
            this.AnioDestino = anio.Value;

            try
            {
                var anioMinimo = c.context.Cronograma.Where(x => x.EdificioId == this.EdificioId).Min(x => x.Anio);
                var anioActual = c.context.Cronograma.Where(x => x.EdificioId == this.EdificioId).Max(x => x.Anio);

                for (int i = anioMinimo; i <= anioActual; i++)
                {
                    var value = (i).ToString();
                    LstAnios.Add(new SelectListItem { Value = value, Text = value });
                }
            }
            catch (Exception)
            {
                Error = true;
            }
        }
    }
}