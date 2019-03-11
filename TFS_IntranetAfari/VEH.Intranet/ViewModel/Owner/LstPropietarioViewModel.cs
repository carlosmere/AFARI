using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;
using VEH.Intranet.Models;
using VEH.Intranet.Helpers;
using VEH.Intranet.Controllers;
using VEH.Intranet.ViewModel.Shared;
using System.Web.Mvc;

namespace VEH.Intranet.ViewModel.Owner
{
    public class LstPropietarioViewModel : BaseViewModel
    {
        public Int32 DepartamentoId { get; set; }
        public String Numero { get; set; }
        public Int32 EdificioId { get; set; }
        public List<Propietario> LstPropietario { get; set; }

        public Departamento Departamento { get; set; }
        public Edificio Edificio { get; set; }
        public String Estado { get; set; }
        public List<SelectListItem> LstEstado { get; set; } = new List<SelectListItem>();

        public LstPropietarioViewModel() {
            LstEstado.Add(new SelectListItem { Value = "1" , Text = "ACTIVO"});
            LstEstado.Add(new SelectListItem { Value = "2", Text = "INACTIVO" });
        }

        public void Fill(CargarDatosContext datacontext)
        {
            baseFill(datacontext);
            Departamento = datacontext.context.Departamento.FirstOrDefault(x=>x.DepartamentoId == DepartamentoId);
            Edificio = datacontext.context.Edificio.FirstOrDefault(x=>x.EdificioId == EdificioId);

            LstPropietario = new List<Propietario>();

            var query = datacontext.context.Propietario.OrderBy(x => x.Nombres).Where(x => x.DepartamentoId == DepartamentoId).AsQueryable();

            if (!String.IsNullOrEmpty(this.Estado) && this.Estado == "2")
            {
                query = query.Where(x => x.Estado != ConstantHelpers.EstadoPendiente && x.Estado != ConstantHelpers.EstadoActivo);
            }
            else
            {
                query = query.Where(x => x.Estado == ConstantHelpers.EstadoActivo);
            }
            LstPropietario = query.ToList();
        }
    }
}