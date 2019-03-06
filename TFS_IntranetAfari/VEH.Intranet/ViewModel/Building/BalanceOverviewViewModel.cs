using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Building
{
    public class BalanceOverviewViewModel : BaseViewModel
    {
        public Decimal Ingresos { get; set; }
        public Decimal Gastos { get; set; }
        public Decimal SaldoAnterior { get; set; }
        public Decimal Saldo { get; set; }
        public Decimal Acumulado { get; set; }
        public Int32 EdificioId { get; set; }
        public Int32? UnidadTiempoId { get; set; }
        public List<UnidadTiempo> LstUnidadTiempo { get; set; }
        public String NombreEdificio { get; set; }

        public void Fill(CargarDatosContext datacontext, Int32? EdificioId)
        {
            baseFill(datacontext);
            var Edificio = datacontext.context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);
            LstUnidadTiempo = datacontext.context.UnidadTiempo.Where(x => x.Estado == ConstantHelpers.EstadoActivo).OrderByDescending(x => x.Orden).ToList();
            if (Edificio != null)
            {
                NombreEdificio = Edificio.Nombre;
            }

        } 

    }
}