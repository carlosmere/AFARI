using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.Filters;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Owner;

namespace VEH.Intranet.Controllers
{
    [AppAuthorize(AppRol.Administrador)]
    public class OwnerController : BaseController
    {
        [ViewParameter("Edificio", "fa fa-key")]
        public ActionResult LstPropietario(Int32 DepartamentoId, Int32 EdificioId, String Estado)
        {
            LstPropietarioViewModel ViewModel = new LstPropietarioViewModel();
            ViewModel.DepartamentoId = DepartamentoId;
            ViewModel.Estado = Estado;
            ViewModel.EdificioId = EdificioId;
            ViewModel.Fill(CargarDatosContext());
            return View(ViewModel);
        }
        public ActionResult DeleteInquilino(Int32? PropietarioId, Int32 DepartamentoId, Int32 EdificioId, Int32? InquilinoId)
        {
            try
            {
                var inquilino = context.Inquilino.FirstOrDefault( x => x.InquilinoId == InquilinoId);
                inquilino.Estado = ConstantHelpers.EstadoInactivo;
                context.SaveChanges();

                PostMessage(MessageType.Success);
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error, ex.Message);
            }

            return RedirectToAction("AddEditPropietario","Owner",new { PropietarioId = PropietarioId , DepartamentoId = DepartamentoId , EdificioId = EdificioId });
        }
        public ActionResult _AddEditInquilino(Int32? PropietarioId, Int32 DepartamentoId, Int32 EdificioId, Int32? InquilinoId)
        {
            var viewModel = new _AddEditInquilinoViewModel();
            viewModel.Fill(CargarDatosContext(),InquilinoId,PropietarioId,DepartamentoId, EdificioId);
            return View(viewModel);
        }
        [HttpPost]
        public ActionResult _AddEditInquilino(_AddEditInquilinoViewModel model)
        {
            try
            {
                Inquilino inq = null;
                if (model.InquilinoId.HasValue)
                {
                    inq = context.Inquilino.FirstOrDefault( x => x.InquilinoId == model.InquilinoId);
                }
                else
                {
                    inq = new Inquilino();
                    inq.PropietarioId = model.PropietarioId.Value;
                    inq.Estado = ConstantHelpers.EstadoActivo;
                    context.Inquilino.Add(inq);
                }

                inq.Nombres = model.NombresInq;
                inq.Telefono = model.TelefonoInq;
                inq.Email = model.EmailInq;
                inq.Celular = model.CelularInq;
                inq.Contacto = model.ContactoInquilino;
                inq.Dni = model.DniInquilino;
                inq.RUC = model.RUCInq;
                inq.RazonSocial = model.RazonSocialInq;
                inq.MostrarRUC = model.MostrarRUCInq;

                context.SaveChanges();

                PostMessage(MessageType.Success);
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error, ex.Message);
            }
            return RedirectToAction("AddEditPropietario", "Owner", new { PropietarioId = model.PropietarioId, DepartamentoId = model.DepartamentoId, EdificioId = model.EdificioId });
        }
        [ViewParameter("Edificio", "fa fa-key")]
        public ActionResult AddEditPropietario(Int32? PropietarioId, Int32 DepartamentoId, Int32 EdificioId)
        {
            AddEditPropietarioViewModel ViewModel = new AddEditPropietarioViewModel();
            ViewModel.PropietarioId = PropietarioId;
            ViewModel.DepartamentoId = DepartamentoId;
            ViewModel.EdificioId = EdificioId;
            ViewModel.Fill(CargarDatosContext());
            return View(ViewModel);
        }

        [HttpPost]
        public ActionResult AddEditPropietario(AddEditPropietarioViewModel ViewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewModel.Fill(CargarDatosContext());
                TryUpdateModel(ViewModel);
                return View(ViewModel);
            }

            try
            {
                Propietario propietario;
                if (ViewModel.PropietarioId.HasValue)
                {
                    propietario = context.Propietario.FirstOrDefault(x => x.DepartamentoId == ViewModel.DepartamentoId && x.PropietarioId == ViewModel.PropietarioId);
                    context.Entry(propietario).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    propietario = new Propietario();
                    propietario.Estado = ConstantHelpers.EstadoActivo;
                    context.Propietario.Add(propietario);
                }
                
                propietario.Nombres = ViewModel.Nombres;
                propietario.ApellidoMaterno = ViewModel.ApellidoMaterno;
                propietario.ApellidoPaterno = ViewModel.ApellidoPaterno;
                propietario.Cargo = ViewModel.Cargo;
                propietario.Celular = ViewModel.Celular;
                propietario.DepartamentoId = ViewModel.DepartamentoId;
                propietario.FechaNacimiento = ViewModel.FechaNacimiento;
                propietario.NroDocumento = ViewModel.NroDocumento;
                propietario.Telefono = ViewModel.Telefono;
                propietario.TipoDocumento = ViewModel.TipoDocumento;
                propietario.Email = ViewModel.Email;
                propietario.RazonSocial = ViewModel.RazonSocial;
                propietario.RUC = ViewModel.RUC;
                propietario.FechaCreacion = null;
                if (!String.IsNullOrEmpty(ViewModel.FechaCreacion))
                {
                    try
                    {
                        propietario.FechaCreacion = ViewModel.FechaCreacion.ToDateTime();
                    }
                    catch (Exception ex)
                    {
                        propietario.FechaCreacion = null;
                    }
                }
                propietario.MostrarRUC = ViewModel.MostrarRUC;
                propietario.Contacto = ViewModel.ContactoPropietario;
                propietario.ParentescoTitular = String.IsNullOrEmpty(ViewModel.Parentesco)?"Titular":ViewModel.Parentesco;
                if (!String.IsNullOrEmpty(ViewModel.Poseedor))
                    propietario.Poseedor = ViewModel.Poseedor;
                //if (ViewModel.NombresInq != null)
                //{
                //    Inquilino inq = context.Inquilino.FirstOrDefault(X => X.PropietarioId == propietario.PropietarioId);
                //    if (inq == null)
                //    {
                //        inq = new Inquilino();
                //        inq.PropietarioId = propietario.PropietarioId;

                //        inq.Estado = ConstantHelpers.EstadoActivo;
                //        context.Inquilino.Add(inq);
                //    }
                //    inq.Nombres = ViewModel.NombresInq;
                //    inq.Telefono = ViewModel.TelefonoInq;
                //    inq.Email = ViewModel.EmailInq;
                //    inq.Celular = ViewModel.CelularInq;
                //    inq.Contacto = ViewModel.ContactoInquilino;
                //    inq.Dni = ViewModel.DniInquilino;
                //    inq.RUC = ViewModel.RUCInq;
                //    inq.RazonSocial = ViewModel.RazonSocialInq;
                //    inq.MostrarRUC = ViewModel.MostrarRUCInq;
                //}
                //else
                //{
                //    Inquilino inq = context.Inquilino.FirstOrDefault(X => X.PropietarioId == propietario.PropietarioId);
                //    if (inq != null)
                //        context.Inquilino.Remove(inq);
                    
                //}

                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch (Exception ex) { PostMessage(MessageType.Error); }
            return RedirectToAction("LstPropietario", new { DepartamentoId = ViewModel.DepartamentoId, EdificioId = ViewModel.EdificioId });
        }
        public ActionResult ViewPropietario(Int32? PropietarioId, Int32 DepartamentoId, Int32 EdificioId)
        {
            AddEditPropietarioViewModel ViewModel = new AddEditPropietarioViewModel();
            ViewModel.PropietarioId = PropietarioId;
            ViewModel.DepartamentoId = DepartamentoId;
            ViewModel.EdificioId = EdificioId;
            ViewModel.Fill(CargarDatosContext());
            return View(ViewModel);
        }
        public ActionResult _DeletePropietario(Int32 PropietarioId, Int32 DepartamentoId, Int32 EdificioId)
        {
            ViewBag.PropietarioId = PropietarioId;
            ViewBag.DepartamentoId = DepartamentoId;
            ViewBag.EdificioId = EdificioId;
            return PartialView();
        }

        [HttpPost]
        public ActionResult DeletePropietario(Int32 PropietarioId, Int32 DepartamentoId, Int32 EdificioId)
        {
            try
            {
                Propietario propietario = context.Propietario.FirstOrDefault(x => x.PropietarioId == PropietarioId 
                && x.DepartamentoId == DepartamentoId && x.Estado == ConstantHelpers.EstadoActivo);

                if (propietario != null)
                {
                    propietario.Estado = ConstantHelpers.EstadoInactivo;
                    context.Entry(propietario).State = System.Data.Entity.EntityState.Modified;

                    if (propietario.ParentescoTitular.ToLower() == "titular")
                    {
                        DepartamentoHistorico historico = new DepartamentoHistorico();
                        historico.DepartamentoId = DepartamentoId;
                        historico.PropietarioId = PropietarioId;
                        historico.Fecha = DateTime.Now;
                        historico.Estado = ConstantHelpers.EstadoActivo;
                        context.DepartamentoHistorico.Add(historico);
                    }

                    context.SaveChanges();
                }

                PostMessage(MessageType.Success);
            }
            catch { PostMessage(MessageType.Error); }
            return RedirectToAction("LstPropietario", new { DepartamentoId = DepartamentoId, EdificioId = EdificioId });
        }
    }
}