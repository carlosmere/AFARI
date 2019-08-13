using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.CorretajeInmobiliario
{
    public class AddEditVisitaViewModel : BaseViewModel
    {
        public Int32? VisitaCorretajeId { get; set; }
        public string Cliente { get; set; } = String.Empty;
        public string Direccion { get; set; } = String.Empty;
        public string Tipo { get; set; } = String.Empty;
        public decimal Precio { get; set; } = 0;
        public string Moneda { get; set; } = String.Empty;
        public string NombreCliente { get; set; } = String.Empty;
        public String Fecha { get; set; } = DateTime.Now.ToString("dd/MM/yyyy");
        public String Hora { get; set; } = DateTime.Now.ToString("HH:mm");
        public String Firma { get; set; } = String.Empty;
        public void Fill(CargarDatosContext c, Int32? visitaCorretajeId)
        {
            this.VisitaCorretajeId = visitaCorretajeId;
            if (this.VisitaCorretajeId.HasValue)
            {
                var visita = c.context.VisitaCorretaje.FirstOrDefault(x => x.VisitaCorretajeId == VisitaCorretajeId);
                this.Cliente = visita.Cliente;
                this.Direccion = visita.Direccion;
                this.Tipo = visita.Tipo;
                this.Precio = visita.Precio;
                this.Moneda = visita.Moneda;
                this.NombreCliente = visita.NombreCliente;
                this.Fecha = visita.Fecha.ToString("dd/MM/yyyy");
                this.Hora = visita.Hora.ToString();
                this.Firma = visita.Firma;
            }
        }
    }
}