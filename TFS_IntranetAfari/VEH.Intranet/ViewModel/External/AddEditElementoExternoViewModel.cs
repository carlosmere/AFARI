using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.External
{
    public class AddEditElementoExternoViewModel : BaseViewModel
    {
        public Int32 EdificioId { get; set; }
        public Int32? ElementoExternoId{ get; set; }
        public HttpPostedFileBase Documento { get; set; }
        public String DocumentoAnterior { get; set; }
        public String Nombre { get; set; }
        public String Icono { get; set; }

        public void Fill(CargarDatosContext datacontext, Int32 EdificioId, Int32? ElementoExternoId)
        {
            baseFill(datacontext);

            this.EdificioId = EdificioId;
            this.ElementoExternoId = ElementoExternoId;

            if(ElementoExternoId.HasValue)
            {
                var elemento = datacontext.context.MenuPropietarioEdificio.FirstOrDefault(X => X.MenuPropietarioEdificioId == ElementoExternoId.Value);
                if(elemento!= null)
                {
                    Nombre = elemento.Nombre;
                    Icono = elemento.Icono;
                    DocumentoAnterior = elemento.Documento;
                }

            }
        }
    }
}