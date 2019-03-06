using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Employee
{
    public class EditCamposQuincenaViewModel : BaseViewModel
    {
        public Int32 TrabajadorId { get; set; }
        public Trabajador Trabajador { get; set; }

        public Int32 DetalleQuincenaId { get; set; }
        public DetalleQuincena DetalleQuincena { get; set; }

        [Display(Name="Movilidad")]
        public bool tieneMovilidad { get; set; }
        [Display(Name = "Seguro")]
        public bool tieneSeguro { get; set; }
        [Display(Name = "Bonificación")]
        public bool tieneBonificacion { get; set; }

        public EditCamposQuincenaViewModel()
        {
        }

        public void Fill(CargarDatosContext datacontext, Int32 trabajadorId)
        {
            baseFill(datacontext);
            this.TrabajadorId = trabajadorId;
            Trabajador = datacontext.context.Trabajador.Find(trabajadorId);
            DetalleQuincena = Trabajador.DetalleQuincena;
            this.DetalleQuincenaId = DetalleQuincena.DetalleQuincenaId;
            tieneMovilidad = DetalleQuincena.BonoPorMovilidad;
            tieneSeguro = DetalleQuincena.Seguro;
            tieneBonificacion = DetalleQuincena.Bonificacion;
        }
    }
}