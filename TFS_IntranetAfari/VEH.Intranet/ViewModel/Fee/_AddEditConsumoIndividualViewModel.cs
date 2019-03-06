using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Fee
{
    public class _AddEditConsumoIndividualViewModel : BaseViewModel
    {
        public Int32? CuotaId { get; set; }
        public Int32 ConsumoIndividualId { get; set; }
        public Int32 UnidadTiempoId { get; set; }
        public String Detalle { get; set; }
        public Decimal Monto { get; set; }
        public String NombreEdificio { get; set; }
        public String NombreUnidadTiempo { get; set; }
        public String NombreDepartamento { get; set; }
        public void Fill(CargarDatosContext c, Int32? cuotaId, Int32 consumoIndividualId, Int32 edificioId, Int32 unidadTiempoId)
        {
            baseFill(c);
            this.EdificioId = edificioId;
            this.ConsumoIndividualId = consumoIndividualId;
            this.CuotaId = cuotaId;
            this.UnidadTiempoId = unidadTiempoId;

            var consumo = c.context.ConsumoIndividual.FirstOrDefault( x => x.ConsumoIndividualId == this.ConsumoIndividualId);
            Detalle = consumo.Detalle;
            Monto = consumo.Monto;


            NombreEdificio = c.context.Edificio.FirstOrDefault(x => x.EdificioId == this.EdificioId).Nombre;
            NombreUnidadTiempo = c.context.UnidadTiempo.FirstOrDefault(x => x.UnidadTiempoId == this.UnidadTiempoId).Descripcion;
            NombreDepartamento = consumo.Cuota.Departamento.Numero;
        }
    }
}