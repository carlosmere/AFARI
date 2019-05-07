using System;

namespace VEH.Intranet.Models.BE
{
    public class TrabajadorBE
    {
       public Int32 trabajadorId { get; set; }
       public String nombre { get; set; }
       public String cargo { get; set; }
       public String dni { get; set; }
       public String foto { get; set; }
    }
}