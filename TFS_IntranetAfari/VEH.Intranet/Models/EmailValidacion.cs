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
    
    public partial class EmailValidacion
    {
        public int EmailValidacionId { get; set; }
        public string Destinatarios { get; set; }
        public string CopiaCarbon { get; set; }
        public string Asunto { get; set; }
        public int UsuarioId { get; set; }
        public int EdificioId { get; set; }
        public string Mensaje { get; set; }
        public string CopiaOculta { get; set; }
    
        public virtual Edificio Edificio { get; set; }
        public virtual Usuario Usuario { get; set; }
    }
}
