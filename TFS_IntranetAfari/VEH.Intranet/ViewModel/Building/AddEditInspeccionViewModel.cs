using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Models;

using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using VEH.Intranet.Helpers;
using VEH.Intranet.ViewModel.Shared;
namespace VEH.Intranet.ViewModel.Building
{
    public class AddEditInspeccionViewModel : BaseViewModel
    {
        public Int32? InspeccionId { get; set; }

        [Display(Name = "Nombre")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public string Nombre { get; set; }
        [Display(Name = "Fecha")]
        public DateTime Fecha { get; set; }
        [Display(Name = "Pregunta1")]
        public Int32 Pregunta1 { get; set; }
        [Display(Name = "Pregunta2")]
        public Int32 Pregunta2 { get; set; }
        [Display(Name = "Pregunta3")]
        public Int32 Pregunta3 { get; set; }
        [Display(Name = "Pregunta4")]
        public Int32 Pregunta4 { get; set; }
        [Display(Name = "Pregunta5")]
        public Int32 Pregunta5 { get; set; }
        [Display(Name = "Pregunta6")]
        public Int32 Pregunta6 { get; set; }
        [Display(Name = "Pregunta7")]
        public Int32 Pregunta7 { get; set; }
        [Display(Name = "Pregunta8")]
        public Int32 Pregunta8 { get; set; }
        [Display(Name = "Pregunta9")]
        public Int32 Pregunta9 { get; set; }
        [Display(Name = "Pregunta10")]
        public Int32 Pregunta10 { get; set; }
        [Display(Name = "Pregunta11")]
        public Int32 Pregunta11 { get; set; }
        [Display(Name = "Pregunta12")]
        public Int32 Pregunta12 { get; set; }
        [Display(Name = "Pregunta13")]
        public Int32 Pregunta13 { get; set; }
        [Display(Name = "Pregunta14")]
        public Int32 Pregunta14 { get; set; }
        [Display(Name = "Pregunta15")]
        public Int32 Pregunta15 { get; set; }
        [Display(Name = "Pregunta16")]
        public Int32 Pregunta16 { get; set; }
        [Display(Name = "Pregunta17")]
        public Int32 Pregunta17 { get; set; }
        [Display(Name = "Pregunta18")]
        public Int32 Pregunta18 { get; set; }
        [Display(Name = "Pregunta19")]
        public Int32 Pregunta19 { get; set; }
        [Display(Name = "Pregunta20")]
        public Int32 Pregunta20 { get; set; }
        [Display(Name = "Pregunta21")]
        public Int32 Pregunta21 { get; set; }
        [Display(Name = "Pregunta22")]
        public Int32 Pregunta22 { get; set; }
        [Display(Name = "Pregunta23")]
        public Int32 Pregunta23 { get; set; }
        [Display(Name = "Pregunta24")]
        public Int32 Pregunta24 { get; set; }
        [Display(Name = "Pregunta25")]
        public Int32 Pregunta25 { get; set; }
        [Display(Name = "Pregunta26")]
        public Int32 Pregunta26 { get; set; }
        [Display(Name = "Pregunta27")]
        public Int32 Pregunta27 { get; set; }
        [Display(Name = "Pregunta28")]
        public Int32 Pregunta28 { get; set; }
        [Display(Name = "Pregunta29")]
        public Int32 Pregunta29 { get; set; }
        [Display(Name = "Pregunta30")]
        public Int32 Pregunta30 { get; set; }
        [Display(Name = "Pregunta31")]
        public Int32 Pregunta31 { get; set; }
        [Display(Name = "Pregunta32")]
        public Int32 Pregunta32 { get; set; }
        [Display(Name = "Pregunta33")]
        public Int32 Pregunta33 { get; set; }
        [Display(Name = "Pregunta34")]
        public Int32 Pregunta34 { get; set; }
        [Display(Name = "Pregunta35")]
        public Int32 Pregunta35 { get; set; }
        [Display(Name = "Pregunta36")]
        public Int32 Pregunta36 { get; set; }

        [Display(Name = "Enunciado1")]
        public String Enunciado1 { get; set; }
        [Display(Name = "Enunciado2")]
        public String Enunciado2 { get; set; }
        [Display(Name = "Enunciado3")]
        public String Enunciado3 { get; set; }
        [Display(Name = "Enunciado4")]
        public String Enunciado4 { get; set; }
        [Display(Name = "Enunciado5")]
        public String Enunciado5 { get; set; }
        [Display(Name = "Enunciado6")]
        public String Enunciado6 { get; set; }
        [Display(Name = "Enunciado7")]
        public String Enunciado7 { get; set; }
        [Display(Name = "Enunciado8")]
        public String Enunciado8 { get; set; }
        [Display(Name = "Enunciado9")]
        public String Enunciado9 { get; set; }

        [Display(Name = "Observaciones")]
        public String Observaciones { get; set; }
        
        public List<NivelEstado> LstNivelEstado { get; set; }
        public Int32 EdificioId { get; set; }

        public AddEditInspeccionViewModel()
        {
        }
        public void Fill(CargarDatosContext datacontext)
        {
            baseFill(datacontext);

            LstNivelEstado = datacontext.context.NivelEstado.ToList();


            if (InspeccionId.HasValue)
            {
                Inspeccion inspeccion = datacontext.context.Inspeccion.FirstOrDefault(x => x.InspeccionId == InspeccionId.Value);
                EdificioId = inspeccion.EdificioId;
                if (inspeccion != null)
                {
                    this.Nombre = inspeccion.Nombre;
                    this.Fecha = inspeccion.Fecha;
                    this.Pregunta1 = inspeccion.Pregunta1;
                    this.Pregunta2 = inspeccion.Pregunta2;
                    this.Pregunta3 = inspeccion.Pregunta3;
                    this.Pregunta4 = inspeccion.Pregunta4;
                    this.Pregunta5 = inspeccion.Pregunta5;
                    this.Pregunta6 = inspeccion.Pregunta6;
                    this.Pregunta7 = inspeccion.Pregunta7;
                    this.Pregunta8 = inspeccion.Pregunta8;
                    this.Pregunta9 = inspeccion.Pregunta9;
                    this.Pregunta10 = inspeccion.Pregunta10;
                    this.Pregunta11 = inspeccion.Pregunta11;
                    this.Pregunta12 = inspeccion.Pregunta12;
                    this.Pregunta13 = inspeccion.Pregunta13;
                    this.Pregunta14 = inspeccion.Pregunta14;
                    this.Pregunta15 = inspeccion.Pregunta15;
                    this.Pregunta16 = inspeccion.Pregunta16; 
                    this.Pregunta17 = inspeccion.Pregunta17;
                    this.Pregunta18 = inspeccion.Pregunta18;
                    this.Pregunta19 = inspeccion.Pregunta19;
                    this.Pregunta20 = inspeccion.Pregunta20;
                    this.Pregunta21 = inspeccion.Pregunta21;
                    this.Pregunta22 = inspeccion.Pregunta22;
                    this.Pregunta23 = inspeccion.Pregunta23;
                    this.Pregunta24 = inspeccion.Pregunta24;
                    this.Pregunta25 = inspeccion.Pregunta25;
                    this.Pregunta26 = inspeccion.Pregunta26;
                    this.Pregunta27 = inspeccion.Pregunta27;
                    this.Pregunta28 = inspeccion.Pregunta28;
                    this.Pregunta29 = inspeccion.Pregunta29;
                    this.Pregunta30 = inspeccion.Pregunta30;
                    this.Pregunta31 = inspeccion.Pregunta31;
                    this.Pregunta32 = inspeccion.Pregunta32;
                    this.Pregunta33 = inspeccion.Pregunta33;
                    this.Pregunta34 = inspeccion.Pregunta34;
                    this.Pregunta35 = inspeccion.Pregunta35;
                    this.Pregunta36 = inspeccion.Pregunta36;
                    
                    this.Enunciado1 = inspeccion.Enunciado1;
                    this.Enunciado2 = inspeccion.Enunciado2;
                    this.Enunciado3 = inspeccion.Enunciado3;
                    this.Enunciado4 = inspeccion.Enunciado4;
                    this.Enunciado5 = inspeccion.Enunciado5;
                    this.Enunciado6 = inspeccion.Enunciado6;
                    this.Enunciado7 = inspeccion.Enunciado7;
                    this.Enunciado8 = inspeccion.Enunciado8;
                    this.Enunciado9 = inspeccion.Enunciado9;

                    this.Observaciones = inspeccion.Observaciones;

                }
            }
        }
    }
}