using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VEH.Intranet.Models;
using VEH.Intranet.Models.BE;
using System.Data.Entity;
using VEH.Intranet.Helpers;

namespace VEH.Intranet.Controllers
{
    public class ServiceController : ApiController
    {
        public SIVEHEntities context;
        [Route("api/AfariService/Login")]
        [HttpPost]
        public ResponseLogin Login(RequestLogin data)
        {
            ResponseLogin ResponseLogin = new ResponseLogin();

            try
            {
                try
                {
                    var usuario = context.Usuario.Include(x => x.Departamento).Include(x => x.Departamento.Edificio).FirstOrDefault(x => x.Codigo == data.usuario && x.Password == data.password);

                    if (usuario == null)
                    {
                        ResponseLogin.error = true;
                        ResponseLogin.mensaje = "Usuario y/o Contraseña Incorrectos";
                    }
                    else
                    {

                        if (usuario.Estado.Equals(ConstantHelpers.EstadoInactivo))
                        {
                            ResponseLogin.error = true;
                            ResponseLogin.mensaje = "Su cuenta no se encuentra habilitada. Consulte con su administrador";

                        }
                        if (usuario.Estado == "TEM")
                        {
                            ResponseLogin.error = true;
                            ResponseLogin.mensaje = "Debe cambiar su contraseña en el portal web";
                        }
                        if (usuario.Estado.Equals(ConstantHelpers.EstadoActivo))
                        {
                            ResponseLogin.error = false;
                            switch (usuario.Rol)
                            {
                                case ConstantHelpers.ROL_PROPIETARIO: ResponseLogin.rol = "PRO"; break;
                                case ConstantHelpers.ROL_ADMINISTRADOR: ResponseLogin.rol = "ADM"; break;
                            }

                            ResponseLogin.nombre = usuario.Nombres + " " + usuario.Apellidos;
                            ResponseLogin.correo = usuario.Email;
                            ResponseLogin.usuarioId = usuario.UsuarioId;
                            ResponseLogin.departamentoId = usuario.DepartamentoId;
                            ResponseLogin.edificioId = usuario.Departamento.EdificioId;

                            if (usuario.Rol.ToLower().Equals(ConstantHelpers.ROL_PROPIETARIO.ToLower()))
                            {
                                if (usuario.Departamento.Estado.Equals(ConstantHelpers.EstadoInactivo) || usuario.Departamento.Edificio.Estado.Equals(ConstantHelpers.EstadoInactivo))
                                {
                                    ResponseLogin.error = true;
                                    ResponseLogin.mensaje = "Su cuenta no se encuentra habilitada. Consulte con su administrador";
                                }

                                try
                                {
                                    var visita = new Visita();
                                    visita.Fecha = DateTime.Now;
                                    visita.DepartamentoId = usuario.DepartamentoId.Value;
                                    visita.Tipo = "APP";
                                    visita.EdificioId = usuario.Departamento.EdificioId;
                                    visita.UsuarioId = usuario.UsuarioId;
                                    context.Visita.Add(visita);
                                    context.SaveChanges();
                                }
                                catch (Exception ex)
                                {
                                    ResponseLogin.error = true;
                                    ResponseLogin.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
                                }

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ResponseLogin.error = true;
                    ResponseLogin.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
                }
            }
            catch (Exception ex)
            {
                ResponseLogin.error = true;
                ResponseLogin.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
            }

            return ResponseLogin;
        }
    }
}
