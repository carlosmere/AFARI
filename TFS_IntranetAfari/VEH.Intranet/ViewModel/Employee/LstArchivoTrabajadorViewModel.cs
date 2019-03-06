using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using System.Web.Mvc;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Employee
{
    public class LstArchivoTrabajadorViewModel : BaseViewModel
    {
        public Int32? np { get; set; }
        public Int32 EdificioId { get; set; }
        public Edificio Edificio { get; set; }
        public IPagedList<ArchivoTrabajador> LstArchivoTrabajador { get; set; }

        public void Fill(CargarDatosContext datacontext, Int32? _np)
        {
            baseFill(datacontext);
            np = _np ?? 1;
            Edificio = datacontext.context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);

            var query = datacontext.context.ArchivoTrabajador.OrderByDescending(x => x.FechaRegistro).Where(x => x.Estado == ConstantHelpers.EstadoActivo && x.EdificioId == EdificioId).AsQueryable();
            LstArchivoTrabajador = query.ToPagedList(np.Value, ConstantHelpers.DEFAULT_PAGE_SIZE);
        }
    }
}