using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class ResponseGetDepartamentos : BaseBE
    {
        public List<DepartamentoBE> lstDepartamento { get; set; } = new List<DepartamentoBE>();
    }
}