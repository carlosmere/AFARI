using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class UploadConstanciaVisitaResponse
    {
        public bool error { get; set; } = false;
        public string mensaje { get; set; } = String.Empty;
        public string detalle { get; set; } = String.Empty;
    }
}