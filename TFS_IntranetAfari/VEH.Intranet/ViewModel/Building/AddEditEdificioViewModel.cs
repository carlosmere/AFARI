using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Models;

using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using VEH.Intranet.Helpers;
using VEH.Intranet.ViewModel.Shared;
namespace VEH.Intranet.ViewModel.Building
{
    public class AddEditEdificioViewModel : BaseViewModel
    {
        public Int32? EdificioId { get; set; }
        [Display(Name = "Acrónimo")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public string Acronimo { get; set; }
        [Display(Name = "Nombre")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public string Nombre { get; set; }
        [Display(Name = "Dirección")]
        //[Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]


        public string Direccion { get; set; }
        [Display(Name = "Referencia")]
        //[Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public string Referencia { get; set; }

        [Display(Name = "Representante")]
        public string Representante { get; set; }


        [Display(Name = "Desfase Recibos")]
        public Int32? Desfase { get; set; }
        public string Estado { get; set; }
        [Display(Name = "Nro. Departamentos")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Int32? NroDepartamentos { get; set; }
        [Display(Name = "Monto de cuota por defecto")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Decimal? MontoCuota { get; set; }

        [Display(Name = "Saldo Historico")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Decimal? SaldoHistorico { get; set; }

        [Display(Name = "Nro. de Cuenta")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public String NroCuenta { get; set; }



        [Display(Name = "Nro. de Cuenta")]
        public HttpPostedFileBase Firma { get; set; }
        public String RutaFirma { get; set; }
        [Display(Name = "Normas de Convivencia")]
        //[Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public String Ruta { get; set; }


        public string UbigeoId { get; set; }

        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Int32? UDepartamentoId { get; set; }
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Int32? UProvinciaId { get; set; }
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Int32? UDistritoId { get; set; }
        public List<SelectListItem> LstComboDepartamento { get; set; }
        public List<SelectListItem> LstComboProvincia { get; set; }
        public List<SelectListItem> LstComboDistrito { get; set; }

        public String FactorAreaComun { get; set; }
        public String FactorAlcantarillado { get; set; }
        public String FactorCargoFijo { get; set; }

        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Int32 Identificador { get; set; }

        [Display(Name = "Mora")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]

        public Decimal PMora { get; set; }
        [Display(Name = "Tipo de Mora")]
        public string TipoMora { get; set; }
        [Display(Name = "Usar Inquilino")]
        public string UsarInquilinoCCPD { get; set; } = "NO";
        [Display(Name = "Fecha de Vencimiento de Cuotas")]
        public Int32 DiaEmisionCuota { get; set; }
        [Display(Name = "Orden")]
        public Int32? Orden { get; set; }

        //[Display(Name = "NormasFile")]
        public HttpPostedFileBase Archivo { get; set; }

        public String EmailEncargado { get; set; }
        public String NombreEncargado { get; set; }
        public Decimal? PresupuestoMensual { get; set; }
        public String NombrePago { get; set; }
        public String MensajeMora { get; set; }
        [Required]
        public Int32 TipoInmuebleId { get; set; }
        public Int32? DiaMora { get; set; }
        public List<TipoInmueble> LstTipoInmueble { get; set; }
        public Int32? SaldoAnteriorUnidadTiempo { get; set; }
        public List<UnidadTiempo> LstUnidadTiempo { get; set; } = new List<UnidadTiempo>();
        public bool error { get; set; }
        public string mensaje { get; set; }
        public string NombreDepartamento { get; set; } = string.Empty;
        public string NombreProvincia { get; set; } = string.Empty;
        public string NombreDistrito { get; set; } = string.Empty;
        public string NombreTipoInmueble { get; set; } = string.Empty;
        public AddEditEdificioViewModel()
        {
            LstComboDepartamento = new List<SelectListItem>();
            LstComboProvincia = new List<SelectListItem>();
            LstComboDistrito = new List<SelectListItem>();
        }
        public Boolean VerificarIdentificadorEdificio(CargarDatosContext datacontext, string identificador)
        {
            return !datacontext.context.Edificio.Any(x => x.Identificador.ToString().ToLower().Equals(identificador.ToLower()));
        }
        public void Fill(CargarDatosContext datacontext)
        {
            LstTipoInmueble = new List<TipoInmueble>();
            LstTipoInmueble = datacontext.context.TipoInmueble.Where(x => x.Estado == ConstantHelpers.EstadoActivo).ToList();

            baseFill(datacontext);
            var LstDepartamento = datacontext.context.UDepartamento.OrderBy(x => x.Nombre).ToList();
            foreach (var item in LstDepartamento)
                LstComboDepartamento.Add(new SelectListItem { Value = item.UDepartamentoId.ToString(), Text = item.Nombre.ToUpper() });

            var LstProvincia = datacontext.context.UProvincia.OrderBy(x => x.Nombre).ToList();
            foreach (var item in LstProvincia)
                LstComboProvincia.Add(new SelectListItem { Value = item.UProvinciaId.ToString(), Text = item.Nombre.ToUpper() });

            var LstDistrito = datacontext.context.UDistrito.OrderBy(x => x.Nombre).ToList();
            foreach (var item in LstDistrito)
                LstComboDistrito.Add(new SelectListItem { Value = item.UDistritoId.ToString(), Text = item.Nombre.ToUpper() });

            Orden = datacontext.context.Edificio.Max( x => x.Orden) + 1;
            LstUnidadTiempo = datacontext.context.UnidadTiempo.Where(x => x.Estado == ConstantHelpers.EstadoActivo).OrderByDescending( x => x.UnidadTiempoId).ToList();
            TipoMora = "POR";
            DiaEmisionCuota = 1;
            if (EdificioId.HasValue)
            {
                Edificio edificio = datacontext.context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId.Value);
                if (edificio != null)
                {
                    this.SaldoHistorico = edificio.SaldoAnteriorHistorico;
                    this.Acronimo = edificio.Acronimo;
                    this.Nombre = edificio.Nombre;
                    this.Direccion = edificio.Direccion;
                    this.Referencia = edificio.Referencia;
                    this.MensajeMora = edificio.MensajeMora;
                    this.Estado = edificio.Estado;
                    this.UDepartamentoId = edificio.UDepartamentoId;
                    if (edificio.UProvinciaId.HasValue)
                        LstComboProvincia = new Helpers.UbigeoHelper().ListarComboProvincias(this.UDepartamentoId.Value);
                    this.UProvinciaId = edificio.UProvinciaId;
                    if (edificio.UDistritoId.HasValue)
                        LstComboDistrito = new Helpers.UbigeoHelper().ListarComboDistritos(this.UProvinciaId.Value);
                    this.UDistritoId = edificio.UDistritoId;
                    this.NroDepartamentos = edificio.NroDepartamentos;
                    this.MontoCuota = edificio.MontoCuota;
                    this.FactorAreaComun = edificio.FactorAreaComun;
                    this.FactorAlcantarillado = edificio.FactorAlcantarillado;
                    this.FactorCargoFijo = edificio.FactorCargoFijo;
                    this.Identificador = edificio.Identificador;
                    this.PMora = edificio.PMora ?? 0;
                    this.NroCuenta = edificio.NroCuenta;
                    this.Ruta = edificio.NormasConvivencia;
                    this.RutaFirma = edificio.Firma;
                    this.SaldoAnteriorUnidadTiempo = edificio.SaldoAnteriorUnidadTiempo;
                    this.TipoMora = edificio.TipoMora;
                    this.Desfase = edificio.DesfaseRecibos;
                    this.Representante = edificio.Representante;
                    this.EmailEncargado = edificio.EmailEncargado;
                    this.NombreEncargado = edificio.NombreEncargado;
                    this.TipoInmuebleId = edificio.TipoInmuebleId.Value;
                    // if (edificio.FechaVencimientoCuota.HasValue)
                    this.DiaEmisionCuota = edificio.DiaEmisionCuota;
                    this.PresupuestoMensual = edificio.PresupuestoMensual;
                    this.NombrePago = edificio.NombrePago;
                    this.Orden = edificio.Orden;
                    this.DiaMora = edificio.DiaMora;
                    this.UsarInquilinoCCPD = edificio.UsarInquilinoCCPD.HasValue ?  (edificio.UsarInquilinoCCPD.Value ? "SI" : "NO") : "NO";

                    if (edificio.TipoMora == "POR")
                        this.PMora = this.PMora * 100;
                }
            }
        }
        public void FillAPI(SIVEHEntities context)
        {
            LstTipoInmueble = new List<TipoInmueble>();
            LstTipoInmueble = context.TipoInmueble.Where(x => x.Estado == ConstantHelpers.EstadoActivo).ToList();

            var LstDepartamento = context.UDepartamento.OrderBy(x => x.Nombre).ToList();
            foreach (var item in LstDepartamento)
                LstComboDepartamento.Add(new SelectListItem { Value = item.UDepartamentoId.ToString(), Text = item.Nombre.ToUpper() });

            var LstProvincia = context.UProvincia.OrderBy(x => x.Nombre).ToList();
            foreach (var item in LstProvincia)
                LstComboProvincia.Add(new SelectListItem { Value = item.UProvinciaId.ToString(), Text = item.Nombre.ToUpper() });

            var LstDistrito = context.UDistrito.OrderBy(x => x.Nombre).ToList();
            foreach (var item in LstDistrito)
                LstComboDistrito.Add(new SelectListItem { Value = item.UDistritoId.ToString(), Text = item.Nombre.ToUpper() });

            Orden = context.Edificio.Max(x => x.Orden) + 1;
            LstUnidadTiempo = context.UnidadTiempo.Where(x => x.Estado == ConstantHelpers.EstadoActivo).OrderByDescending(x => x.UnidadTiempoId).ToList();
            TipoMora = "POR";
            DiaEmisionCuota = 1;
            if (EdificioId.HasValue)
            {
                Edificio edificio = context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId.Value);
                if (edificio != null)
                {
                    this.SaldoHistorico = edificio.SaldoAnteriorHistorico;
                    this.Acronimo = edificio.Acronimo;
                    this.Nombre = edificio.Nombre;
                    this.Direccion = edificio.Direccion;
                    this.Referencia = edificio.Referencia;
                    this.MensajeMora = edificio.MensajeMora;
                    this.Estado = edificio.Estado;
                    this.UDepartamentoId = edificio.UDepartamentoId;
                    if (edificio.UProvinciaId.HasValue)
                        LstComboProvincia = new Helpers.UbigeoHelper().ListarComboProvincias(this.UDepartamentoId.Value);
                    this.UProvinciaId = edificio.UProvinciaId;
                    if (edificio.UDistritoId.HasValue)
                        LstComboDistrito = new Helpers.UbigeoHelper().ListarComboDistritos(this.UProvinciaId.Value);
                    this.UDistritoId = edificio.UDistritoId;
                    this.NroDepartamentos = edificio.NroDepartamentos;
                    this.MontoCuota = edificio.MontoCuota;
                    this.FactorAreaComun = edificio.FactorAreaComun;
                    this.FactorAlcantarillado = edificio.FactorAlcantarillado;
                    this.FactorCargoFijo = edificio.FactorCargoFijo;
                    this.Identificador = edificio.Identificador;
                    this.PMora = edificio.PMora ?? 0;
                    this.NroCuenta = edificio.NroCuenta;
                    this.Ruta = edificio.NormasConvivencia;
                    this.RutaFirma = edificio.Firma;
                    this.SaldoAnteriorUnidadTiempo = edificio.SaldoAnteriorUnidadTiempo;
                    this.TipoMora = edificio.TipoMora;
                    this.Desfase = edificio.DesfaseRecibos;
                    this.Representante = edificio.Representante;
                    this.EmailEncargado = edificio.EmailEncargado;
                    this.NombreEncargado = edificio.NombreEncargado;
                    this.TipoInmuebleId = edificio.TipoInmuebleId.Value;
                    // if (edificio.FechaVencimientoCuota.HasValue)
                    this.DiaEmisionCuota = edificio.DiaEmisionCuota;
                    this.PresupuestoMensual = edificio.PresupuestoMensual;
                    this.NombrePago = edificio.NombrePago;
                    this.Orden = edificio.Orden;
                    this.DiaMora = edificio.DiaMora;
                    if (edificio.TipoMora == "POR")
                        this.PMora = this.PMora * 100;

                    this.NombreDepartamento = context.UDepartamento.FirstOrDefault( x => x.UDepartamentoId == edificio.UDepartamentoId).Nombre;
                    this.NombreProvincia = context.UProvincia.FirstOrDefault(x => x.UProvinciaId == edificio.UProvinciaId).Nombre;
                    this.NombreDistrito = context.UDistrito.FirstOrDefault(x => x.UDistritoId == edificio.UDistritoId).Nombre;
                    this.NombreTipoInmueble = edificio.TipoInmueble.Nombre;
                    this.UsarInquilinoCCPD = edificio.UsarInquilinoCCPD.HasValue ? (edificio.UsarInquilinoCCPD.Value ? "SI" : "NO") : "NO";

                }
            }
        }
    }
}