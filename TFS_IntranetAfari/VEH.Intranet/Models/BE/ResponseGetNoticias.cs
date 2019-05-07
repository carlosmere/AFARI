using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class ResponseGetNoticias : BaseBE
    {
        public List<NoticiaBE> lstNoticia { get; set; } = new List<NoticiaBE>();
    }
}