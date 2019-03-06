using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Models;
using PagedList;
using VEH.Intranet.Controllers;
using VEH.Intranet.Helpers;
using System.Data.Entity;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Spending
{
    public class LstDetalleGastoViewModel : BaseViewModel
    {
        public Int32? np { get; set; }
        public Int32 EdificioId { get; set; }
        public Edificio Edificio { get; set; }
        public String DescripcionUnidadMedida { get; set; }
        public Int32? GastoId { get; set; }

        public List<DetalleGasto> LstDetalleGasto { get; set; }

        public LstDetalleGastoViewModel() { }

        public void Fill(CargarDatosContext datacontext, Int32? _np)
        {
            baseFill(datacontext);
            np = _np ?? 1;

            Edificio = datacontext.context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);
            var query = datacontext.context.DetalleGasto
                .Include(x => x.Gasto)
                .Include(x => x.Gasto.Edificio)
                .OrderByDescending(x => x.FechaRegistro)
                .Where(x => x.Estado == ConstantHelpers.EstadoActivo && x.Gasto.Edificio.EdificioId == EdificioId)
                .AsQueryable();
            if (GastoId.HasValue)
            {
                DescripcionUnidadMedida = datacontext.context.Gasto.FirstOrDefault(x => x.GastoId == GastoId).UnidadTiempo.Descripcion.ToUpper();
                query = query.Where(x => x.GastoId == GastoId.Value);
            }
            int ContOrden = query.Count(x => x.Orden.HasValue == false);

            if (ContOrden == 0)
                LstDetalleGasto = query.OrderBy(x => x.Orden).ToList();
            else
                LstDetalleGasto = query.ToList();//(np.Value, ConstantHelpers.DEFAULT_PAGE_SIZE);
        }
    }
}