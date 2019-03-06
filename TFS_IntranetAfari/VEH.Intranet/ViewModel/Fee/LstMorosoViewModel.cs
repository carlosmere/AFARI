using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.Controllers;
using VEH.Intranet.Models;
using System.Data.Entity;
using VEH.Intranet.Helpers;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Fee
{
    public class LstMorosoViewModel : BaseViewModel
    {
        public LstMorosoViewModel()
        {
            LstMoroso = new List<Cuota>();
            LstComboUnidadTiempo = new List<SelectListItem>();
        }

        public Int32 EdificioId { get; set; }
        public Edificio Edificio { get; set; }
        public Int32? UnidadTiempoId { get; set; }
        public List<Cuota> LstMoroso { get; set; }
        public List<SelectListItem> LstComboUnidadTiempo { get; set; }

        public void Fill(CargarDatosContext datacontext)
        {
            baseFill(datacontext);
            Edificio = datacontext.context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);
            var lstunidadtiempo = datacontext.context.UnidadTiempo.OrderByDescending(x => x.Anio).OrderByDescending(x => x.Mes).Where(x => x.Estado == ConstantHelpers.EstadoActivo);
            foreach (var item in lstunidadtiempo)
                LstComboUnidadTiempo.Add(new SelectListItem { Value = item.UnidadTiempoId.ToString(), Text = item.Descripcion.ToUpper() });


            List<Cuota> lstCuotas = new List<Cuota>();
            if (UnidadTiempoId.HasValue)
                lstCuotas = datacontext.context.Cuota.Include(x => x.Departamento).Where(x => x.Departamento.EdificioId == EdificioId && x.UnidadTiempoId == UnidadTiempoId).ToList();
            else
            {
                Int32 unidadtiempoid = lstunidadtiempo.First().UnidadTiempoId;
                lstCuotas = datacontext.context.Cuota.Include(x => x.Departamento).Where(x => x.Departamento.EdificioId == EdificioId && x.UnidadTiempoId == unidadtiempoid).ToList();
            }
            if (lstCuotas != null)
            {
                for (int i = 0; i < lstCuotas.Count(); i++)
                {
                    if (lstCuotas[i].Mora > 0)
                        LstMoroso.Add(lstCuotas[i]);
                }
            }
        }
    }
}