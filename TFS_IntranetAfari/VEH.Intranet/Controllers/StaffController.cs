using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.Filters;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Staff;

namespace VEH.Intranet.Controllers
{
    [AppAuthorize(AppRol.Administrador)]
    public class StaffController : BaseController
    {
        //
        // GET: /Staff/

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-book")]
        public ActionResult LstPlanillaAdmin(Int32? np, Int32 TrabajadorId ,Int32 EdificioId)
        {
            LstPlanillaViewModel ViewModel = new LstPlanillaViewModel();
            ViewModel.EdificioId = EdificioId;
            ViewModel.TrabajadorId = TrabajadorId;
            ViewModel.Fill(CargarDatosContext(), np);
            return View(ViewModel);
        }
        [AppAuthorize(AppRol.Propietario)]
        [ViewParameter("Trabajador", "fa fa-book")]
        public ActionResult LstPlanilla(Int32? np,  Int32 TrabajadorId)
        {
            LstPlanillaViewModel ViewModel = new LstPlanillaViewModel();
            ViewModel.EdificioId = SessionHelpers.GetEdificioId(Session);
            ViewModel.TrabajadorId = TrabajadorId;
            ViewModel.Fill(CargarDatosContext(), np);
            return View(ViewModel);
        }

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Edificio", "fa fa-book")]
        public ActionResult AddEditPlanilla(Int32? PlanillaId, Int32 EdificioId, Int32 TrabajadorId)
        {
            AddEditPlanillaViewModel ViewModel = new AddEditPlanillaViewModel();
            ViewModel.TrabajadorId = TrabajadorId;
            ViewModel.EdificioId = EdificioId;
            ViewModel.PlanillaId = PlanillaId;
            ViewModel.FillComboUnidadTiempo(CargarDatosContext());
            ViewModel.Fill(CargarDatosContext());
            return View(ViewModel);
        }

        [HttpPost]
        public ActionResult AddEditPlanilla(AddEditPlanillaViewModel ViewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewModel.FillComboUnidadTiempo(CargarDatosContext());
                ViewModel.Fill(CargarDatosContext());
                PostMessage(MessageType.Error, "Ha ocurrido un error, verifique los datos ingresados.");
                return View(ViewModel);
            }
            try
            {
                if (ViewModel.PlanillaId.HasValue)
                {
                    Planilla planilla = context.Planilla.FirstOrDefault(x => x.PlanillaId == ViewModel.PlanillaId.Value);
                    planilla.AdelantoQuincena = planilla.AdelantoQuincena;
                    planilla.AporteObligatorio = planilla.AporteObligatorio;
                    planilla.ComisionAFP = planilla.ComisionAFP;
                    planilla.CTSMes = planilla.CTSMes;
                    planilla.ESSALUD = planilla.ESSALUD;
                    planilla.Feriado = planilla.Feriado;
                    planilla.HorasExtras = planilla.HorasExtras;
                    planilla.PrimaSeguro = planilla.PrimaSeguro;
                    planilla.ReemplazoVacaciones = planilla.ReemplazoVacaciones;
                    planilla.SegundaQuincena = planilla.SegundaQuincena;
                    planilla.SegundaQuincenaNeto = planilla.SegundaQuincenaNeto;
                    planilla.SueldoTotalNeto = planilla.SueldoTotalNeto;
                    planilla.TotalDescuentos = planilla.TotalDescuentos;
                    planilla.UnidadTiempoId = ViewModel.UnidadTiempoId;
                    context.Entry(planilla).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    Planilla planilla = new Planilla();
                    planilla.TrabajadorId = ViewModel.TrabajadorId;
                    planilla.AdelantoQuincena = ViewModel.AdelantoQuincena.ToDecimal();
                    planilla.AporteObligatorio = ViewModel.AporteObligatorio.ToDecimal();
                    planilla.ComisionAFP = ViewModel.ComisionAFP.ToDecimal();
                    planilla.CTSMes = ViewModel.CTSMes.ToDecimal();
                    planilla.ESSALUD = ViewModel.ESSALUD.ToDecimal();
                    planilla.Feriado = ViewModel.Feriado.ToDecimal();
                    planilla.HorasExtras = ViewModel.HorasExtras.ToDecimal();
                    planilla.PrimaSeguro = ViewModel.PrimaSeguro.ToDecimal();
                    planilla.ReemplazoVacaciones = ViewModel.ReemplazoVacaciones.ToDecimal();
                    planilla.SegundaQuincena = ViewModel.SegundaQuincena.ToDecimal();
                    planilla.SegundaQuincenaNeto = ViewModel.SegundaQuincenaNeto.ToDecimal();
                    planilla.SueldoTotalNeto = ViewModel.SueldoTotalNeto.ToDecimal();
                    planilla.TotalDescuentos = ViewModel.TotalDescuentos.ToDecimal();
                    planilla.UnidadTiempoId = ViewModel.UnidadTiempoId;
                    context.Planilla.Add(planilla);
                }
                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch { PostMessage(MessageType.Error); }
            return RedirectToAction("LstPlanillaAdmin", new { EdificioId = ViewModel.EdificioId, TrabajadorId = ViewModel.TrabajadorId });
        }

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter(PageIcon: "fa fa-trash-o")]
        public ActionResult _DeletePlanilla(Int32 PlanillaId, Int32 EdificioId, Int32 TrabajadorId)
        {
            ViewBag.EdificioId = EdificioId;
            ViewBag.TrabajadorId = TrabajadorId;
            ViewBag.PlanillaId = PlanillaId;
            return PartialView();
        }

        [HttpPost]
        public ActionResult DeletePlanilla(Int32 PlanillaId, Int32 EdificioId, Int32 TrabajadorId)
        {
            try
            {
                Planilla planilla = context.Planilla.FirstOrDefault(x => x.PlanillaId == PlanillaId);
                context.Planilla.Remove(planilla);
                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch { PostMessage(MessageType.Error); }
            return RedirectToAction("LstPlanillaAdmin", new { EdificioId = EdificioId, TrabajadorId = TrabajadorId });
        }
    }
}
