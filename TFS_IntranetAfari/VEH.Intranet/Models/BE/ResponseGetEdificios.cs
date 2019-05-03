using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class ResponseGetEdificios : BaseBE
    {
        public List<EdificioBE> lstEdificios { get; set; } = new List<EdificioBE>();
    }
}