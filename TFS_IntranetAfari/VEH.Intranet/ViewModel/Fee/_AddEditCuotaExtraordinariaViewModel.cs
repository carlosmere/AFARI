﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Fee
{
    public class _AddEditCuotaExtraordinariaViewModel : BaseViewModel
    {
        public Int32 EdificioId { get; set; }
        public Int32 UnidadTiempoId { get; set; }
        public Decimal MontoExtraordinaria { get; set; }
        public String NombreEdificio { get; set; }
        public String NombreUnidadTiempo { get; set; }
        public void Fill(CargarDatosContext c, Int32 edificioId, Int32 unidadTiempoId)
        {
            baseFill(c);

            this.EdificioId = edificioId;
            this.UnidadTiempoId = unidadTiempoId;
            NombreEdificio = c.context.Edificio.FirstOrDefault( x => x.EdificioId == this.EdificioId).Nombre;
            NombreUnidadTiempo = c.context.UnidadTiempo.FirstOrDefault(x => x.UnidadTiempoId == this.UnidadTiempoId).Descripcion;
        }
    }
}