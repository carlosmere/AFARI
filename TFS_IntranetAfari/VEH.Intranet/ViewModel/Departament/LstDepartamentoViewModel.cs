using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Departament
{
    public class LstDepartamentoViewModel : BaseViewModel
    {
        public Int32 Identificador { get; set; }
        public Int32 CantidadReporte { get; set; }
        public Int32? DesfaseRecibos { get; set; }
        public String NombreEdificio { get; set; }
        public String AcronimoEdificio { get; set; }
        public Int32 EdificioId { get; set; }
        public List<Departamento> LstDepartamentos { get; set; }
        public Dictionary<Int32, String> DicNumeroRecibo { get; set; } = new Dictionary<int, String>();
        public LstDepartamentoViewModel() { }

        public void Fill(CargarDatosContext datacontext)
        {
            baseFill(datacontext);
            var Edificio = datacontext.context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);
            NombreEdificio = Edificio.Nombre;
            DesfaseRecibos = Edificio.DesfaseRecibos;
            CantidadReporte = Edificio.CantidadReporte;
            this.Identificador = Edificio.Identificador;
            AcronimoEdificio = Edificio.Acronimo;
            LstDepartamentos = datacontext.context.Departamento.Where(x => x.EdificioId == EdificioId).ToList();
            long utRecibo = 0;
            foreach (var item in LstDepartamentos)
            {
                try
                {
                    utRecibo = item.UnidadTiempoReciboDepartamento.Where(x => item.DepartamentoId == x.DepartamentoId).Max( x => x.NumeroRecibo);
                }
                catch
                {
                    utRecibo = 0;
                }
                
                DicNumeroRecibo.Add(item.DepartamentoId, utRecibo.ToString().PadLeft(6,'0'));
            }
         
        }
    }
}