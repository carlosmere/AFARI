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
    public class ItemsGenericosViewModel : BaseViewModel
    {
        public List<DatoEdificio> LstDatos { get; set; }
        public String Vista { get; set; }
        public Int32 EdificioId { get; set; }
        public String filtroNombre { get; set; }
        public String filtroTipo { get; set; }
        public String filtroDato { get; set; }
        public Int32? UnidadTiempoId { get; set; }
        public List<UnidadTiempo> LstUnidadTiempo { get; set; } = new List<UnidadTiempo>();
        public List<SelectListItem> LstAnios { get; set; } = new List<SelectListItem>();
        public Int32? Anio { get; set; }
        public Int32 MaxOrden { get; set; }
        public void fill(CargarDatosContext datacontext, String filtroTipo, String filtroNombre, String filtroDato, Int32? UnidadTiempoId, Int32 EdificioId, Int32? Anio)
        {
            baseFill(datacontext);
            this.EdificioId = EdificioId;
            this.filtroDato = filtroDato;
            this.filtroTipo = filtroTipo;
            this.filtroNombre = filtroNombre;
            this.UnidadTiempoId = UnidadTiempoId;
            this.Anio = Anio;

            if (!this.Anio.HasValue)
                this.Anio = DateTime.Now.Year;

            var anioMinimo = datacontext.context.UnidadTiempo.Min(x => x.Anio);
            var anioActual = DateTime.Now.Year;

            for (int i = 0; i <= anioActual - anioMinimo; i++)
            {
                var value = (anioActual - i).ToString();
                LstAnios.Add(new SelectListItem { Value = value, Text = value });
            }

            if (this.Anio.HasValue)
            {
                if (!String.IsNullOrEmpty(filtroTipo))
                {
                    if (filtroTipo.Contains("Crono") || filtroTipo.Contains("Equipo"))
                    {
                        LstDatos = datacontext.context.DatoEdificio.Where(X => X.EdificioId == EdificioId
                        && X.UnidadTiempo.Anio == this.Anio && X.Dato != X.Nombre).OrderBy(X => X.Orden).ToList();
                    }
                    else
                        LstDatos = datacontext.context.DatoEdificio.Where(X => X.EdificioId == EdificioId
                        && X.UnidadTiempo.Anio == this.Anio && X.Dato != X.Nombre).OrderByDescending(X => X.UnidadTiempo.Orden).ToList();
                }
                else
                {
                    LstDatos = datacontext.context.DatoEdificio.Where(X => X.EdificioId == EdificioId
                        && X.UnidadTiempo.Anio == this.Anio && X.Dato != X.Nombre).OrderByDescending(X => X.UnidadTiempo.Orden).ToList();
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(filtroTipo))
                {
                    if (filtroTipo.Contains("Crono") || filtroTipo.Contains("Equipo"))
                        LstDatos = datacontext.context.DatoEdificio.Where(X => X.EdificioId == EdificioId && X.Dato != X.Nombre).OrderBy(X => X.Orden).ToList();
                    else
                        LstDatos = datacontext.context.DatoEdificio.Where(X => X.EdificioId == EdificioId && X.Dato != X.Nombre).OrderByDescending(X => X.UnidadTiempo.Orden).ToList();
                }
                else
                {
                    LstDatos = datacontext.context.DatoEdificio.Where(X => X.EdificioId == EdificioId && X.Dato != X.Nombre).OrderByDescending(X => X.UnidadTiempo.Orden).ToList();
                }
            }
            LstDatos.AddRange(datacontext.context.DatoEdificio.Where(X => X.EdificioId == EdificioId && X.Dato != X.Nombre && X.UnidadTiempoId == null).ToList());

            MaxOrden = (LstDatos.Max(x => x.Orden) ?? 0) + 1;
            if (!String.IsNullOrWhiteSpace(filtroTipo)) LstDatos = LstDatos.Where(X => X.Tipo.Contains(filtroTipo)).ToList();
            if (!String.IsNullOrWhiteSpace(filtroNombre)) LstDatos = LstDatos.Where(X => X.Nombre.Contains(filtroNombre)).ToList();
            if (!String.IsNullOrWhiteSpace(filtroDato)) LstDatos = LstDatos.Where(X => X.Dato.Contains(filtroDato)).ToList();
            //if (UnidadTiempoId.HasValue) LstDatos = LstDatos.Where(X => (X.UnidadTiempoId ?? -1) == UnidadTiempoId.Value).ToList();
        }

    }
}