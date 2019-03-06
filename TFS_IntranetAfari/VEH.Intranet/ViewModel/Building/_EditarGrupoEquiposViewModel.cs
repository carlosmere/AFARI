using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Building
{
    public class _EditarGrupoEquiposViewModel : BaseViewModel
    {
        public String Nombre { get; set; }
        public Int32 EdificioId { get; set; }
        public String FiltroTipo { get; set; }
        public String vista { get; set; }
        public String LstDatosId { get; set; }
        public void Fill(CargarDatosContext c,Int32 EdificioId,String filtroTipo,String vista,String lstDatosId, String nombre)
        {
            baseFill(c);
            this.EdificioId = EdificioId;
            this.FiltroTipo = filtroTipo;
            this.vista = vista;
            this.LstDatosId = lstDatosId;
            this.Nombre = nombre;
        }
    }
}