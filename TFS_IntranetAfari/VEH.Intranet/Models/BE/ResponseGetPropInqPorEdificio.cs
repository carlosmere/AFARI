using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class ResponseGetPropInqPorEdificio : BaseBE
    {
        public List<PropInqBE> lstProInq { get; set; } = new List<PropInqBE>();
    }
}