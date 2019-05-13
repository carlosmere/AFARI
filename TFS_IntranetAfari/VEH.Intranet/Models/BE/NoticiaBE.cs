using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class NoticiaBE
    {
        public int noticiaId { get; set; }
        public String titulo { get; set; }
        public String detalle { get; set; }
        public String fecha { get; set; }
    }
}