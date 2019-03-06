using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;
namespace VEH.Intranet.ViewModel.Building
{
    public class ArchivosCorregidosViewModel : BaseViewModel
    {


        public List<ArchivoCorrecionEdificio> LstArchivoCorrecion { get; set; }
        public Int32 UnidadTiempoId { get; set; }
        public Int32 EdificioId { get; set; }
        public List<UnidadTiempo> LstUnidadTiempo { get; set; }
        public void fill(Controllers.CargarDatosContext cargarDatosContext, int UnidadTiempoId, int EdificioId)
        {
            baseFill(cargarDatosContext);
            var c = cargarDatosContext.context;
            this.EdificioId = EdificioId;
            this.UnidadTiempoId = UnidadTiempoId == -1 ? c.UnidadTiempo.FirstOrDefault(X=>X.EsActivo).UnidadTiempoId : UnidadTiempoId;
            LstUnidadTiempo = c.UnidadTiempo.Where( x => x.Estado == ConstantHelpers.EstadoActivo).OrderByDescending( x => x.Orden).ToList();
            LstArchivoCorrecion = c.ArchivoCorrecionEdificio.Where(X => X.UnidadTiempoId == UnidadTiempoId && X.EdificioId == EdificioId).ToList();
        }
    }
}