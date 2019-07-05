using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class ResponseGetComprobantesPago : BaseBE
    {
        public List<ArchivoGastoBE> LstArchivoGasto { get; set; } = new List<ArchivoGastoBE>();
    }
}