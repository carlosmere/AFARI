using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class BaseBE
    {
        public bool error { get; set; } = false;
        public string mensaje { get; set; } = String.Empty;
    }
}