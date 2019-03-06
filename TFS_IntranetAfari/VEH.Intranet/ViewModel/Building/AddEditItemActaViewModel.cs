using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Building
{
    public class AddEditItemActaViewModel : BaseViewModel
    {
        public Int32? ItemActaId { get; set; }
        public string Nombre { get; set; }
        public HttpPostedFileBase Ruta { get; set; }
        public DateTime? Fecha { get; set; }
        public int EdificioId { get; set; }
        public List<UnidadTiempo> LstUnidadTiempo { get; set; } = new List<UnidadTiempo>();
        public int UnidadTiempoId { get; set; }
        public String RutaArchivo { get; set; }
        public String NombreEdificio { get; set; }
        public void Fill(CargarDatosContext c, Int32? itemActaId, Int32 edificioId)
        {
            this.ItemActaId = itemActaId;
            this.EdificioId = edificioId;
            LstUnidadTiempo = c.context.UnidadTiempo.Where(x => x.Estado == ConstantHelpers.EstadoActivo).OrderByDescending(x => x.UnidadTiempoId).ToList();
            if (this.ItemActaId.HasValue)
            {
                var item = c.context.ItemActa.FirstOrDefault(x => x.ItemActaId == this.ItemActaId);
                this.Nombre = item.Nombre;
                this.RutaArchivo = item.Ruta;
                this.Fecha = item.Fecha;
                this.EdificioId = item.EdificioId;
                this.UnidadTiempoId = item.UnidadTiempoId;
            }
            NombreEdificio = c.context.Edificio.FirstOrDefault(x => x.EdificioId == this.EdificioId).Nombre;
        }
    }
}