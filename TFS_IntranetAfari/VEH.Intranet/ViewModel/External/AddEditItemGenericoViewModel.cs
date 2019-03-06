using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.External
{
    public class AddEditItemGenericoViewModel:BaseViewModel
    {
        public String tipo { get; set; }
        public String dato { get; set; }
        public String nombre { get; set; }
        public String vista { get; set; }
        public String TipoLista { get; set; } 
        public Boolean AplicaMantenimiento { get; set; }
        public Int32? UnidadTiempoId { get; set; }
        public List<UnidadTiempo> LstUnidadTiempo { get; set; }
        public Int32? itemId { get; set; }
        public Int32? Orden { get; set; }
        public HttpPostedFileBase file { get; set; }
        public void fill(CargarDatosContext datacontext,Int32? itemId,String vista,Int32 EdificioId,String tipo,Int32? orden)
        {
            baseFill(datacontext);
            LstUnidadTiempo = datacontext.context.UnidadTiempo.Where(X => X.Estado == "ACT").OrderByDescending(X => X.Orden).ToList();
            this.itemId = itemId;
            this.EdificioId = EdificioId;
            this.vista = vista;
            this.Orden = orden;
            this.tipo = tipo;
            if(itemId.HasValue)
            {
                var item = datacontext.context.DatoEdificio.FirstOrDefault(X => X.DatoEdificioId == itemId.Value);
                if (item != null)
                {
                    this.tipo = item.Tipo;
                    this.dato = item.Dato;
                    this.nombre = item.Nombre;
                    this.EdificioId = item.EdificioId;
                    this.UnidadTiempoId = item.UnidadTiempoId;
                    this.AplicaMantenimiento = item.AplicaMantenimiento;
                    this.Orden = item.Orden;
                }
            }
        }
    }
}