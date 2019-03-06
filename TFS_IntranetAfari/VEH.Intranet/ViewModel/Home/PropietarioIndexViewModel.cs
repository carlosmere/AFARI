using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Models;
using VEH.Intranet.Controllers;
using VEH.Intranet.Helpers;
using VEH.Intranet.ViewModel.Shared;
using System.Data.Entity;
using System.Web.Mvc;

namespace VEH.Intranet.ViewModel.Home
{
    public class PropietarioIndexViewModel : BaseViewModel
    {
        public String DesUnidadTiempo { get; set; }
        public String DesEdificio { get; set; }
        public String DesDepartamento { get; set; }
        public Decimal MontoCuota { get; set; }
        public String DesEstadoCuota { get; set; }
        public Dictionary<Int32, String> LstMeses { get; set; } = new Dictionary<Int32, String>();
        public Dictionary<Int32, String> LstMesesExtraordinaria { get; set; } = new Dictionary<Int32, String>();
        public Int32 ContNombreInquilino { get; set; }
        public List<String> LstCuadro { get; set; } = new List<String>();
        public Dictionary<Int32, Decimal> LstTotalCuadro { get; set; } = new Dictionary<Int32, Decimal>();
        public Dictionary<Int32, Decimal> LstTotalCuadroExtraordinario { get; set; } = new Dictionary<Int32, Decimal>();
        public List<String> LstCuadroExtraordinaria { get; set; } = new List<String>();
        public bool AlertaMora { get; set; } = false;
        public PropietarioIndexViewModel()
        {
        }

        public void CargarDatos(CargarDatosContext dataContext)
        {
            baseFill(dataContext);
            UnidadTiempo _UnidadTiempo = dataContext.context.UnidadTiempo.FirstOrDefault(x => x.EsActivo);
            DesUnidadTiempo = _UnidadTiempo == null ? String.Empty : _UnidadTiempo.Descripcion;

            Int32 EdificioId = dataContext.session.GetEdificioId();
            Edificio _Edificio = dataContext.context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);
            DesEdificio = _Edificio == null ? String.Empty : _Edificio.Nombre;

            Int32 DepartamentoId = dataContext.session.GetDepartamentoId();
            Departamento _Departamento = dataContext.context.Departamento.FirstOrDefault(x => x.DepartamentoId == DepartamentoId);
            this.AlertaMora = _Departamento.AlertaMora;
            DesDepartamento = _Departamento == null ? String.Empty : _Departamento.Numero;

            if (_UnidadTiempo == null)
                MontoCuota = 0;
            else
            { 
                Cuota _Cuota = dataContext.context.Cuota.FirstOrDefault(x => x.UnidadTiempoId == _UnidadTiempo.UnidadTiempoId && x.DepartamentoId == DepartamentoId);
                MontoCuota = _Cuota == null ? Decimal.Zero : _Cuota.Total;
                DesEstadoCuota = _Cuota == null ? String.Empty : _Cuota.Estado;
            }

