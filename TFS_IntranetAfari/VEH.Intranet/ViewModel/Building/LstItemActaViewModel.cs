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
    public class LstItemActaViewModel : BaseViewModel
    {
        public Int32? EdificioId { get; set; }
        public List<Edificio> LstEdificio { get; set; } = new List<Edificio>();
        public List<ItemActa> LstItem { get; set; } = new List<ItemActa>();
        public Int32? Anio { get; set; } = DateTime.Now.Year;
        public List<SelectListItem> LstAnios { get; set; } = new List<SelectListItem>();
        public void Fill(CargarDatosContext c, Int32? anio, Int32? edificioId)
        {
            baseFill(c);
            this.Anio = anio ?? DateTime.Now.Year;
            this.EdificioId = edificioId;

            var query = c.context.ItemActa.Where(x => x.UnidadTiempo.Anio == this.Anio && x.Estado == ConstantHelpers.EstadoActivo).AsQueryable();

            if (Anio.HasValue)
            {
                query = query.Where(x => x.UnidadTiempo.Anio == Anio);
            }
            if (EdificioId.HasValue)
            {
                query = query.Where(x => x.EdificioId == EdificioId);
            }
            LstEdificio = c.context.Edificio.Where(x => x.Estado == ConstantHelpers.EstadoActivo).ToList();

            var anioMinimo = c.context.UnidadTiempo.Where(x => x.Estado == ConstantHelpers.EstadoActivo).Min(x => x.Anio);
            var anioActual = c.context.UnidadTiempo.Where(x => x.Estado == ConstantHelpers.EstadoActivo).Max(x => x.Anio);

            for (int i = 0; i <= anioActual - anioMinimo; i++)
            {
                var value = (anioActual - i).ToString();
                LstAnios.Add(new SelectListItem { Value = value, Text = value });
            }
            LstItem = query.OrderBy(x => x.Nombre).ToList();
        }
    }
}