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
    public class LstDatosClientesViewModel : BaseViewModel
    {
        public IPagedList<ClienteCorretaje> LstClienteCorretaje { get; set; }
        public String Direccion { get; set; }
        public Int32? np { get; set; }
        public String Distrito { get; set; }
        public String Cliente { get; set; }
        public void Fill(CargarDatosContext c, Int32? np, String direccion, String distrito, String cliente)
        {
            this.np = np ?? 1;
            this.Direccion = direccion;
            this.Distrito = distrito;
            this.Cliente = cliente;

            var query = c.context.ClienteCorretaje.Where( x => x.Estado == ConstantHelpers.EstadoActivo).AsQueryable();
            if (!String.IsNullOrEmpty(this.Direccion))
            {
                query = query.Where( x => x.Direccion.Contains(this.Direccion));
            }
            if (!String.IsNullOrEmpty(this.Distrito))
            {
                query = query.Where(x => x.Distrito.Contains(this.Distrito));
            }
            if (!String.IsNullOrEmpty(this.Cliente))
            {
                query = query.Where(x => x.Cliente.Contains(this.Cliente));
            }


            query = query.OrderBy( x => x.Distrito);

            LstClienteCorretaje = query.ToPagedList(this.np ?? 1 , ConstantHelpers.DEFAULT_PAGE_SIZE);
        }
    }
}