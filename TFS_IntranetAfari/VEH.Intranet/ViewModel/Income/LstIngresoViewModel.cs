using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Models;
using PagedList;
using VEH.Intranet.Helpers;
using System.Web.Mvc;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Income
{
    public class LstIngresoViewModel : BaseViewModel
    {
        public Int32 EdificioId { get; set; }
        public Int32 DepartamentoId { get; set; }
        public Int32? np { get; set; }
        public Departamento Departamento { get; set; }
        public Edificio Edificio { get; set; }
        public IPagedList<Ingreso> LstIngreso { get; set; }
        public Int32? Anio { get; set; }
        public List<SelectListItem> LstAnios { get; set; }
        public LstIngresoViewModel() { LstAnios = new List<SelectListItem>(); }
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

            Departamento = datacontext.context.Departamento.FirstOrDefault(x => x.DepartamentoId == DepartamentoId);
            Edificio = datacontext.context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);

            //var query = datacontext.context.Ingreso.OrderBy(x => x.FechaRegistro).Where(x => x.Estado == ConstantHelpers.EstadoActivo).AsQueryable();
            var query = datacontext.context.Ingreso.OrderByDescending(x => x.FechaRegistro).Where(x => x.Estado == ConstantHelpers.EstadoActivo && x.EdificioId == Edificio.EdificioId).AsQueryable();
            if (this.Anio.HasValue)
                query = query.Where(x => x.UnidadTiempo.Anio == this.Anio);

            LstIngreso = query.ToPagedList(np.Value, ConstantHelpers.DEFAULT_PAGE_SIZE);
        }
    }

    public class LstDetalleArchivoIngresoViewModel
    {
        public LstArchivoIngresoViewModel LstArchivo { get; set; }
        public LstDetalleIngresoViewModel LstDetalle { get; set; }
        public Int32 Pestania { get; set; }
        public Int32? IngresoId { get; set; }
        public List<SelectListItem> LstIngreso { get; set; }

        public LstDetalleArchivoIngresoViewModel() { }

        public void Fill(CargarDatosContext datacontext, Int32 EdificioId)
        {
            LstIngreso = new List<SelectListItem>();
            var ingresos = datacontext.context.Ingreso.Where(x => x.EdificioId == EdificioId && x.Estado == ConstantHelpers.EstadoActivo);
            if (ingresos != null && ingresos.Count() > 0)
                foreach (var item in ingresos)
                    LstIngreso.Add(new SelectListItem { Value = item.IngresoId.ToString(), Text = item.UnidadTiempo.Descripcion.ToUpper() });
        }
    }

}