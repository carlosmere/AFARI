using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Owner
{
    public class _AddEditInquilinoViewModel : BaseViewModel
    {
        public Int32? InquilinoId { get; set; }
        public Int32? PropietarioId { get; set; }
        public Int32 DepartamentoId { get; set; }
        public Int32 EdificioId { get; set; }
        [Display(Name = "Razón Social")]
        public string RazonSocialInq { get; set; }
        [Display(Name = "RUC")]
        public string RUCInq { get; set; }
        public bool MostrarRUC { get; set; }
        [Display(Name = "Contacto")]
        public string ContactoInquilino { get; set; }
        [Display(Name = "DNI")]
        public string DniInquilino { get; set; }
        [Display(Name = "¿Mostrar RUC?")]
        public bool MostrarRUCInq { get; set; }
        public string FechaCreacion { get; set; }
        [Display(Name = "Teléfono")]
        public string TelefonoInq { get; set; }
        [Display(Name = "Email")]
        public string EmailInq { get; set; }
        [Display(Name = "Nombres")]
        public string NombresInq { get; set; } // Inq -> Inquilino
        [Display(Name = "Celular")]
        public string CelularInq { get; set; }
        public void Fill(CargarDatosContext c, Int32? inquilinoId, Int32? propietarioId, Int32 departamentoId, Int32 edificioId)
        {
            baseFill(c);

            this.PropietarioId = propietarioId;
            this.DepartamentoId = departamentoId;
            this.EdificioId = edificioId;
            this.InquilinoId = inquilinoId;
            if (this.InquilinoId.HasValue)
            {
                Inquilino inq = c.context.Inquilino.FirstOrDefault(X => X.InquilinoId == this.InquilinoId);
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