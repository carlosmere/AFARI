using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Staff
{
    public class LstPlanillaViewModel : BaseViewModel
    {
        public Int32? np { get; set; }
        public Int32 TrabajadorId { get; set; }
        public Trabajador Trabajador { get; set; }
        public Int32 EdificioId { get; set; }
        public Edificio Edificio { get; set; }
        public IPagedList<Planilla> LstPlanilla { get; set; }

        public LstPlanillaViewModel() { }

        public void Fill(CargarDatosContext datacontext, Int32? _np)
        {
            baseFill(datacontext);
            np = _np ?? 1;
            Trabajador = datacontext.context.Trabajador.FirstOrDefault(x => x.TrabajadorId == TrabajadorId);
            Edificio = datacontext.context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);
            var query = datacontext.context.Planilla.OrderByDescending(x => x.UnidadTiempo.Descripcion).Where(x=>x.TrabajadorId == TrabajadorId).AsQueryable();
            LstPlanilla = query.ToPagedList(np.Value, ConstantHelpers.DEFAULT_PAGE_SIZE);
        }
    }
}