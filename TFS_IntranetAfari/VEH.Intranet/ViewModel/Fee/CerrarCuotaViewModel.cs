using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.Controllers;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Fee
{
    public class CerrarCuotaViewModel : BaseViewModel
    {
        public Int32? UnidadTiempoInicio { get; set; }
        public Int32? UnidadTiempoFin { get; set; }
        public Int32? DepartamentoId { get; set; }
        public Int32? EdificioId { get; set; }
        public Int32? EdiId { get; set; }
        public String AcronimoEdificio { get; set; }
        public String NombreEdificio { get; set; }
        public String StrLeyendas { get; set; } = String.Empty;
        public List<SelectListItem> LstComboUnidadTiempo { get; set; } = new List<SelectListItem>();
        public List<Departamento> LstDepartamento { get; set; } = new List<Departamento>();
        public List<Cuota> LstCuota { get; set; } = new List<Cuota>();
        public Decimal FactorMora { get; set; }
        public String TipoMora { get; set; }
        public Int32 DiaMora { get; set; }
        public List<Leyenda> LstLeyenda { get; set; } = new List<Leyenda>();
        public String Estado { get; set; }
        public List<SelectListItem> LstEstado { get; set; } = new List<SelectListItem>();
        public String FechaConsiderar { get; set; }
        public Int32 AnioElegido { get; set; } = DateTime.Now.Year;
        public Int32 MesElegido { get; set; } = DateTime.Now.Month;
        public void Fill(CargarDatosContext c, Int32? unidadTiempoInicio, Int32? unidadTiempoFin, Int32? departamentoId, Int32 edificioId, String estado)
        {
            baseFill(c);
            this.EdiId = edificioId;
            var edificio = c.context.Edificio.FirstOrDefault(x => x.EdificioId == edificioId);
            AcronimoEdificio = edificio.Acronimo;
            NombreEdificio = edificio.Nombre;
            FactorMora = edificio.PMora.Value;
            TipoMora = edificio.TipoMora;
            DiaMora = edificio.DiaMora ?? 15;
            this.UnidadTiempoInicio = unidadTiempoInicio;
            this.UnidadTiempoFin = unidadTiempoFin;
            this.DepartamentoId = departamentoId;
            this.Estado = estado ?? "0";
            List<UnidadTiempo> lstunidadtiempo = c.context.UnidadTiempo.OrderBy(x => -x.Orden).Where(x => x.Estado == ConstantHelpers.EstadoActivo).ToList();
            foreach (var item in lstunidadtiempo)
                LstComboUnidadTiempo.Add(new SelectListItem { Value = item.UnidadTiempoId.ToString(), Text = item.Descripcion.ToUpper() });

            LstEstado.Add(new SelectListItem { Text = "Pagado", Value = "1" });
            LstEstado.Add(new SelectListItem { Text = "Sin Pagar", Value = "0" });

            if (!UnidadTiempoFin.HasValue)
            {
                var uni = lstunidadtiempo.FirstOrDefault(x => x.EsActivo);
                UnidadTiempoFin = uni.UnidadTiempoId;
                AnioElegido = uni.Anio;
                MesElegido = uni.Mes;
            }
            else
            {
                var uni = lstunidadtiempo.FirstOrDefault(x => x.Estado == ConstantHelpers.EstadoActivo && x.UnidadTiempoId == UnidadTiempoFin.Value);
                AnioElegido = uni.Anio;
                MesElegido = uni.Mes;
            }
            var mesActivo = lstunidadtiempo.FirstOrDefault(x => x.EsActivo).Mes;
            if (mesActivo == 2 && (DiaMora == 30 || DiaMora == 31))
            {
                DiaMora = 28;
                FechaConsiderar = "28/" + mesActivo + "/" + lstunidadtiempo.FirstOrDefault(x => x.EsActivo).Anio;
            }
            else
            {
                if (DiaMora == 30)
                {
                    if (mesActivo == 1 ||
                        mesActivo == 3 ||
                        mesActivo == 5 ||
                        mesActivo == 7 ||
                        mesActivo == 8 ||
                        mesActivo == 10 ||
                        mesActivo == 12)
                    {
                        DiaMora += 1;
                        FechaConsiderar = DiaMora + "/" + mesActivo + "/" + lstunidadtiempo.FirstOrDefault(x => x.EsActivo).Anio;
                    }
                }
                else
                {
                    FechaConsiderar = DiaMora + "/" + mesActivo + "/" + lstunidadtiempo.FirstOrDefault(x => x.EsActivo).Anio;
                }
            }
            LstDepartamento = c.context.Departamento.Where(x => x.Estado == ConstantHelpers.EstadoActivo && x.EdificioId == edificioId).ToList();


            if (this.DepartamentoId.HasValue)
            {
                var query = c.context.Cuota.Where(x => x.DepartamentoId == this.DepartamentoId && x.UnidadTiempo.Estado == ConstantHelpers.EstadoActivo && x.UnidadTiempoId <= UnidadTiempoFin).AsQueryable();
                if (!String.IsNullOrEmpty(this.Estado))
                {
                    var e = this.Estado == "0" ? false : true;
                    query = query.Where(x => x.Pagado == e);
                }
                LstCuota = query.OrderBy(x => x.DepartamentoId).ThenByDescending(x => x.UnidadTiempo.Orden).ToList();
            }
            else
            {
                var query = c.context.Cuota.Where(x => x.UnidadTiempoId <= UnidadTiempoFin && x.UnidadTiempo.Estado == ConstantHelpers.EstadoActivo && x.Departamento.EdificioId == this.EdiId).AsQueryable();
                if (!String.IsNullOrEmpty(this.Estado))
                {
                    var e = this.Estado == "0" ? false : true;
                    query = query.Where(x => x.Pagado == e);
                }
                LstCuota = query.OrderBy(x => x.DepartamentoId).ThenByDescending(x => x.UnidadTiempo.Orden).ToList();
            }
            LstLeyenda = c.context.Leyenda.Where(X => X.BalanceUnidadTiempoEdificio.UnidadDeTiempoId == UnidadTiempoFin && X.BalanceUnidadTiempoEdificio.EdificioId == EdiId).ToList();
            foreach (var item in LstLeyenda) { StrLeyendas += item.Numero + "." + item.Descripcion + "\r\n"; }
        }
    }
}