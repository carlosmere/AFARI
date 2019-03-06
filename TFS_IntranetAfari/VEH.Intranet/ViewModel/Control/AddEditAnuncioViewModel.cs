using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Control
{
    public class AddEditAnuncioViewModel : BaseViewModel
    {
        public Int32? AnuncioId { get; set; }
        [Required]
        public String Nombre { get; set; }

        public HttpPostedFileBase Archivo { get; set; }
        public String Ruta { get; set; }
        [Required]
        public String Url { get; set; }
        public String Descripcion { get; set; }
        public Int32 Prioridad { get; set; }
        public void Fill(CargarDatosContext c, Int32? anuncioId)
        {
            this.AnuncioId = anuncioId;
            if (this.AnuncioId.HasValue)
            {
                var anuncio = c.context.Anuncio.FirstOrDefault( x => x.AnuncioId == this.AnuncioId);
                this.Nombre = anuncio.Nombre;
                this.Ruta = anuncio.Ruta;
                this.Url = anuncio.Url;
                this.Prioridad = anuncio.Prioridad;
                this.Descripcion = anuncio.Descripcion;
            }
        }
    }
}