using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class ResponseGetDetalleEdificio : BaseBE
    {
        public EdificioDetalleBE detalleEdificio { get; set; } = new EdificioDetalleBE();
    }
}