using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.News
{
    public class AddEditNoticiaViewModel : BaseViewModel
    {

        public Int32? NoticiaId { get; set; }
        //[Display(Name = "Título")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public String Titulo { get; set; }
        //[Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public String Sumilla { get; set; }
        //[Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public String Detalle { get; set; }
        public String Estado { get; set; }
        public Int32 EdificioId { get; set; }
        public DateTime? Fecha { get; set; }
        public AddEditNoticiaViewModel() { }

        public Edificio Edificio { get; set; }
        public void Fill(CargarDatosContext datacontext)
        {
            baseFill(datacontext);
            Edificio = datacontext.context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);
            if (NoticiaId.HasValue)
            {
                Noticia noticia = datacontext.context.Noticia.FirstOrDefault(x => x.NoticiaId == NoticiaId.Value);
                this.Titulo = noticia.Titulo;
                this.Sumilla = noticia.Sumilla;
                this.Estado = noticia.Estado;
                this.Fecha = noticia.Fecha;
                this.Detalle = noticia.Detalle;
            }
        }
    }
}