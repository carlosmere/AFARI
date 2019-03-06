using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Data /*    */      .Entity;
using VEH.Intranet.Controllers;
using VEH.Intranet.Models;
using VEH.Intranet.Helpers;
using VEH.Intranet.ViewModel.Shared;
using System.Web;

namespace VEH.Intranet.ViewModel.User
{
    public class AddEditUsuarioViewModel : BaseViewModel
    {
        public Int32? UsuarioId { get; set; }
        [Display(Name = "Código")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public String Codigo { get; set; }
        [Display(Name = "Contraseña")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public String Password { get; set; }

        [Display(Name = "Nombres")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public String Nombres { get; set; }
        [Display(Name = "Apellidos")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public String Apellidos { get; set; }
        public String Rol { get; set; }
        public String Email { get; set; }
        public String Estado { get; set; }
        public Int32? DepartamentoId { get; set; }
        public Departamento Departamento { get; set; }
        public Edificio Edificio { get; set; }
        public Int32? EdificioId { get; set; }
        public String PasswordNuevo { get; set; }
        public String Tipo { get; set; }
        public String TipoUsuario { get; set; }
        public String EmailEncargado { get; set; }
        public String NombreEncargado { get; set; }
        public HttpPostedFileBase Firma { get; set; }
        public String RutaFirma { get; set; }
        public AddEditUsuarioViewModel() { }
        public void Cargar(CargarDatosContext datacontext)
        {
            baseFill(datacontext);
        }
        public void Fill(CargarDatosContext datacontext)
        {
            baseFill(datacontext);
            if (DepartamentoId.HasValue)
                Departamento = datacontext.context.Departamento.FirstOrDefault(x => x.DepartamentoId == DepartamentoId.Value);
            if (EdificioId.HasValue)
                Edificio = datacontext.context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId.Value);

            if (UsuarioId.HasValue)
            {
                Usuario usuario = datacontext.context.Usuario.FirstOrDefault(x => x.UsuarioId == UsuarioId.Value);
                this.Codigo = usuario.Codigo;
                this.Password = usuario.Password;
                this.Nombres = usuario.Nombres;
                this.Apellidos = usuario.Apellidos;
                this.Rol = usuario.Rol;
                this.Estado = usuario.Estado;
                this.DepartamentoId = usuario.DepartamentoId;
                this.Email = usuario.Email;
                this.RutaFirma = usuario.Firma;
                this.NombreEncargado = usuario.NombreRemitente;
            }
            else
            {

                if (DepartamentoId.HasValue && EdificioId.HasValue)
                {
                   
                    var a = datacontext.context.Propietario.Select(x => new { Nombres = x.Nombres }).Join(datacontext.context.Inquilino.Select(x => new { Nombres = x.Nombres }), y => y, z => z, (y, z) => new { Nombres = y.Nombres });
                    var Propetario = Departamento.Propietario.FirstOrDefault(x => x.Estado == ConstantHelpers.EstadoActivo);
                   /* ???? var prop = datacontext.context.Propietario.Include(x => x.Departamento).Where(x => x.DepartamentoId == DepartamentoId).Select(x => new { UsuarioId = x.PropietarioId, Nombres = x.Nombres }).ToList();
                    var inq = datacontext.context.Inquilino.Include(x => x.Propietario).Include(x => x.Propietario.Departamento).Where(x => x.Propietario.DepartamentoId == DepartamentoId).Select(x => new { UsuarioId = x.InquilinoId, Nombres = x.Nombres }).ToList();
                    inq.AddRange(prop);*/
                    this.Nombres = Propetario.Nombres;
                    this.Email = Propetario.Email;
                }
            }
        }

        public Boolean VerificarCodigoUsuario(CargarDatosContext datacontext, string codigo)
        {
            Usuario usuario = datacontext.context.Usuario.FirstOrDefault(x => x.Codigo.ToLower().Equals(codigo.ToLower()) && x.Estado=="ACT");
            if (usuario != null)
                return false;
            return true;
        }
    }

}