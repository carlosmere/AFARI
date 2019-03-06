using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.Controllers;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Building
{
    public class UploadReportViewModel : BaseViewModel
    {
        public Int32 EdificioId { get; set; }

        public Edificio Edificio { get; set; }
        public Int32? UnidadTiempoId { get; set; }
        public HttpPostedFileBase archivoReporte { get; set; }
        public String RutaArchivo { get; set; }
        public String NombreArchivo { get; set; }

        public List<SelectListItem> LstComboUnidadTiempo { get; set; }
        public void fill(CargarDatosContext datacontext,Int32 EdificioId,Int32? UnidadTiempoId)
        {
            baseFill(datacontext);
            this.EdificioId=EdificioId;
            this.UnidadTiempoId = UnidadTiempoId;
            Edificio = datacontext.context.Edificio.First(X => X.EdificioId == EdificioId);
            LstComboUnidadTiempo = new List<SelectListItem>();

            var lstunidadtiempo = datacontext.context.UnidadTiempo.OrderByDescending(x => x.Anio).OrderByDescending(x => x.Mes).Where(x => x.Estado == ConstantHelpers.EstadoActivo);
            foreach (var item in lstunidadtiempo)
                LstComboUnidadTiempo.Add(new SelectListItem { Value = item.UnidadTiempoId.ToString(), Text = item.Descripcion.ToUpper() });

            if(UnidadTiempoId.HasValue)
            {

                var Archivo = datacontext.context.ReporteEdificioUnidadTiempo.FirstOrDefault(X => X.EdificioId == EdificioId && X.UnidadTiempoId == UnidadTiempoId.Value);

                if (Archivo == null)
                {
                    RutaArchivo = "";
                    NombreArchivo = "No se ha subido ningún archivo";
                }
                else
                {
                    RutaArchivo = Archivo.Ruta;
                    NombreArchivo = Archivo.Nombre;
                }
            }
        }
    }
}