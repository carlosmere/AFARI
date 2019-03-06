using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Fee
{
    public class LstCuotaViewModel : BaseViewModel
    {
        public Int32 EdificioId { get; set; }
        public Int32 DepartamentoId { get; set; }
        public Int32? np { get; set; }
        public Departamento Departamento { get; set; }
        public Edificio Edificio { get; set; }

        public IPagedList<Cuota> LstCuota { get; set; }

        public void Fill(CargarDatosContext datacontext, Int32? _np)
        {
            baseFill(datacontext);
            np = _np ?? 1;

            Departamento = datacontext.context.Departamento.FirstOrDefault(x => x.DepartamentoId == DepartamentoId);
            Edificio = datacontext.context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);

            var query = datacontext.context.Cuota.OrderBy(x => x.FechaRegistro).Where(x=>x.DepartamentoId == DepartamentoId).AsQueryable();
            LstCuota = query.ToPagedList(np.Value, ConstantHelpers.DEFAULT_PAGE_SIZE);
        }
    }
}