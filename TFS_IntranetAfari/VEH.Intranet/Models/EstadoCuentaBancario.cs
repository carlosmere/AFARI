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
    
    public partial class EstadoCuentaBancario
    {
        public int EstadoCuentaBancarioId { get; set; }
        public int UnidadTiempoId { get; set; }
        public int EdificioId { get; set; }
        public string Ruta { get; set; }
        public string Estado { get; set; }
        public string Nombre { get; set; }
    
        public virtual Edificio Edificio { get; set; }
        public virtual UnidadTiempo UnidadTiempo { get; set; }
    }
}
