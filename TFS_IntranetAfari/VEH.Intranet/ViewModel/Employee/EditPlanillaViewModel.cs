using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.Controllers;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Employee
{
    public class EditPlanillaViewModel : BaseViewModel
    {
        public Int32 EdificioId { get; set; }
        public Int32? UnidadTiempoId { get; set; }

      //  public List<Trabajador> LstTrabajadores { get; set; }
        public List<SelectListItem> LstComboUnidadTiempo { get; set; }

      //  public List<Planilla> LstPlanilla { get; set; }

        public Edificio Edificio { get; set; }
        
        public String RutaExcel { get; set; }
        public String RutaPDF { get; set; }
        [Display(Name = "Planilla Privada (Excel)")]
        public HttpPostedFileBase Archivo { get; set; }
        [Display(Name = "Planilla Pública (PDF)")]
        public HttpPostedFileBase ArchivoPublico { get; set; }
        public PlanillaR PlanillaR { get; set; }
        public UnidadTiempo objUnidadTiempo { get; set; }
        public EditPlanillaViewModel()
        {
         //   LstTrabajadores = new List<Trabajador>();
            LstComboUnidadTiempo = new List<SelectListItem>();
         //   LstPlanilla = new List<Planilla>();
        }

        public void Fill(CargarDatosContext datacontext, Int32 EdificioId, Int32? UnidadTiempoId)
        {

            baseFill(datacontext);
            this.EdificioId = EdificioId;
            this.UnidadTiempoId = UnidadTiempoId;

            //LstTrabajadores = datacontext.context.Trabajador.Where(x => x.EdificioId == EdificioId && x.Estado == ConstantHelpers.EstadoActivo).ToList();

            var lstunidadtiempo = datacontext.context.UnidadTiempo.OrderByDescending(x => x.Anio).OrderByDescending(x => x.Mes).Where(x => x.Estado == ConstantHelpers.EstadoActivo);
            foreach (var item in lstunidadtiempo)
                LstComboUnidadTiempo.Add(new SelectListItem { Value = item.UnidadTiempoId.ToString(), Text = item.Descripcion.ToUpper() });

            Edificio = datacontext.context.Edificio.Find(EdificioId);
            objUnidadTiempo = datacontext.context.UnidadTiempo.FirstOrDefault(x => x.UnidadTiempoId == UnidadTiempoId);
            
            PlanillaR planillaR = datacontext.context.PlanillaR.FirstOrDefault(x => x.EdificioId == EdificioId && x.UnidadTiempoId == UnidadTiempoId);
            if (planillaR != null)
            {
                RutaExcel = planillaR.RutaExcel;
                RutaPDF = planillaR.RutaPDF;
            }
            //Ruta = 

            //foreach (var item in LstTrabajadores)
            //{
            //    Planilla p = datacontext.context.Planilla.Where(x => x.TrabajadorId == item.TrabajadorId && x.UnidadTiempoId == UnidadTiempoId).FirstOrDefault();
            //    PlanillaQuincena pq = datacontext.context.PlanillaQuincena.Where(x => x.TrabajadorId == item.TrabajadorId && x.UnidadTiempoId == UnidadTiempoId).FirstOrDefault();
            //    if (p != null && pq != null)
            //    {
            //        p.AdelantoQuincena = pq.TotalQuincena ?? 0;
            //    }
            //    LstPlanilla.Add(p);
            //}
        }
    }
}