﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Models;
using PagedList;
using VEH.Intranet.Helpers;
using System.Web.Mvc;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Spending
{
    public class LstGastoViewModel : BaseViewModel
    {
        public Int32 EdificioId { get; set; }
        public Int32 DepartamentoId { get; set; }
        public Int32? np { get; set; }
        public Departamento Departamento { get; set; }
        public Edificio Edificio { get; set; }
        public Int32? Anio { get; set; }
        public List<SelectListItem> LstAnios { get; set; }
        public IPagedList<Gasto> LstGasto { get; set; }

        public LstGastoViewModel() { LstAnios = new List<SelectListItem>(); }
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

            //var query = datacontext.context.Gasto.OrderBy(x => x.FechaRegistro).Where(x => x.Estado == ConstantHelpers.EstadoActivo).AsQueryable();
            var query = datacontext.context.Gasto.OrderByDescending(x => x.FechaRegistro).Where(x => x.Estado == ConstantHelpers.EstadoActivo && x.EdificioId == Edificio.EdificioId).AsQueryable();
            if (this.Anio.HasValue)
                query = query.Where(x => x.UnidadTiempo.Anio == this.Anio);

            LstGasto = query.ToPagedList(np.Value, ConstantHelpers.DEFAULT_PAGE_SIZE);
        }
    }

    public class LstDetalleArchivoGastoViewModel :BaseViewModel 
    {
        public LstArchivoGastoViewModel LstArchivo { get; set; }
        public LstDetalleGastoViewModel LstDetalle { get; set; }
        public Int32 Pestania { get; set; }
        public Int32? GastoId { get; set; }
        public List<SelectListItem> LstGasto { get; set; }
        public Int32 EdificioId { get; set; }
        public Int32 UnidadTiempoId { get; set; }
        public List<SelectListItem> LstAnio { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> LstMeses { get; set; } = new List<SelectListItem>();
        public Int32? Anio { get; set; }
        public Int32? Mes { get; set; }
        public LstDetalleArchivoGastoViewModel() { }

        public void Fill(CargarDatosContext datacontext, Int32 EdificioId)
        {
            var lstunidadtiempo = datacontext.context.Gasto.Where(x => x.EdificioId == EdificioId && x.Estado == ConstantHelpers.EstadoActivo).OrderByDescending( x => x.UnidadTiempo.Orden).ToList();
            List<Int32> valAnio = new List<int>();
            List<Int32> valMes = new List<int>();
            //var mesActivo = lstunidadtiempo.FirstOrDefault(x => x.EsActivo).Mes;
            foreach (var item in lstunidadtiempo)
            {
                if (item.UnidadTiempo.EsActivo)
                {
                    continue;
                }
                var mes = item.UnidadTiempo.Descripcion.Substring(0, item.UnidadTiempo.Descripcion.Length - 4);

                if (!valAnio.Contains(item.UnidadTiempo.Anio))
                {
                    LstAnio.Add(new SelectListItem { Value = item.UnidadTiempo.Anio.ToString(), Text = item.UnidadTiempo.Anio.ToString() });
                    valAnio.Add(item.UnidadTiempo.Anio);
                }
                if (!valMes.Contains(item.UnidadTiempo.Mes))
                {
                    LstMeses.Add(new SelectListItem { Value = item.UnidadTiempo.Mes.ToString(), Text = mes });
                    valMes.Add(item.UnidadTiempo.Mes);
                }
            }

            baseFill(datacontext);
            LstGasto = new List<SelectListItem>();
            var gastos = datacontext.context.Gasto.Where(x => x.EdificioId == EdificioId && x.Estado == ConstantHelpers.EstadoActivo);
            if (gastos != null && gastos.Count() > 0)
                foreach (var item in gastos)
                    LstGasto.Add(new SelectListItem { Value = item.GastoId.ToString(), Text = item.UnidadTiempo.Descripcion.ToUpper() });
            this.EdificioId = EdificioId;
        }
    }

}