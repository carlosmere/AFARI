using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Fee
{
    public class _AddEditMConsumoIndividual : BaseViewModel
    {
        public Int32 DepartamentoId { get; set; }
        public Int32? CuotaId { get; set; }
        public Int32 ConsumoIndividualId { get; set; }
        public Int32 UnidadTiempoId { get; set; }
        public String Detalle { get; set; }
        public Decimal Monto { get; set; }
        public String NombreEdificio { get; set; }
        public String NombreUnidadTiempo { get; set; }
        public String NombreDepartamento { get; set; }
        public List<Departamento> LstDepartamento { get; set; } = new List<Departamento>();
        public List<String> lstdetalle { get; set; } = new List<string>() { "", "", "", "", "", "", "", "", "", "" };
        public List<Decimal> lstmonto { get; set; } = new List<decimal>() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public void Fill(CargarDatosContext c, Int32? cuotaId, Int32 edificioId, Int32 unidadTiempoId)
        {
            baseFill(c);
            this.EdificioId = edificioId;
            this.CuotaId = cuotaId;
            this.UnidadTiempoId = unidadTiempoId;

            LstDepartamento = c.context.Departamento.Where(x => x.EdificioId == this.EdificioId).ToList();

            NombreEdificio = c.context.Edificio.FirstOrDefault(x => x.EdificioId == this.EdificioId).Nombre;
            NombreUnidadTiempo = c.context.UnidadTiempo.FirstOrDefault(x => x.UnidadTiempoId == this.UnidadTiempoId).Descripcion;
        }
    }
}