using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.Controllers;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Employee
{
    public class EditPlanillaQuincenaViewModel : BaseViewModel
    {
        public Int32 EdificioId { get; set; }
        public Int32? UnidadTiempoId { get; set; }

        public List<Trabajador> LstTrabajadores { get; set; }
        public List<SelectListItem> LstComboUnidadTiempo { get; set; }

        public List<PlanillaQuincena> LstPlanilla { get; set; }

        public Edificio Edificio { get; set; }

        public EditPlanillaQuincenaViewModel()
        {
            LstTrabajadores = new List<Trabajador>();
            LstComboUnidadTiempo = new List<SelectListItem>();
            LstPlanilla = new List<PlanillaQuincena>();
        }

        public void Fill(CargarDatosContext datacontext, Int32 EdificioId, Int32? UnidadTiempoId)
        {
            baseFill(datacontext);
            this.EdificioId = EdificioId;
            this.UnidadTiempoId = UnidadTiempoId;

            LstTrabajadores = datacontext.context.Trabajador.Where(x => x.EdificioId == EdificioId && x.Estado == ConstantHelpers.EstadoActivo).ToList();

            var lstunidadtiempo = datacontext.context.UnidadTiempo.OrderByDescending(x => x.Anio).OrderByDescending(x => x.Mes).Where(x => x.Estado == ConstantHelpers.EstadoActivo);
            foreach (var item in lstunidadtiempo)
                LstComboUnidadTiempo.Add(new SelectListItem { Value = item.UnidadTiempoId.ToString(), Text = item.Descripcion.ToUpper() });

            Edificio = datacontext.context.Edificio.Find(EdificioId);

            foreach (var item in LstTrabajadores)
            {
                PlanillaQuincena p = datacontext.context.PlanillaQuincena.Where(x => x.TrabajadorId == item.TrabajadorId && x.UnidadTiempoId == UnidadTiempoId).FirstOrDefault();
                LstPlanilla.Add(p);
            }
        }
    }
}