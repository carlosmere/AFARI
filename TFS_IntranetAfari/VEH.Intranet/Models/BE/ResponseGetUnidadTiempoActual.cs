using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class ResponseGetUnidadTiempoActual : BaseBE
    {
        public UnidadTiempoBE unidadTiempoActual { get; set; } = new UnidadTiempoBE();
    }
}