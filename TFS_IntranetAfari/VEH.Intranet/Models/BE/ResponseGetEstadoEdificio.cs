using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEH.Intranet.Models.BE
{
    public class ResponseGetEstadoEdificio : BaseBE
    {
        public decimal ingresos { get; set; } = 0;
        public decimal gastos { get; set; } = 0;
        public decimal saldo { get; set; } = 0;
        public decimal saldoAnterior { get; set; } = 0;
        public decimal saldoAcumulado { get; set; } = 0;
    }
}