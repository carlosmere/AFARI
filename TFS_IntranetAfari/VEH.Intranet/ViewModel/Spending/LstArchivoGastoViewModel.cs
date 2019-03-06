using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using System.Data.Entity;
using VEH.Intranet.ViewModel.Shared;
using System.Web.Mvc;

namespace VEH.Intranet.ViewModel.Spending
{
    public class LstArchivoGastoViewModel : BaseViewModel
    {
        public Int32? np { get; set; }
        public Int32 EdificioId { get; set; }
        public Edificio Edificio { get; set; }
        public String DescripcionUnidadMedida { get; set; }
        public Int32? GastoId { get; set; }
        public Int32? Anio { get; set; }
        public List<SelectListItem> LstAnios { get; set; }

        public IPagedList<ArchivoGasto> LstArchivoGasto { get; set; }

        public LstArchivoGastoViewModel() { LstAnios = new List<SelectListItem>(); }

        public void Fill(CargarDatosContext datacontext, Int32? _np,Int32? anio)
        {
            baseFill(datacontext);
            np = _np ?? 1;
            this.Anio = anio;

            if (!this.Anio.HasValue)
                this.Anio = DateTime.Now.Year;

            var anioMinimo = datacontext.context.UnidadTiempo.Min(x => x.Anio);
            var anioActual = DateTime.Now.Year;

            for (int i = 0; i <= anioActual - anioMinimo; i++)
            {
                var value = (anioActual - i).ToString();
                LstAnios.Add(new SelectListItem { Value = value, Text = value });
            }

            Edificio = datacontext.context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);
            Boolean EsAdmin = datacontext.session.GetRol().Value== AppRol.Administrador;
            var query = datacontext.context.ArchivoGasto
                .Include(x => x.Gasto)
                .Include(x => x.Gasto.Edificio)
                .OrderByDescending(x => x.FechaRegistro)
                .Where(x => x.Estado == ConstantHelpers.EstadoActivo && x.Gasto.Edificio.EdificioId == EdificioId)
                //&& (EsAdmin || (x.FechaRegistro.Year < DateTime.Now.Year ||( x.FechaRegistro.Year== DateTime.Now.Year && x.FechaRegistro.Month<DateTime.Now.Month))))
                .AsQueryable();

            if (this.Anio.HasValue)
                query = query.Where( x => x.Gasto.UnidadTiempo.Anio == this.Anio);

            if (GastoId.HasValue)
            {
                DescripcionUnidadMedida = datacontext.context.Gasto.FirstOrDefault(x => x.GastoId == GastoId.Value).UnidadTiempo.Descripcion.ToUpper();
                query = query.Where(x => x.GastoId == GastoId.Value);
            }
            LstArchivoGasto = query.ToPagedList(np.Value, ConstantHelpers.DEFAULT_PAGE_SIZE);
        }
    }
}