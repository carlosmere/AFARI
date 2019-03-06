using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Models;

using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using VEH.Intranet.Helpers;
using VEH.Intranet.ViewModel.Shared;
namespace VEH.Intranet.ViewModel.Building
{
    public class AddEditEstadoCuentaBancarioViewModel : BaseViewModel
    {
        public Int32? EstadoCuentaBancarioId { get; set; }
        public Int32 UnidadTiempoId { get; set; }
        public String Ruta { get; set; }
        public String Nombre { get; set; }
        public HttpPostedFileBase Archivo { get; set; }
        public List<UnidadTiempo> LstUnidadTiempo { get; set; } = new List<UnidadTiempo>();
        public Int32 EdiId { get; set; }
        public void Fill(CargarDatosContext c, Int32? estadoCuentaBancarioId,Int32 edificioId)
        {
            baseFill(c);
            this.EdiId = edificioId;
            this.EstadoCuentaBancarioId = estadoCuentaBancarioId;
            LstUnidadTiempo = c.context.UnidadTiempo.Where(x => x.Estado == ConstantHelpers.EstadoActivo).OrderByDescending( x => x.UnidadTiempoId).ToList();
            if (this.EstadoCuentaBancarioId.HasValue)
            {
                var estadoCuenta = c.context.EstadoCuentaBancario.FirstOrDefault(x => x.EstadoCuentaBancarioId == this.EstadoCuentaBancarioId);
                this.UnidadTiempoId = estadoCuenta.UnidadTiempoId;
                this.Ruta = estadoCuenta.Ruta;
                this.Nombre = estadoCuenta.Nombre;
            }
        }
    }
}