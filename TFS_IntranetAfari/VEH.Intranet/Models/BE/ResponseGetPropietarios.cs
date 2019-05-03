using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class ResponseGetPropietarios : BaseBE
    {
        public List<PropietarioBE> lstPropietario { get; set; } = new List<PropietarioBE>();
    }
}