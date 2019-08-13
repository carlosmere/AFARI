using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.CorretajeInmobiliario
{
    public class LstVisitasViewModel : BaseViewModel
    {
        public IPagedList<VisitaCorretaje> LstVisita { get; set; }
        public Int32? np { get; set; }
        public String Fecha { get; set; }
        public String NombreCliente { get; set; }
        public void Fill(CargarDatosContext datacontext, Int32? np, String fecha, String nombreCliente)
        {
            this.Fecha = fecha;
            baseFill(datacontext);
            this.np = np ?? 1;
            this.NombreCliente = nombreCliente;
            var query = datacontext.context.VisitaCorretaje.Where(x => x.Estado == ConstantHelpers.EstadoActivo).OrderBy(x => x.Fecha).AsQueryable();

            if (!String.IsNullOrEmpty(this.Fecha))
            {
                var dtFecha = this.Fecha.ToDateTime();
                query = query.Where(x => x.Fecha.Day == dtFecha.Day && x.Fecha.Month == dtFecha.Month && x.Fecha.Year == dtFecha.Year);
            }
            if (!String.IsNullOrEmpty(this.NombreCliente))
            {
                query = query.Where(x => x.NombreCliente.Contains(this.NombreCliente));
            }
            LstVisita = query.ToPagedList(this.np ?? 1 , ConstantHelpers.DEFAULT_PAGE_SIZE);
        }
    }
}