using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class EdificioDetalleBE
    {
        public string Acronimo { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public string Referencia { get; set; }
        public string Representante { get; set; }
        public Int32? Desfase { get; set; }
        public string Estado { get; set; }
        public Int32? NroDepartamentos { get; set; }
        public Decimal? MontoCuota { get; set; }
        public Decimal? SaldoHistorico { get; set; }
        public String NroCuenta { get; set; }
        public String RutaFirma { get; set; }
        public String Ruta { get; set; }
        public string UbigeoId { get; set; }
        public Int32? UDepartamentoId { get; set; }
        public Int32? UProvinciaId { get; set; }
        public Int32? UDistritoId { get; set; }
        public String FactorAreaComun { get; set; }
        public String FactorAlcantarillado { get; set; }
        public String FactorCargoFijo { get; set; }
        public Int32 Identificador { get; set; }
        public Decimal PMora { get; set; }
        public string TipoMora { get; set; }
        public Int32 DiaEmisionCuota { get; set; }
        public Int32? Orden { get; set; }
        public String EmailEncargado { get; set; }
        public String NombreEncargado { get; set; }
        public Decimal? PresupuestoMensual { get; set; }
        public String NombrePago { get; set; }
        public String MensajeMora { get; set; }
        public Int32 TipoInmuebleId { get; set; }
        public Int32? DiaMora { get; set; }
        public Int32? SaldoAnteriorUnidadTiempo { get; set; }
        public String NombreDepartamento { get; set; } = String.Empty;
        public String NombreProvincia { get; set; } = String.Empty;
        public String NombreDistrito { get; set; } = String.Empty;
        public String NombreTipoInmueble { get; set; } = String.Empty;
    }
}