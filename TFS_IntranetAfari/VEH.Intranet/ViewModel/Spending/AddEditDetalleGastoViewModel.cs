using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Models;
using VEH.Intranet.Controllers;
using VEH.Intranet.Helpers;
using System.ComponentModel.DataAnnotations;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Spending
{
    public class AddEditDetalleGastoViewModel : BaseViewModel
    {
        public Int32? DetalleGastoId { get; set; }
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public String Concepto { get; set; }
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Decimal? Monto { get; set; }
        public String MontoAdicional { get; set; }
        public Int32 EdificioId { get; set; }
        public String Estado { get; set; }
        public Int32 GastoId { get; set; }
        public String DescripcionUnidadMedida { get; set; }
        public Edificio Edificio { get; set; }
        public Boolean Pagado { get; set; }
        public Boolean Ordinario { get; set; }
        public Int32? Orden { get; set; }
        public List<Tuple<String, String>> detalles { get; set; }
        public AddEditDetalleGastoViewModel()
        {
            MontoAdicional = "0";
        }

        public void Fill(CargarDatosContext datacontext)
        {
            baseFill(datacontext);
            Edificio = datacontext.context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);
            var gasto = datacontext.context.Gasto.FirstOrDefault(x => x.GastoId == GastoId);
            DescripcionUnidadMedida = gasto.UnidadTiempo.Descripcion.ToUpper();
            this.Orden = datacontext.context.DetalleGasto.Where(x => x.GastoId == gasto.GastoId && x.Estado == ConstantHelpers.EstadoActivo).Max( x => x.Orden) + 1;
            detalles = new List<Tuple<string, string>>();
            if (DetalleGastoId.HasValue)
            {
                DetalleGasto detallegasto = datacontext.context.DetalleGasto.FirstOrDefault(x => x.DetalleGastoId == DetalleGastoId.Value);
                if (detallegasto != null)
                {
                    this.Concepto = detallegasto.Concepto;
                    this.DetalleGastoId = detallegasto.DetalleGastoId;
                    this.Monto = detallegasto.Monto;
                    this.Estado = detallegasto.Estado;
                    this.GastoId = detallegasto.GastoId;
                    this.Pagado = detallegasto.Pagado;
                    this.Ordinario = detallegasto.Ordinario;
                    this.Orden = detallegasto.Orden;
                    if (!String.IsNullOrWhiteSpace(detallegasto.Detalle))
                    {
                        detalles.AddRange(detallegasto.Detalle.Split('|').Where(X => !String.IsNullOrWhiteSpace(X)).Select(X => new Tuple<String, String>(X.Split(';').First(), X.Split(';').Last())).ToList());
                    }


                 
                }
                else
                {
                    this.Pagado = true;
                    this.Ordinario = true;
                }
            }
        }
    }
}