using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.Controllers;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using System.Data.Entity;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Employee
{
    public class LstTrabajadorViewModel : BaseViewModel
    {
        public Int32? np { get; set; }
        public Int32? EdificioId { get; set; }
        public List<Trabajador> LstTrabajador { get; set; }
        public List<SelectListItem> LstComboEdificio { get; set; }
        public Edificio Edificio { get; set; }
        public LstTrabajadorViewModel() { LstComboEdificio = new List<SelectListItem>(); }

        public void Fill(CargarDatosContext datacontext, Int32? _np)
        {
            baseFill(datacontext);
            np = _np ?? 1;

            var edificios = datacontext.context.Edificio.OrderBy(x => x.Nombre).Where(x => x.Estado == ConstantHelpers.EstadoActivo);
            foreach (var item in edificios)
                LstComboEdificio.Add(new SelectListItem { Value = item.EdificioId.ToString(), Text = item.Nombre });

            var query = datacontext.context.Trabajador
                .Include(x => x.Edificio)
                .OrderBy(x => x.Nombres)
                .OrderBy(x => x.Apellidos)
                .Where(x => x.Estado == ConstantHelpers.EstadoActivo && x.Edificio.Estado == ConstantHelpers.EstadoActivo)
                .AsQueryable();

            if (EdificioId.HasValue)
            {
                Edificio = datacontext.context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId.Value);
                query = query.Where(x => x.EdificioId == EdificioId.Value);
            }
            LstTrabajador = query.ToList();// (np.Value, ConstantHelpers.DEFAULT_PAGE_SIZE);
        }
    }

    public class LstTrabajadorArchivoViewModel : BaseViewModel
    {
        public void fill( CargarDatosContext datacontext)
        {
            baseFill(datacontext);
        }
        public Int32 Pestania { get; set; }
        public LstTrabajadorViewModel LstTrabajador { get; set; }
        public LstArchivoTrabajadorViewModel LstArchivo { get; set; }
    }
}