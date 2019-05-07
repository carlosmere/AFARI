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
    
    public partial class Cuota
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Cuota()
        {
            this.ConsumoIndividual = new HashSet<ConsumoIndividual>();
        }
    
        public int CuotaId { get; set; }
        public int DepartamentoId { get; set; }
        public decimal LecturaAgua { get; set; }
        public decimal ConsumoMes { get; set; }
        public decimal ConsumoAgua { get; set; }
        public decimal Monto { get; set; }
        public Nullable<int> Leyenda { get; set; }
        public decimal Mora { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; }
        public int UnidadTiempoId { get; set; }
        public System.DateTime FechaRegistro { get; set; }
        public decimal ConsumoSoles { get; set; }
        public decimal AreaComun { get; set; }
        public decimal Alcantarillado { get; set; }
        public decimal CargoFijo { get; set; }
        public decimal IGV { get; set; }
        public decimal ConsumoAguaTotal { get; set; }
        public Nullable<decimal> CuotaExtraordinaria { get; set; }
        public Nullable<System.DateTime> FechaEmision { get; set; }
        public Nullable<System.DateTime> FechaVencimiento { get; set; }
        public bool Pagado { get; set; }
        public Nullable<System.DateTime> FechaPagado { get; set; }
        public Nullable<decimal> LecturaAnterior { get; set; }
        public Nullable<System.DateTime> FechaLeyenda { get; set; }
        public Nullable<bool> EsExtraordinaria { get; set; }
        public Nullable<bool> EsAdelantado { get; set; }
        public Nullable<decimal> Otros { get; set; }
    
        public virtual Departamento Departamento { get; set; }
        public virtual UnidadTiempo UnidadTiempo { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ConsumoIndividual> ConsumoIndividual { get; set; }
    }
}