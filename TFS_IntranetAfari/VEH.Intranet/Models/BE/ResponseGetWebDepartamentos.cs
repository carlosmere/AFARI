using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class ResponseGetWebDepartamentos : BaseBE
    {
        public List<AfariDB_Departamento> lstDepartamento { get; set; }
    }
}