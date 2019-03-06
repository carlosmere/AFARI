using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;
namespace VEH.Intranet.ViewModel.Building
{
    public class AddEditArchivoCorregidoViewModel : BaseViewModel
    {


        public Int32? ArchivoCorregidoId { get; set; }
        public Int32 EdificioId { get; set; }
        public Int32 UnidadTiempoId { get; set; }
        public String Tipo { get; set; }
        public String Departamento { get; set; }
        public HttpPostedFileBase archivo { get; set; }
        public List<Departamento> LstDepartamentos { get; set; }

        public void fill(Controllers.CargarDatosContext cargarDatosContext,Int32 edificioId)
        {
            baseFill(cargarDatosContext);
            LstDepartamentos = cargarDatosContext.context.Departamento.Where(X => X.EdificioId == edificioId).ToList();
           if(ArchivoCorregidoId.HasValue)
           {
               var archivoCorregido = cargarDatosContext.context.ArchivoCorrecionEdificio.FirstOrDefault(X=>X.ArchivoCorrecionEdificioId == ArchivoCorregidoId.Value);
               if(archivoCorregido == null)
                   return;
               Tipo = archivoCorregido.Tipo;
           }
        }
    }
}