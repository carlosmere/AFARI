using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Models;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using VEH.Intranet.ViewModel.Shared;
using VEH.Intranet.Helpers;
using System.Data.Entity;

namespace VEH.Intranet.ViewModel.Owner
{
    public class AddEditPropietarioViewModel : BaseViewModel
    {
        public Int32? PropietarioId { get; set; }
        [Display(Name = "Tipo de documento")]
        public string TipoDocumento { get; set; }
        [Display(Name = "Nro. de documento")]
        public string NroDocumento { get; set; }
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        [Display(Name = "Nombres")]
        public string Nombres { get; set; }

        [Display(Name = "Nombres")]
        public string NombresInq { get; set; } // Inq -> Inquilino
        // [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        [Display(Name = "Apellido Paterno")]
        public string ApellidoPaterno { get; set; }
        // [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        [Display(Name = "Apellido Materno")]
        public string ApellidoMaterno { get; set; }
        [Display(Name = "Fecha de nacimiento")]
        public Nullable<System.DateTime> FechaNacimiento { get; set; }
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }
        [Display(Name = "Teléfono")]
        public string TelefonoInq { get; set; }
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Display(Name = "Email")]
        public string EmailInq { get; set; }
        public string Celular { get; set; }
        public string CelularInq { get; set; }
        public string Cargo { get; set; }
        public string Estado { get; set; }

        public String Poseedor { get; set; }
        public String Parentesco { get; set; }
        public Int32 DepartamentoId { get; set; }
        public Int32 EdificioId { get; set; }

        public Departamento Departamento { get; set; }
        public Edificio Edificio { get; set; }
        [Display(Name = "Razón Social")]
        public string RazonSocialInq { get; set; }
        [Display(Name = "RUC")]
        public string RUCInq { get; set; }
        [Display(Name = "Razón Social")]
        public string RazonSocial { get; set; }
        [Display(Name = "RUC")]
        public string RUC { get; set; }
        public bool MostrarRUC { get; set; }
        [Display(Name = "Contacto")]
        public string ContactoPropietario { get; set; }
        [Display(Name = "Contacto")]
        public string ContactoInquilino { get; set; }
        [Display(Name = "DNI")]
        public string DniInquilino { get; set; }
        [Display(Name = "¿Mostrar RUC?")]
        public bool MostrarRUCInq { get; set; }
        public string FechaCreacion { get; set; }

        public List<Inquilino> LstInquilino { get; set; } = new List<Inquilino>();
        public AddEditPropietarioViewModel() { }
        public void Fill(CargarDatosContext datacontext)
        {
            baseFill(datacontext);
            Departamento = datacontext.context.Departamento.FirstOrDefault(x => x.DepartamentoId == DepartamentoId);
            Edificio = datacontext.context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);

            if (PropietarioId.HasValue)
            {
                Propietario propietario = datacontext.context.Propietario.Include(x => x.Inquilino).FirstOrDefault(x => x.PropietarioId == PropietarioId.Value);
                if (propietario != null)
                {
                    LstInquilino = propietario.Inquilino.Where( x => x.Estado == ConstantHelpers.EstadoActivo).ToList();
                    this.TipoDocumento = propietario.TipoDocumento;
                    this.NroDocumento = propietario.NroDocumento;
                    this.Nombres = propietario.Nombres;
                    this.ApellidoMaterno = propietario.ApellidoMaterno;
                    this.ApellidoPaterno = propietario.ApellidoPaterno;
                    this.FechaNacimiento = propietario.FechaNacimiento;
                    this.Telefono = propietario.Telefono;
                    this.Celular = propietario.Celular;
                    this.Cargo = propietario.Cargo;
                    this.Estado = propietario.Estado;
                    this.Email = propietario.Email;
                    this.RazonSocial = propietario.RazonSocial;
                    this.RUC = propietario.RUC;
                    this.FechaCreacion = propietario.FechaCreacion.HasValue ? propietario.FechaCreacion.Value.ToString("dd/MM/yyyy") : String.Empty;
                    this.Parentesco = propietario.ParentescoTitular;
                    this.MostrarRUC = propietario.MostrarRUC.Value;
                    this.ContactoPropietario = propietario.Contacto;
                    if (!String.IsNullOrEmpty(propietario.Poseedor))
                        this.Poseedor = propietario.Poseedor;
                        
                    Inquilino inq = datacontext.context.Inquilino.FirstOrDefault(X => X.PropietarioId == PropietarioId.Value && X.Estado == Helpers.ConstantHelpers.EstadoActivo);
                    if (inq != null)
                    {
                        this.NombresInq = inq.Nombres;
                        this.CelularInq = inq.Celular;
                        this.TelefonoInq = inq.Telefono;
                        this.EmailInq = inq.Email;
                        this.ContactoInquilino = inq.Contacto;
                        this.DniInquilino = inq.Dni;
                        this.RUCInq = inq.RUC;
                        this.RazonSocialInq = inq.RazonSocial;
                        this.MostrarRUCInq = inq.MostrarRUC.Value;
                    }
                }
            }
        }
    }
}