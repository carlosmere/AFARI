using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using System.Web.Mvc;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Employee
{
    public class AddEditTrabajadorViewModel : BaseViewModel
    {
        public Int32? TrabajadorId { get; set; }
        [Display(Name = "Nombres")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public String Nombres { get; set; }
        [Display(Name = "Apellidos")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public String Apellidos { get; set; }
        [Display(Name = "DNI")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public String DNI { get; set; }
        [Display(Name = "Fecha de nacimiento")]
        public DateTime? FechaNacimiento { get; set; }
        //[Display(Name = "Foto")]
        public String Foto { get; set; }
        [Display(Name = "Cargo")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public String Cargo { get; set; }
        [Display(Name = "AFP")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Int32 AFP { get; set; }
        [Display(Name = "CUSSP")]
        public String CUSSP { get; set; }
        [Display(Name = "Comisión")]
        public Decimal? Comision { get; set; }
        [Display(Name = "Fecha de ingreso")]
        public DateTime? FechaIngreso { get; set; }
        public String Antecedentes { get; set; }
        public String Partida { get; set; }
       
        public String Modalidad { get; set; }
        public Decimal? SueldoBase { get; set; }
        public String Estado { get; set; }

        [Display(Name = "Edificio")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Int32 EdificioId { get; set; }
        [Display(Name = "Foto")]
        public HttpPostedFileBase FotoFile { get; set; }
        [Display(Name = "Antecedentes Policiales")]
        public HttpPostedFileBase AntecedenteFile { get; set; }
        [Display (Name ="Curriculum Vitae")]
        public HttpPostedFileBase PartidaFile { get; set; }

        public Decimal MontoHoras25 { get; set; }

        public Decimal MontoHoras35 { get; set; }

        public Decimal MontoFeriado { get; set; }

        public Decimal AdelantoQuincena { get; set; }

        public String ComisionFlujo { get; set; }


        public List<SelectListItem> LstComboEdificio { get; set; }
        public List<SelectListItem> LstAFP { get; set; }
        public List<SelectListItem> LstComisionFlujo { get; set; }

        public AddEditTrabajadorViewModel()
        {
            LstAFP = new List<SelectListItem>();
            LstComboEdificio = new List<SelectListItem>();
            LstComisionFlujo = new List<SelectListItem>();
        }
        public void Fill(CargarDatosContext datacontext)
        {
            baseFill(datacontext);
            var edificios = datacontext.context.Edificio.OrderBy(x => x.Nombre).Where(x => x.Estado == ConstantHelpers.EstadoActivo);
            foreach (var item in edificios)
                LstComboEdificio.Add(new SelectListItem { Value = item.EdificioId.ToString(), Text = item.Nombre });

            var afps = datacontext.context.AFP.OrderBy(x => x.Nombre);
            LstAFP.Add(new SelectListItem { Value = "0",  Text = "Sin AFP" });
            foreach (var item in afps)
                LstAFP.Add(new SelectListItem { Value = item.AFPId.ToString(), Text = item.Nombre });

            LstComisionFlujo.Add(new SelectListItem { Value = ConstantHelpers.COMISION_MENSUAL, Text = ConstantHelpers.COMISION_MENSUAL_TEXT, });
            LstComisionFlujo.Add(new SelectListItem { Value = ConstantHelpers.COMISION_ANUAL, Text = ConstantHelpers.COMISION_ANUAL_TEXT});

            

            if (TrabajadorId.HasValue)
            {
                Trabajador trabajador = datacontext.context.Trabajador.FirstOrDefault(x => x.TrabajadorId == TrabajadorId.Value);
                if (trabajador != null)
                {
                    this.TrabajadorId = trabajador.TrabajadorId;
                    this.Nombres = trabajador.Nombres;
                    this.Apellidos = trabajador.Apellidos;
                    this.DNI = trabajador.DNI;
                    this.FechaNacimiento = trabajador.FechaNacimiento;
                    this.Estado = trabajador.Estado;
                    this.Foto = trabajador.Foto;
                    this.Cargo = trabajador.Cargo;
                    this.AFP = trabajador.AFPId??0;
                    this.CUSSP = trabajador.CUSSP;
                    this.Comision = trabajador.Comision;
                    this.FechaIngreso = trabajador.FechaIngreso;
                    this.Antecedentes = trabajador.AntecedentesPoliciales;
                    this.Partida = trabajador.PartidaNacimiento;
                    this.EdificioId = trabajador.EdificioId;
                    this.Modalidad = trabajador.Modalidad;
                    this.SueldoBase = trabajador.SueldoBase;
                    this.MontoHoras25 = trabajador.MontoHoras25.Value;
                    this.MontoHoras35 = trabajador.MontoHoras35.Value;
                    this.MontoFeriado = trabajador.MontoFeriado.Value;
                    this.AdelantoQuincena = trabajador.AdelantoQuincena ??0;
                    this.ComisionFlujo = trabajador.ComisionFlujo;
                }
            }
        }
    }
}