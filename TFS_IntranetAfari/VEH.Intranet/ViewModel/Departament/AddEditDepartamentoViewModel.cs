using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Departament
{
    public class AddEditDepartamentoViewModel : BaseViewModel
    {

        public Int32? DepartamentoId { get; set; }
        [Display(Name = "Número")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public string Numero { get; set; }
         [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public int Piso { get; set; }
        public string Estado { get; set; }
        public Int32? EdificioId { get; set; }
        [Display(Name = "Lectura de agua")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public decimal LecturaAgua { get; set; }
        public String NombreEdificio { get; set; }
        public String AcronimoEdificio { get; set; }
        [Display(Name = "Factor de Gasto")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Decimal FactorGasto { get; set; }

        public String Estacionamiento { get; set; }
        [Display(Name="Depósito")]
        public String Deposito { get; set; }

        [Display(Name="Área del departamento (m2)")]
        public Decimal? DepartamentoM2 { get; set; }
        [Display(Name="Área del estacionamiento (m2)")]
        public Decimal? EstacionamientoM2 { get; set; }
        [Display(Name = "Área del depósito (m2)")]
        public Decimal? DepositoM2 { get; set; }
        [Display(Name = "Área total (m2)")]
        public Decimal? AreaTotalM2 { get; set; }

        [Display(Name = "%Distribución")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Decimal PDistribucion { get; set; }

        public AddEditDepartamentoViewModel() { }
        [Display(Name = "Cuota Predeterminada")]
        public Decimal? CuotaDefault { get; set; }
        [Display(Name = "Recibo a nombre de (Propietario / Inquilino)")]
        public String NombreRecibo { get; set; }
        [Display(Name = "¿Se mostrará alerta de moras?")]
        public Boolean AlertaMora { get; set; } = true;
        public String AlertaMoraDrop { get; set; } = String.Empty;
        public List<SelectListItem> LstNombreRecibo { get; set; } = new List<SelectListItem>();
        public String NombreReciboDepartamento { get; set; } = String.Empty;
        public List<SelectListItem> LstAlertaMora = new List<SelectListItem>();
        public List<TipoInmueble> LstTipoInmueble { get; set; }
        [Required]
        public Int32 TipoInmuebleId { get; set; } = 1;
        public String ContactoPropietario { get; set; } = "-";
        public String Propietario { get; set; } = "-";
        public String DniPropietario { get; set; } = "-";

        public String ContactoInquilino { get; set; } = "-";
        public String Inquilino { get; set; } = "-";
        public String DniInquilino { get; set; } = "-";

        public void Fill(Controllers.CargarDatosContext datacontext)
        {
            baseFill(datacontext);
            LstTipoInmueble = new List<TipoInmueble>();
            var Edificio = datacontext.context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);
            NombreEdificio = Edificio.Nombre;
            AcronimoEdificio = Edificio.Acronimo;
            this.FactorGasto = 0;

            LstNombreRecibo.Add(new SelectListItem { Value = "P", Text = "PROPIETARIO" });
            LstNombreRecibo.Add(new SelectListItem { Value = "I", Text = "INQUILINO" });

            LstAlertaMora.Add(new SelectListItem { Value = "0", Text="NO" });
            LstAlertaMora.Add(new SelectListItem { Value = "1", Text = "SÍ" });

            LstTipoInmueble = datacontext.context.TipoInmueble.Where(x => x.Estado == ConstantHelpers.EstadoActivo).ToList();
            if (DepartamentoId.HasValue)
            {   
                Departamento departamento = datacontext.context.Departamento.FirstOrDefault(x=>x.DepartamentoId == DepartamentoId.Value);
                this.Numero = departamento.Numero;
                this.Piso = departamento.Piso;
                this.LecturaAgua = departamento.LecturaAgua;
                this.Estado = departamento.Estado;
                this.EdificioId = departamento.EdificioId;
                this.FactorGasto = departamento.FactorGasto??0;
                this.Estacionamiento = departamento.Estacionamiento;
                this.Deposito = departamento.Deposito;
                this.DepartamentoM2 = departamento.DepartamentoM2;
                this.EstacionamientoM2 = departamento.EstacionamientoM2;
                this.DepositoM2 = departamento.DepositoM2;
                this.PDistribucion = departamento.PDistribucion??0;
                this.CuotaDefault = departamento.CuotaDefault;
                this.AreaTotalM2 = departamento.TotalM2;
                this.TipoInmuebleId = departamento.TipoInmuebleId ?? 0;
                this.NombreRecibo = departamento.NombreRecibo;
                this.AlertaMora = departamento.AlertaMora;
                AlertaMoraDrop = AlertaMora ? "1" : "0";
                this.NombreReciboDepartamento = departamento.NombrePropietario ? "P" : "I";
                var propietario = departamento.Propietario.FirstOrDefault(x => x.ParentescoTitular == "Titular");

                if (propietario != null)
                {
                    this.ContactoPropietario =propietario.Contacto;
                    this.Propietario = propietario.Nombres;
                    this.DniPropietario = propietario.NroDocumento;

                    var inquilino = propietario.Inquilino.FirstOrDefault(x => x.Estado == ConstantHelpers.EstadoActivo);
                    if (inquilino != null)
                    {
                        this.ContactoInquilino = inquilino.Contacto;
                        this.Inquilino = inquilino.Nombres;
                        this.DniInquilino = inquilino.Dni;
                    }
                }
            }
        }
    }
}