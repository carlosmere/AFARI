using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class ResponseGetObligacionesLaborales : BaseBE
    {
        public List<ItemOL> lstObligacionesLaborales { get; set; } = new List<ItemOL>();
    }
}