﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class ResponseGetCategoriaVentaAlquiler : BaseBE
    {
        public List<VehDB_Categoria> lstCategoria { get; set; }
    }
}