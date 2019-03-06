﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Models;
using PagedList;
using VEH.Intranet.Controllers;
using VEH.Intranet.Helpers;
using System.Data.Entity;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Income
{
    public class LstDetalleIngresoViewModel : BaseViewModel
    {
        public Int32? np { get; set; }
        public Int32 EdificioId { get; set; }
        public Edificio Edificio { get; set; }
        public String DescripcionUnidadMedida { get; set; }
        public Int32? IngresoId { get; set; }

        public IPagedList<DetalleIngreso> LstDetalleIngreso { get; set; }

        public LstDetalleIngresoViewModel() { }

        public void Fill(CargarDatosContext datacontext, Int32? _np)
        {
            baseFill(datacontext);
            np = _np ?? 1;

            Edificio = datacontext.context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);
            var query = datacontext.context.DetalleIngreso
                .Include(x => x.Ingreso)
                .Include(x => x.Ingreso.Edificio)
                .OrderByDescending(x => x.FechaRegistro)
                .Where(x => x.Estado == ConstantHelpers.EstadoActivo && x.Ingreso.Edificio.EdificioId == EdificioId)
                .AsQueryable();
            if (IngresoId.HasValue)
            {
                DescripcionUnidadMedida = datacontext.context.Ingreso.FirstOrDefault(x => x.IngresoId == IngresoId).UnidadTiempo.Descripcion.ToUpper();
                query = query.Where(x => x.IngresoId == IngresoId.Value);
            }
            LstDetalleIngreso = query.ToPagedList(np.Value, ConstantHelpers.DEFAULT_PAGE_SIZE);
        }
    }
}