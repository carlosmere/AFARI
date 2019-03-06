using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Models;
using VEH.Intranet.Controllers;
using VEH.Intranet.Helpers;
using System.ComponentModel.DataAnnotations;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Income
{
    public class AddEditDetalleIngresoViewModel : BaseViewModel
    {
        public Int32? DetalleIngresoId { get; set; }
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public String Concepto { get; set; }
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Decimal? Monto { get; set; }
        public Decimal MontoAdicional { get; set; }
        public Int32 EdificioId { get; set; }
        public String Estado { get; set; }
        public Int32 IngresoId { get; set; }
        public String DescripcionUnidadMedida { get; set; }
        public Edificio Edificio { get; set; }
        public Boolean Pagado { get; set; }
       

        public AddEditDetalleIngresoViewModel()
        {
            MontoAdicional = 0;
        }

        public void Fill(CargarDatosContext datacontext)
        {
            baseFill(datacontext);
            Edificio = datacontext.context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);
            DescripcionUnidadMedida = datacontext.context.Ingreso.FirstOrDefault(x => x.IngresoId == IngresoId).UnidadTiempo.Descripcion.ToUpper();
            if (DetalleIngresoId.HasValue)
            {
                DetalleIngreso detalleingreso = datacontext.context.DetalleIngreso.FirstOrDefault(x => x.DetalleIngresoId == DetalleIngresoId.Value);
                if (detalleingreso != null)
                {
                    this.Concepto = detalleingreso.Concepto;
                    this.DetalleIngresoId = detalleingreso.DetalleIngresoId;
                    this.Monto = detalleingreso.Monto;
                    this.Estado = detalleingreso.Estado;
                    this.IngresoId = detalleingreso.IngresoId;
                    this.Pagado = detalleingreso.Pagado;
                }

            }
            else
            {
                this.Pagado = true;
                
            }
        }
    }
}