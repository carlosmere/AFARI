using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class EnviarCorreoMasivoRequest
    {
        public Int32 usuarioId;
        public Int32 edificioId;
        public string asunto;
        public string mensaje;
        public string cc;
        public List<DestinatarioCorreo> lstDestinatarioCorreo = new List<DestinatarioCorreo>();
    }
    public class DestinatarioCorreo
    {
        public String dpto;
        public String email;
        public String nombre;
    }
}