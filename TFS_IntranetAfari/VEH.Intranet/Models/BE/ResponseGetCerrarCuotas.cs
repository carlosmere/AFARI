using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class ResponseGetCerrarCuotas : BaseBE
    {
        public decimal? mora { get; set; } = 0;
        public string tipoMora { get; set; } = String.Empty;
        public string diaMesConsiderar { get; set; } = String.Empty;
        public List<CuotaBE> lstCuota = new List<CuotaBE>();
    }
}