using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.CorretajeInmobiliario
{
    public class AddEditDatosClientesViewModel : BaseViewModel
    {
        public Int32? ClienteCorretajeId { get; set; }
        public string TipoServicio { get; set; }
        public string TipoInmueble { get; set; }
        public string Direccion { get; set; }
        public string Distrito { get; set; }
        public string Area { get; set; }
        public string Dormitorios { get; set; }
        public string Estacionamientos { get; set; }
        public string Deposito { get; set; }
        public string Antiguedad { get; set; }
        public string CantidadPiso { get; set; }
        public string Precio { get; set; }
        public string CostoMantenimiento { get; set; }
        public string Cliente { get; set; }
        public string Numero { get; set; }
        public string Correo { get; set; }
        public string FlagVer { get; set; }
        public string Otros { get; set; }
        public string CantidadInmuebles { get; set; }
        public void Fill(CargarDatosContext c, Int32? clienteCorretajeId, String flagVer)
        {
            this.FlagVer = flagVer;
            this.ClienteCorretajeId = clienteCorretajeId;
            if (this.ClienteCorretajeId.HasValue)
            {
                var cliente = c.context.ClienteCorretaje.FirstOrDefault(x => x.ClienteCorretajeId == this.ClienteCorretajeId);
                this.TipoServicio = cliente.TipoServicio;
                this.TipoInmueble = cliente.TipoInmueble;
                this.Direccion = cliente.Direccion;
                this.Distrito = cliente.Distrito;
                this.Area = cliente.Area;
                this.Dormitorios = cliente.Dormitorios;
                this.Estacionamientos = cliente.Estacionamientos;
                this.Deposito = cliente.Deposito;
                this.Antiguedad = cliente.Antiguedad;
                this.CantidadPiso = cliente.CantidadPiso;
                this.Precio = cliente.Precio;
                this.CostoMantenimiento = cliente.CostoMantenimiento;
                this.Cliente = cliente.Cliente;
                this.Numero = cliente.Numero;
                this.Correo = cliente.Correo;
                this.Otros = cliente.Otros;
                this.CantidadInmuebles = CantidadInmuebles;
            }
        }
    }
}