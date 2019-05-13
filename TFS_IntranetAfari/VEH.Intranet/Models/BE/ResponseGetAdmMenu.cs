using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class ResponseGetAdmMenu : BaseBE
    {
        public List<AdmMenuBE> lstAdmMenuBE { get; set; } = new List<AdmMenuBE>();
    }
}