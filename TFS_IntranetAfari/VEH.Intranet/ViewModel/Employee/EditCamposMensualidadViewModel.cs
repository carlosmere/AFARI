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
    public class EditCamposMensualidadViewModel : BaseViewModel
    {
        public Int32 TrabajadorId { get; set; }
        public Trabajador Trabajador { get; set; }

        public Int32 DetalleMensualidadId { get; set; }
        public DetalleMensualidad DetalleMensualidad { get; set; }

        [Display(Name = "Essalud")]
        public bool tieneEssalud { get; set; }
        [Display(Name = "CTS")]
        public bool tieneCTS { get; set; }

        public EditCamposMensualidadViewModel()
        {

        }

        public void Fill(CargarDatosContext datacontext, Int32 trabajadorId)
        {
            baseFill(datacontext);
            this.TrabajadorId = trabajadorId;
            Trabajador = datacontext.context.Trabajador.Find(trabajadorId);
            DetalleMensualidad = Trabajador.DetalleMensualidad;
            this.DetalleMensualidadId = DetalleMensualidad.DetalleMensualidadId;
            tieneEssalud = DetalleMensualidad.Essalud;
            tieneCTS = DetalleMensualidad.CTS;
        }
    }
}