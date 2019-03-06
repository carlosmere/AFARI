using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.Filters;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.User;
using System.Data.Entity;
using System.Net.Mail;
using VEH.Intranet.Logic;
using System.IO;

namespace VEH.Intranet.Controllers
{
    public class UserController : BaseController
    {
        //
        // GET: /User/

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("UsuarioAdm", "fa fa-shield")]
        public ActionResult LstUsuarioAdm()
        {
            LstUsuarioViewModel ViewModel = new LstUsuarioViewModel();
            ViewModel.FillAdm(CargarDatosContext());
            return View(ViewModel);
        }

        [ViewParameter("Edificio", "fa fa-user")]
        public ActionResult LstUsuario(Int32 DepartamentoId, Int32 EdificioId)
        {
            LstUsuarioViewModel ViewModel = new LstUsuarioViewModel();
            ViewModel.DepartamentoId = DepartamentoId;
            ViewModel.EdificioId = EdificioId;
            ViewModel.Fill(CargarDatosContext());
            return View(ViewModel);
        }
        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("UsuarioAdm", "fa fa-shield")]
        public ActionResult AddEditUsuarioAdm(Int32? UsuarioId)
        {
            AddEditUsuarioViewModel ViewModel = new AddEditUsuarioViewModel();
            ViewModel.UsuarioId = UsuarioId;
            ViewModel.Fill(CargarDatosContext());
            return View(ViewModel);
        }


        [ViewParameter("Edificio", "fa fa-user")]
        public ActionResult AddEditUsuario(Int32? UsuarioId, Int32 DepartamentoId, Int32 EdificioId)
        {
            AddEditUsuarioViewModel ViewModel = new AddEditUsuarioViewModel();
            ViewModel.UsuarioId = UsuarioId;
            ViewModel.DepartamentoId = DepartamentoId;
            ViewModel.EdificioId = EdificioId;
            ViewModel.Fill(CargarDatosContext());
            return View(ViewModel);
        }

        [HttpPost]
        public ActionResult AddEditUsuarioAdm(AddEditUsuarioViewModel ViewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewModel.Fill(CargarDatosContext());
                TryUpdateModel(ViewModel);
                return View(ViewModel);
            }

            try
            {
                Usuario Usuario = null;
                if (ViewModel.UsuarioId.HasValue)
                {
                    Usuario = context.Usuario.FirstOrDefault(x => x.UsuarioId == ViewModel.UsuarioId);
                    Usuario.Nombres = ViewModel.Nombres;
                    Usuario.Apellidos = ViewModel.Apellidos;
                    Usuario.Codigo = ViewModel.Codigo;
                    Usuario.Password = ViewModel.Password;
                    Usuario.Estado = ViewModel.Estado;
                    Usuario.Email = ViewModel.Email;
                    Usuario.NombreRemitente = ViewModel.NombreEncargado;
                    context.Entry(Usuario).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    Usuario = new Usuario();
                    Usuario.Nombres = ViewModel.Nombres;
                    Usuario.Apellidos = ViewModel.Apellidos;
                    Usuario.Codigo = ViewModel.Codigo;
                    Usuario.Password = ViewModel.Password;
                    Usuario.Rol = ConstantHelpers.ROL_ADMINISTRADOR;
                    Usuario.DepartamentoId = ViewModel.DepartamentoId;
                    Usuario.Estado = ViewModel.Estado;
                    Usuario.Email = ViewModel.Email;
                    Usuario.EsAdmin = false;
                    Usuario.NombreRemitente = ViewModel.NombreEncargado;
                    context.Usuario.Add(Usuario);
                }

                Session.Set(SessionKey.Correo, Usuario.Email);
                Session.Set(SessionKey.NombreRemitente, Usuario.NombreRemitente);

                if (ViewModel.Firma != null && ViewModel.Firma.ContentLength != 0)
                {
                    string _rutaFirmaserv = Server.MapPath("~");
                    string _rutaFirmadir = _rutaFirmaserv + Path.Combine("/Resources/Files", String.Empty);
                    if (!System.IO.Directory.Exists(_rutaFirmadir))
                        Directory.CreateDirectory(_rutaFirmadir);

                    string _nombrearc = Usuario.Nombres + "_" + DateTime.Now.Ticks.ToString() + "_" + Path.GetExtension(ViewModel.Firma.FileName);
                    _rutaFirmadir = Path.Combine(_rutaFirmadir, _nombrearc);

                    Usuario.Firma = _nombrearc;
                    ViewModel.Firma.SaveAs(_rutaFirmadir);

                }

                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch { PostMessage(MessageType.Error); }
            return RedirectToAction("LstUsuarioAdm");
        }

        [HttpPost]
        public ActionResult AddEditUsuario(AddEditUsuarioViewModel ViewModel)
        {
            try
            {
                if (ViewModel.UsuarioId.HasValue)
                {
                    Usuario Usuario = context.Usuario.FirstOrDefault(x => x.UsuarioId == ViewModel.UsuarioId);
                    Usuario.Nombres = ViewModel.Nombres;
                    Usuario.Apellidos = ViewModel.Apellidos;
                    Usuario.Codigo = ViewModel.Codigo;
                    Usuario.Password = ViewModel.Password;
                    Usuario.Estado = ViewModel.Estado; 
                    Usuario.Email = ViewModel.Email;

                    context.Entry(Usuario).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                }
                else
                {
                    Usuario Usuario = context.Usuario.FirstOrDefault(X => X.Codigo == ViewModel.Codigo);
                        
                    if(Usuario==null)
                        Usuario= new Usuario();

                    Usuario.Nombres = ViewModel.Nombres;
                    Usuario.Apellidos = ViewModel.Apellidos;
                    Usuario.Codigo = ViewModel.Codigo;
                    Usuario.Password = ViewModel.Password;
                    Usuario.Rol = ConstantHelpers.ROL_PROPIETARIO;
                    Usuario.DepartamentoId = ViewModel.DepartamentoId;
                    Usuario.Estado = "TEM"; 
                    Usuario.Email = ViewModel.Email;
                    context.Usuario.Add(Usuario);



                   
                    context.SaveChanges();

                    
                    if (!String.IsNullOrEmpty(Usuario.Email))
                    {
                        EmailLogic logic = new EmailLogic(this, CargarDatosContext());
                        var edificio = context.Edificio.Include(X=>X.Departamento).FirstOrDefault(X=>X.Departamento.Any(Y=>Y.DepartamentoId==(Usuario.DepartamentoId??-1)));
                        if(edificio!=null)
                        {
                            RegisterNewUserViewModel model = new RegisterNewUserViewModel();
                            model.Password = Usuario.Password;
                            model.Nombre = Usuario.Nombres;
                            model.Usuario = Usuario.Codigo;
                            var archivos = new List<String>();
                            archivos.Add(Path.Combine(Server.MapPath("~/Content"), "Intranet Afari.pdf"));
                            logic.SendEmail("Usuario Afari","RegistroUsuario","sistema@afari.pe","Afari",Usuario.Email,model, archivos, String.Empty,String.Empty);

                        }
                    }
                }
                
                PostMessage(MessageType.Success);
            }
            catch(Exception ex) { PostMessage(MessageType.Error,"Error tecnico: "+ex.Message.ToString()); }
            return RedirectToAction("LstUsuario", new { DepartamentoId = ViewModel.DepartamentoId, EdificioId = ViewModel.EdificioId });
        }
        public ActionResult AddEditPdfUsuarioNuevo()
        {
            var model = new AddEditPdfUsuarioNuevoViewModel();
            model.Fill(CargarDatosContext());
            return View(model);
        }
        [HttpPost]
        public ActionResult AddEditPdfUsuarioNuevo(AddEditPdfUsuarioNuevoViewModel model)
        {
            try
            {
                if (model.Archivo != null && model.Archivo.ContentLength != 0)
                {
                    string _rutaFirmaserv = Server.MapPath("~");
                    string _rutaFirmadir = _rutaFirmaserv + Path.Combine("/Content", String.Empty);
                    if (!System.IO.Directory.Exists(_rutaFirmadir))
                        Directory.CreateDirectory(_rutaFirmadir);

                    string _nombrearc = "Intranet Afari.pdf";
                    _rutaFirmadir = Path.Combine(_rutaFirmadir, _nombrearc);

                    model.Archivo.SaveAs(_rutaFirmadir);

                }
                PostMessage(MessageType.Success);
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error, ex.Message);
            }
            
            return RedirectToAction("AddEditPdfUsuarioNuevo");
        }

        public ActionResult _DeleteUsuario(Int32 UsuarioId, Int32? DepartamentoId, Int32? EdificioId, string Tipo)
        {
            ViewBag.UsuarioId = UsuarioId;
            ViewBag.DepartamentoId = DepartamentoId;
            ViewBag.EdificioId = EdificioId;
            ViewBag.Tipo = Tipo;
            return PartialView();
        }

        [HttpPost]
        public ActionResult DeleteUsuario(Int32 UsuarioId, Int32? DepartamentoId, Int32? EdificioId, string Tipo)
        {
            try
            {
                Usuario Usuario = context.Usuario.FirstOrDefault(x => x.UsuarioId == UsuarioId);
                Usuario.Estado = ConstantHelpers.EstadoEliminado;
                context.SaveChanges();
                PostMessage(MessageType.Success);
            }
            catch { PostMessage(MessageType.Error); }
            if (Tipo == ConstantHelpers.ROL_ADMINISTRADOR)
                return RedirectToAction("LstUsuarioAdm");
            else
                return RedirectToAction("LstUsuario", new { DepartamentoId = DepartamentoId, EdificioId = EdificioId });
        }
        public PartialViewResult _ForgotPassword()
        {
            var model = new _ForgotPasswordViewModel();
            return PartialView(model);
        }
        [HttpPost]
        public ActionResult _ForgotPassword(_ForgotPasswordViewModel model)
        {
            try
            {
                EmailLogic mailLogic = new EmailLogic(this, CargarDatosContext());
                ViewModel.Templates.infoViewModel mailModel = new ViewModel.Templates.infoViewModel();

                var Lstusuario = context.Usuario.Where(x => x.Email == model.Email).ToList();
                if (Lstusuario.Count == 0)
                {
                    PostMessage(MessageType.Error, "El correo " + model.Email + " no se encuentra registrado en el sistema. Intente nuevamente ingresando un correo registrado en el sistema.");
                    return RedirectToAction("Login", "Home");
                }
                var Mensaje = String.Empty;
                foreach (var item in Lstusuario)
                {
                    if(item.Departamento != null)
                    {
                        Mensaje += "Edificio: " + item.Departamento.Edificio.Nombre + " - Departamento: " + item.Departamento.Numero + "<br/>";
                        Mensaje += "Usuario: " + item.Codigo + "<br/>";
                        Mensaje += "Contraseña: " + item.Password + "<br/>";
                        Mensaje += "---------------------------------<br/>";
                    }
                }
                mailModel.Mensaje = Mensaje;
                mailModel.Titulo = "Recuperar Contraseña";

                mailLogic.SendEmail("Recuperar Contraseña", "ForgorPassword", "sistema@afari.pe", "Afari", model.Email, mailModel, null);
                PostMessage(MessageType.Success,"Se envió un email con los datos de acceso a " + model.Email);
                return RedirectToAction("Login", "Home");
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error,"No se pudo enviar email.");
                return RedirectToAction("Login","Home");
            }
        }

        public JsonResult VerificarCodigoUsuario(string codigo)
        {
            return Json(new AddEditUsuarioViewModel().VerificarCodigoUsuario(CargarDatosContext(), codigo), JsonRequestBehavior.AllowGet);
        }
        public ActionResult CambiarContrasena()
        {
            return View();
        }
        public ActionResult CambiarPassword()
        {
            AddEditUsuarioViewModel ViewModel = new AddEditUsuarioViewModel();
            Int32 UsuarioId = Session.GetUsuarioId();
            Usuario usuario = context.Usuario.Include(x => x.Departamento).Include(x => x.Departamento.Edificio).FirstOrDefault(x => x.UsuarioId == UsuarioId);
            ViewModel.UsuarioId = usuario.UsuarioId;
            ViewModel.Nombres = usuario.Nombres;
            ViewModel.Apellidos = usuario.Apellidos;
            ViewModel.DepartamentoId = usuario.DepartamentoId;
            ViewModel.Password = usuario.Password;
            if (usuario.Rol != ConstantHelpers.ROL_ADMINISTRADOR)
            {
                ViewModel.TipoUsuario = ConstantHelpers.ROL_PROPIETARIO;
                ViewModel.DepartamentoId = usuario.DepartamentoId;
                ViewModel.EdificioId = usuario.Departamento.Edificio.EdificioId;
            }
            else { ViewModel.TipoUsuario = ConstantHelpers.ROL_ADMINISTRADOR; }
            ViewModel.Cargar(CargarDatosContext());
            return View(ViewModel);
        }

        [HttpPost]
        public ActionResult CambiarPassword(AddEditUsuarioViewModel ViewModel)
        {
            if (String.IsNullOrEmpty(ViewModel.PasswordNuevo))
            {
                Int32 UsuarioId = Session.GetUsuarioId();
                Usuario usuario = context.Usuario.Include(x => x.Departamento).Include(x => x.Departamento.Edificio).FirstOrDefault(x => x.UsuarioId == UsuarioId);
                ViewModel.Nombres = usuario.Nombres;
                ViewModel.Apellidos = usuario.Apellidos;
                ViewModel.DepartamentoId = usuario.DepartamentoId;
                ViewModel.Password = usuario.Password;
                if (usuario.Rol != ConstantHelpers.ROL_ADMINISTRADOR)
                {
                    ViewModel.TipoUsuario = ConstantHelpers.ROL_PROPIETARIO;
                    ViewModel.DepartamentoId = usuario.DepartamentoId;
                    ViewModel.EdificioId = usuario.Departamento.Edificio.EdificioId;
                }
                else { ViewModel.TipoUsuario = ConstantHelpers.ROL_ADMINISTRADOR; }

                TryUpdateModel(ViewModel);
                PostMessage(MessageType.Error, "Debe ingresar una contraseña nueva para continuar.");
                return View(ViewModel);
            }
            try
            {
                if (ViewModel.UsuarioId.HasValue)
                {
                    Usuario usuario = context.Usuario.FirstOrDefault(x => x.UsuarioId == ViewModel.UsuarioId.Value);
                    usuario.Password = ViewModel.PasswordNuevo;
                    context.Entry(usuario).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                    PostMessage(MessageType.Success,"Se cambió la nueva contraseña satisfactoriamente.");
                }
            }
            catch { PostMessage(MessageType.Error); }
            if (ViewModel.TipoUsuario == ConstantHelpers.ROL_ADMINISTRADOR) return RedirectToAction("AdministradorIndex","Home");
            return RedirectToAction("PropietarioIndex", "Home");
        }

    }
}
