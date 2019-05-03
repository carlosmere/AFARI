using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class ResponseGetUnidadTiempo : BaseBE
    {
        public List<UnidadTiempoBE> lstUnidadTiempo { get; set; } = new List<UnidadTiempoBE>();
    }
}