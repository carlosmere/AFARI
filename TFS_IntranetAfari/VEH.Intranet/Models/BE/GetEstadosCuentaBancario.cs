using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class GetEstadosCuentaBancario : BaseBE
    {
        public List<EstadoCuentaBancarioBE> lstEstadoCuenta { get; set; } = new List<EstadoCuentaBancarioBE>();
    }
}