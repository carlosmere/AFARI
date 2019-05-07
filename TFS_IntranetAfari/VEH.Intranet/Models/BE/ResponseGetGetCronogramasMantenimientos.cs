using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class ResponseGetGetCronogramasMantenimientos : BaseBE
    {
        public List<CronogramaBE> lstCronograma { get; set; } = new List<CronogramaBE>();
    }
}