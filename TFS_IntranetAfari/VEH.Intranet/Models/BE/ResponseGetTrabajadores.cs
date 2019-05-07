using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class ResponseGetTrabajadores : BaseBE
    {
        public List<TrabajadorBE> lstTrabajador { get; set; } = new List<TrabajadorBE>();
    }
}