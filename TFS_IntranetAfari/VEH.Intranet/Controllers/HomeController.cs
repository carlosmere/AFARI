using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.Filters;
using VEH.Intranet.Helpers;
using VEH.Intranet.ViewModel.Home;
using VEH.Intranet.Models;
using OfficeOpenXml.Style;
using System.IO;
using OfficeOpenXml;

namespace VEH.Intranet.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }

        public ActionResult Idioma(String lang, String returnURL)
        {
            Session["Culture"] = new CultureInfo(lang);
            return Redirect(returnURL);
        }

        public ContentResult KeepAlive()
        {
            Session.Set(SessionKey.UsuarioId, Session.Get(SessionKey.UsuarioId));
            return Content("");
        }

        public ActionResult Login(String Sistema)
        {
            var model = new LoginViewModel();
            model.Sistema = Sistema;
            return View(model);
        }
        public ActionResult DescargarEstadisticas(Int32 Anio)
        {
            var ruta = Server.MapPath(@"~\Files\ReporteVisita.xlsx");
            try
            {
                using (FileStream fs = System.IO.File.OpenRead(ruta))
                using (ExcelPackage excelPackage = new ExcelPackage(fs))
                {
                    ExcelWorkbook excelWorkBook = excelPackage.Workbook;
                    ExcelWorksheet excelWorksheet = excelWorkBook.Worksheets.FirstOrDefault();

                    if (excelWorksheet != null)
                    {
                        var LstVisitas = context.Visita.Where( x => x.Fecha.Year == Anio).OrderByDescending(x => x.Fecha).ToList();
                        Int32 row = 3;
                        foreach (var item in LstVisitas)
                        {
                            excelWorksheet.Cells[row, 1].Value = item.Usuario.Nombres + " " + item.Usuario.Apellidos;
                            excelWorksheet.Cells[row, 2].Value = item.Tipo;
                            excelWorksheet.Cells[row, 3].Value = item.Departamento.Numero;
                            excelWorksheet.Cells[row, 4].Value = item.Edificio.Nombre;
                            excelWorksheet.Cells[row, 5].Value = item.Fecha.ToString("dd/MM/yyyy hh:mm:ss");

                            row++;
                        }
                    }

                    var fileStreamResult = new FileContentResult(excelPackage.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                    fileStreamResult.FileDownloadName = "Reporte de Accesos_" + DateTime.Now.ToShortDateString() + ".xlsx";
                    return fileStreamResult;
                }
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error, ex.Message + " " + (ex.InnerException != null ? ex.InnerException.Message : ""));
                return RedirectToAction("CuadroMoroso");
            }
        }

        [AppAuthorize(AppRol.Administrador)]
        [ViewParameter("Dashboard", "fa fa-dashboard")]
        public ActionResult AdministradorIndex(Int32? Anio)
        {
            var viewModel = new AdministradorIndexViewModel();
            viewModel.CargarDatos(CargarDatosContext(), Anio);
            return View(viewModel);
        }

        [AppAuthorize(AppRol.Propietario)]
        [ViewParameter("Dashboard", "fa fa-dashboard")]
        public ActionResult PropietarioIndex()
        {
            var viewModel = new PropietarioIndexViewModel();
            viewModel.CargarDatos(CargarDatosContext());
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuario = context.Usuario.Include(x => x.Departamento).Include(x => x.Departamento.Edificio).FirstOrDefault(x => x.Codigo == model.Codigo && x.Password == model.Contrasena);

            if (usuario == null)
            {
                //Session.Set(SessionKey.UsuarioId, usuario.UsuarioId);
                //Session.Set(SessionKey.NombreCompleto, usuario.Nombres + " " + usuario.Apellidos);
                PostMessage(MessageType.Error, "Usuario y/o Contraseña Incorrectos");
            }
            else
            {

                if (usuario.Estado.Equals(ConstantHelpers.EstadoInactivo))
                {
                    PostMessage(MessageType.Error, "Su cuenta no se encuentra habilitada. Consulte con su administrador");

                }
                if (usuario.Estado == "TEM")
                {
                    Session.Set(SessionKey.UsuarioId, usuario.UsuarioId);
                    Session.Set(SessionKey.NombreCompleto, usuario.Nombres + " " + usuario.Apellidos);
                    return RedirectToAction("CambiarContrasena");
                }
                if (usuario.Estado.Equals(ConstantHelpers.EstadoActivo))
                {
                    Session.Clear();
                    AppRol rol = AppRol.Propietario;

                    switch (usuario.Rol)
                    {
                        case ConstantHelpers.ROL_PROPIETARIO: rol = AppRol.Propietario; break;
                        case ConstantHelpers.ROL_ADMINISTRADOR: rol = AppRol.Administrador; break;
                    }
                    Session.Set(SessionKey.EsAdmin,usuario.EsAdmin ?? false);
                    Session.Set(SessionKey.Usuario, usuario);
                    Session.Set(SessionKey.UsuarioId, usuario.UsuarioId);
                    Session.Set(SessionKey.NombreCompleto, usuario.Nombres + " " + usuario.Apellidos);
                    Session.Set(SessionKey.Rol, rol);
                    Session.Set(SessionKey.RolCompleto, usuario.Rol);
                    Session.Set(SessionKey.Correo, usuario.Email ?? String.Empty);
                    Session.Set(SessionKey.NombreRemitente, usuario.NombreRemitente ?? String.Empty);

                    if (usuario.Rol.ToLower().Equals(ConstantHelpers.ROL_PROPIETARIO.ToLower()))
                    {
                        if (usuario.Departamento.Estado.Equals(ConstantHelpers.EstadoInactivo) || usuario.Departamento.Edificio.Estado.Equals(ConstantHelpers.EstadoInactivo))
                        {
                            PostMessage(MessageType.Error, "Su cuenta no se encuentra habilitada. Consulte con su administrador");
                            return View(model);
                        }
                        Session.Set(SessionKey.DepartamentoId, usuario.DepartamentoId);
                        Session.Set(SessionKey.EdificioId, usuario.Departamento.EdificioId);

                        try
                        {
                            var visita = new Visita();
                            visita.Fecha = DateTime.Now;
                            visita.DepartamentoId = usuario.DepartamentoId.Value;
                            visita.Tipo = "WEB";
                            visita.EdificioId = usuario.Departamento.EdificioId;
                            visita.UsuarioId = usuario.UsuarioId;
                            context.Visita.Add(visita);
                            context.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                        }

                    }
                    return Dashboard();
                }
            }
            return View(model);
        }

        public ActionResult Dashboard()
        {
            switch (Session.GetRol())
            {
                case AppRol.Propietario: return RedirectToAction("PropietarioIndex");
                case AppRol.Administrador: return RedirectToAction("AdministradorIndex");
            }

            return RedirectToAction("Login");
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
        public ActionResult CambiarContrasena()
        {
            var model = new CambiarContrasenaViewModel();
            model.Fill(CargarDatosContext(), Session.GetUsuarioId(), Session.GetNombreCompleto());
            return View(model);
        }
        [HttpPost]
        public ActionResult CambiarContrasena(CambiarContrasenaViewModel model)
        {
            try
            {
                var usuario = context.Usuario.FirstOrDefault( x => x.UsuarioId == model.UsuarioId);
                if (model.Password != usuario.Password)
                {
                    PostMessage(MessageType.Warning,"Las contraseñas no coinciden.");
                    return View(model);
                }
                usuario.Estado = ConstantHelpers.EstadoActivo;
                usuario.Password = model.NewPassword;
                context.SaveChanges();
                return Dashboard();
            }
            catch (Exception ex)
            {
                return View(model);
            }
        }
    }
}
