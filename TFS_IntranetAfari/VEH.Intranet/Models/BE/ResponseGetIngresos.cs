using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class ResponseGetIngresos : BaseBE
    {
        public List<IngresoBE> lstIngreso { get; set; } = new List<IngresoBE>();
    }
}