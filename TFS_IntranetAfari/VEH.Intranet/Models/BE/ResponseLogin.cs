﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class ResponseLogin : BaseBE
    {
        public string nombre { get; set; }
        public Int32 usuarioId { get; set; }
        public string rol { get; set; }
        public string correo { get; set; }
        public Int32? departamentoId { get; set; }
        public Int32? edificioId { get; set; }
        public string nombreEdificio { get; set; }
        public string nombreDepartamento { get; set; }
        public string ultimaPagada { get; set; }
    }
}