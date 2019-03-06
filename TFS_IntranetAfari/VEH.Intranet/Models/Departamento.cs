//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace VEH.Intranet.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Departamento
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Departamento()
        {
            this.Cuota = new HashSet<Cuota>();
            this.DepartamentoHistorico = new HashSet<DepartamentoHistorico>();
            this.DepartamentoPropietario = new HashSet<DepartamentoPropietario>();
            this.Propietario = new HashSet<Propietario>();
            this.UnidadTiempoReciboDepartamento = new HashSet<UnidadTiempoReciboDepartamento>();
            this.Usuario = new HashSet<Usuario>();
            this.Visita = new HashSet<Visita>();
        }
    
        public int DepartamentoId { get; set; }
        public string Numero { get; set; }
        public int Piso { get; set; }
        public string Estado { get; set; }
        public int EdificioId { get; set; }
        public decimal LecturaAgua { get; set; }
        public Nullable<decimal> FactorGasto { get; set; }
        public string Estacionamiento { get; set; }
        public string Deposito { get; set; }
        public Nullable<decimal> DepartamentoM2 { get; set; }
        public Nullable<decimal> EstacionamientoM2 { get; set; }
        public Nullable<decimal> DepositoM2 { get; set; }
        public Nullable<decimal> PDistribucion { get; set; }
        public decimal MontoMora { get; set; }
        public bool OmitirMora { get; set; }
        public Nullable<System.DateTime> FechaPago { get; set; }
        public Nullable<decimal> CuotaDefault { get; set; }
        public Nullable<decimal> TotalM2 { get; set; }
        public Nullable<int> TipoInmuebleId { get; set; }
        public string NombreRecibo { get; set; }
        public bool AlertaMora { get; set; }
        public bool NombrePropietario { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Cuota> Cuota { get; set; }
        public virtual Edificio Edificio { get; set; }
        public virtual TipoInmueble TipoInmueble { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DepartamentoHistorico> DepartamentoHistorico { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DepartamentoPropietario> DepartamentoPropietario { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Propietario> Propietario { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UnidadTiempoReciboDepartamento> UnidadTiempoReciboDepartamento { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Usuario> Usuario { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Visita> Visita { get; set; }
    }
}