            /*************************************************************************/
            if (AlertaMora)
            {
                var fechaActual = DateTime.Now;
                var unidadTiempoActivo = dataContext.context.UnidadTiempo.FirstOrDefault(X => X.EsActivo);
                var LstCuotas = dataContext.context.Cuota.Include(x => x.Departamento)
                                .Include(x => x.UnidadTiempo)
                                .Include(x => x.Departamento.Propietario)
                                .Where(x => x.Departamento.EdificioId == EdificioId && x.Pagado == false
                                && x.UnidadTiempoId < unidadTiempoActivo.UnidadTiempoId && x.UnidadTiempo.Estado == ConstantHelpers.EstadoActivo
                                && x.DepartamentoId == DepartamentoId
                                && (x.UnidadTiempo.Mes - fechaActual.Month != 0 || x.UnidadTiempo.Anio - fechaActual.Year != 0)).OrderBy(x => x.UnidadTiempo.Orden).ToList();


                foreach (var item in LstCuotas)
                {
                    if (!LstMeses.ContainsKey(item.UnidadTiempo.Orden.Value))
                    {
                        LstMeses.Add(item.UnidadTiempo.Orden.Value, item.UnidadTiempo.Descripcion);
                        LstTotalCuadro.Add(item.UnidadTiempo.Orden.Value, 0);
                    }
                }

                LstTotalCuadro.Add(-1, 0);

                LstCuotas = LstCuotas.OrderBy(x => x.DepartamentoId).ToList();
                List<Int32> LstDepartamentoId = new List<Int32>();
                LstCuotas = LstCuotas.OrderBy(x => x.DepartamentoId).ToList();
                Decimal TotalGeneral = 0;
                var NombreInquilino = String.Empty;
                ContNombreInquilino = 0;
                String Registro;
                Decimal Total = 0;
                foreach (var item in LstCuotas)
                {
                    Registro = String.Empty;

                    if (LstDepartamentoId.Contains(item.DepartamentoId) == false)
                    {
                        var objTitular = item.Departamento.Propietario.FirstOrDefault(x => x.ParentescoTitular.Contains("Titular"));

                        if (objTitular == null)
                            objTitular = item.Departamento.Propietario.FirstOrDefault();

                        Registro = item.Departamento.Numero;
                        Registro += "#" + (objTitular != null ? objTitular.Nombres : String.Empty);


                        if (objTitular != null)
                        {
                            NombreInquilino = objTitular.Inquilino.FirstOrDefault() == null ? String.Empty : objTitular.Inquilino.FirstOrDefault().Nombres;
                        }
                        else
                        {
                            NombreInquilino = String.Empty;
                        }
                        if (!String.IsNullOrEmpty(NombreInquilino))
                        {
                            ContNombreInquilino++;
                        }

                        Registro += "#" + NombreInquilino;

                        foreach (var mes in LstMeses)
                        {
                            Total = 0;

                            var cuota = LstCuotas.FirstOrDefault(x => x.Departamento.Numero == item.Departamento.Numero && x.UnidadTiempo.Orden == mes.Key);
                            Registro += "#" + (cuota != null ? String.Format("{0:#,##0.00}", cuota.Total - cuota.CuotaExtraordinaria) : "0");

                            Total += cuota != null ? ((cuota.Total - cuota.CuotaExtraordinaria).Value) : 0;

                            if (LstTotalCuadro.ContainsKey(mes.Key))
                            {
                                LstTotalCuadro[mes.Key] += Total;
                            }
                            TotalGeneral += Total;
                        }

                        Registro += "#" + String.Format("{0:#,##0.00}", TotalGeneral);
                        LstTotalCuadro[-1] += TotalGeneral;
                        TotalGeneral = 0;

                        LstDepartamentoId.Add(item.DepartamentoId);
                        LstCuadro.Add(Registro);
                    }
                }
                LstCuotas = LstCuotas.Where(x => x.CuotaExtraordinaria > 0).ToList();

                foreach (var item in LstCuotas)
                {
                    if (!LstMesesExtraordinaria.ContainsKey(item.UnidadTiempo.Orden.Value))
                    {
                        LstMesesExtraordinaria.Add(item.UnidadTiempo.Orden.Value, item.UnidadTiempo.Descripcion);
                        LstTotalCuadroExtraordinario.Add(item.UnidadTiempo.Orden.Value, 0);
                    }
                }

                LstTotalCuadroExtraordinario.Add(-1, 0);

                LstDepartamentoId = new List<Int32>();
                LstCuotas = LstCuotas.OrderBy(x => x.DepartamentoId).ToList();
                TotalGeneral = 0;
                foreach (var item in LstCuotas)
                {
                    Registro = String.Empty;
                    if (LstDepartamentoId.Contains(item.DepartamentoId) == false)
                    {
                        var objTitular = item.Departamento.Propietario.FirstOrDefault(x => x.ParentescoTitular.Contains("Titular"));

                        if (objTitular == null)
                            objTitular = item.Departamento.Propietario.FirstOrDefault();

                        Registro = item.Departamento.Numero;
                        Registro += "#" + (objTitular != null ? objTitular.Nombres : String.Empty);


                        if (objTitular != null)
                        {
                            NombreInquilino = objTitular.Inquilino.FirstOrDefault() == null ? String.Empty : objTitular.Inquilino.FirstOrDefault().Nombres;
                        }
                        else
                        {
                            NombreInquilino = String.Empty;
                        }
                        if (!String.IsNullOrEmpty(NombreInquilino))
                        {
                            ContNombreInquilino++;
                        }

                        Registro += "#" + NombreInquilino;

                        foreach (var mes in LstMeses)
                        {
                            Total = 0;

                            var cuota = LstCuotas.FirstOrDefault(x => x.Departamento.Numero == item.Departamento.Numero && x.UnidadTiempo.Orden == mes.Key);
                            Registro += "#" + (cuota != null ? String.Format("{0:#,##0.00}", cuota.CuotaExtraordinaria) : "0");

                            Total += cuota != null ? (cuota.CuotaExtraordinaria.Value) : 0;

                            if (LstTotalCuadroExtraordinario.ContainsKey(mes.Key))
                            {
                                LstTotalCuadroExtraordinario[mes.Key] += Total;
                            }
                            TotalGeneral += Total;
                        }
                        Registro += "#" + String.Format("{0:#,##0.00}", Total);
                        LstTotalCuadroExtraordinario[-1] += TotalGeneral;
                        TotalGeneral = 0;

                        LstDepartamentoId.Add(item.DepartamentoId);
                        LstCuadroExtraordinaria.Add(Registro);
                    }

                }
            }
            /*****************************************************************************/
        }
    }
}