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
    
    public partial class UnidadTiempoReciboDepartamento
    {
        public int UnidadTiempoReciboDepartamentoId { get; set; }
        public int DepartamentoId { get; set; }
        public int UnidadTiempoId { get; set; }
        public long NumeroRecibo { get; set; }
    
        public virtual Departamento Departamento { get; set; }
        public virtual UnidadTiempo UnidadTiempo { get; set; }
    }
}
