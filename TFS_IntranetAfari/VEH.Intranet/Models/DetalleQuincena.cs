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
    
    public partial class DetalleQuincena
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DetalleQuincena()
        {
            this.Trabajador = new HashSet<Trabajador>();
        }
    
        public int DetalleQuincenaId { get; set; }
        public bool BonoPorMovilidad { get; set; }
        public bool Bonificacion { get; set; }
        public bool TotalQuincena { get; set; }
        public bool Seguro { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Trabajador> Trabajador { get; set; }
    }
}