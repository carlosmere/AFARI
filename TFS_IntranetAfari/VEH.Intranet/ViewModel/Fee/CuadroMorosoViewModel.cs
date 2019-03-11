using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;
using System.Data.Entity;

namespace VEH.Intranet.ViewModel.Fee
{
    public class CuadroMorosoViewModel : BaseViewModel
    {
        public Int32 Id { get; set; }
        public Dictionary<Int32, String> LstMeses { get; set; } = new Dictionary<Int32, String>();
        public Dictionary<Int32, String> LstMesesExtraordinaria { get; set; } = new Dictionary<Int32, String>();
        public Int32 ContNombreInquilino { get; set; }
        public List<String> LstCuadro { get; set; } = new List<String>();
        public Dictionary<Int32, Decimal> LstTotalCuadro { get; set; } = new Dictionary<Int32, Decimal>();
        public Dictionary<Int32, Decimal> LstTotalCuadroExtraordinario { get; set; } = new Dictionary<Int32, Decimal>();
        public List<String> LstCuadroExtraordinaria { get; set; } = new List<String>();
        public String TipoEdificio { get; set; }
        public String NombreEdificio { get; set; }
        public void Fill(CargarDatosContext datacontext, Int32 EdificioId)
        {
            try
            {
                baseFill(datacontext);
                NombreEdificio = datacontext.context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId).Nombre;
                TipoEdificio = datacontext.context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId).TipoInmueble.Nombre;
                Id = EdificioId;
                var unidadTiempoActivo = datacontext.context.UnidadTiempo.FirstOrDefault(X => X.EsActivo);
                var LstCuotasT = datacontext.context.Cuota.Include(x => x.Departamento)
                                .Include(x => x.UnidadTiempo)
                                .Include(x => x.Departamento.Propietario)
                                .Where(x => x.Departamento.EdificioId == EdificioId && x.Pagado == false && x.UnidadTiempoId < unidadTiempoActivo.UnidadTiempoId && x.UnidadTiempo.Estado == ConstantHelpers.EstadoActivo).OrderBy(x => x.UnidadTiempo.Orden).ThenBy(x => x.CuotaId).ToList();

                List<Cuota> LstCuotas = new List<Cuota>();

                foreach (var cuota in LstCuotasT)
                {
                    if (cuota.EsExtraordinaria.HasValue && cuota.EsExtraordinaria.Value)
                    {
                        var validacionExtra = LstCuotas.FirstOrDefault(x => x.DepartamentoId == cuota.DepartamentoId);
                        if (validacionExtra != null)
                        {
                            

                            if (cuota.UnidadTiempo.Mes == validacionExtra.UnidadTiempo.Mes)
                            {
                                LstCuotas.Remove(validacionExtra);
                                validacionExtra.CuotaExtraordinaria += cuota.CuotaExtraordinaria;
                                validacionExtra.Total += cuota.CuotaExtraordinaria ?? 0;
                                LstCuotas.Add(validacionExtra);
                            }
                            else if (cuota.UnidadTiempo.Mes != validacionExtra.UnidadTiempo.Mes)
                            {
                                //validacionExtra.Total += cuota.CuotaExtraordinaria ?? 0;
                                LstCuotas.Add(cuota);
                            }
                            else if (cuota.FechaEmision.Value.Month == validacionExtra.FechaEmision.Value.Month)
                            {
                                LstCuotas.Remove(validacionExtra);
                                validacionExtra.CuotaExtraordinaria += cuota.CuotaExtraordinaria;
                                validacionExtra.Total += validacionExtra.CuotaExtraordinaria ?? 0;
                                LstCuotas.Add(validacionExtra);
                            }

                            
                        }
                        else
                        {
                            LstCuotas.Add(cuota);
                        }

                    }
                    else
                    {
                        LstCuotas.Add(cuota);
                    }
                }




                foreach (var item in LstCuotas)
                {
                    if (!LstMeses.ContainsKey(item.UnidadTiempo.Orden.Value) && item.UnidadTiempo.Estado == ConstantHelpers.EstadoActivo)
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
                        var objTitular = item.Departamento.Propietario.FirstOrDefault(x => x.ParentescoTitular.Contains("Titular") && x.Estado == ConstantHelpers.EstadoActivo);


                        if (objTitular == null)
                            objTitular = item.Departamento.Propietario.FirstOrDefault(x => x.Estado == ConstantHelpers.EstadoActivo);

                        Registro = item.Departamento.Numero;
                        Registro += "#" + (objTitular != null ? objTitular.Nombres : String.Empty);


                        if (objTitular != null)
                        {
                            NombreInquilino = objTitular.Inquilino.FirstOrDefault(x => x.Estado == ConstantHelpers.EstadoActivo) == null ? String.Empty : objTitular.Inquilino.FirstOrDefault(x => x.Estado == ConstantHelpers.EstadoActivo).Nombres;
                        }
                        else
                        {
                            continue;
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
                            //Registro += "#" + (cuota != null ? String.Format("{0:#,##0.00}", cuota.Total - cuota.CuotaExtraordinaria) : "0");

                            if (cuota != null)
                            {
                                if (objTitular.FechaCreacion.HasValue)
                                {
                                    var fechaComparar = new DateTime();

                                    if (cuota.FechaVencimiento.HasValue)
                                    {
                                        try
                                        {
                                            fechaComparar = new DateTime(cuota.FechaVencimiento.Value.Year, cuota.UnidadTiempo.Mes, cuota.FechaVencimiento.Value.Day);
                                        }
                                        catch (Exception ex)
                                        {
                                            try
                                            {
                                                fechaComparar = new DateTime(cuota.FechaVencimiento.Value.Year, cuota.UnidadTiempo.Mes, cuota.FechaVencimiento.Value.Day - 1);
                                            }
                                            catch (Exception ex2)
                                            {
                                                fechaComparar = new DateTime(cuota.FechaVencimiento.Value.Year, cuota.UnidadTiempo.Mes, cuota.FechaVencimiento.Value.Day - 3);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (cuota.UnidadTiempo.Mes + 1 >= 13)
                                        {
                                            fechaComparar = new DateTime(cuota.UnidadTiempo.Anio + 1, 1, cuota.Departamento.Edificio.DiaMora ?? 30);
                                        }
                                        else
                                            fechaComparar = new DateTime(cuota.UnidadTiempo.Anio, cuota.UnidadTiempo.Mes + 1, cuota.Departamento.Edificio.DiaMora ?? 30);
                                    }


                                    if (fechaComparar >= objTitular.FechaCreacion.Value.Date)
                                    //if (cuota.FechaEmision.HasValue && cuota.FechaEmision.Value.Month == objTitular.FechaCreacion.Value.Month)
                                    {
                                        if (cuota.Total != cuota.CuotaExtraordinaria)
                                        {
                                            Registro += "#" + (cuota != null ? String.Format("{0:#,##0.00}", cuota.Total - cuota.CuotaExtraordinaria) : "0");
                                        }
                                        else
                                        {
                                            Registro += "#" + String.Empty;
                                        }
                                        
                                        Total += cuota != null ? ((cuota.Total - cuota.CuotaExtraordinaria).Value) : 0;
                                    }
                                    else
                                    {
                                        Registro += "#0";
                                    }
                                }
                                else
                                {
                                    if (cuota.Total != cuota.CuotaExtraordinaria)
                                    {
                                        Registro += "#" + (cuota != null ? String.Format("{0:#,##0.00}", cuota.Total - cuota.CuotaExtraordinaria) : "0");
                                    }
                                    else
                                    {
                                        Registro += "#" + String.Empty;
                                    }
                                    Total += cuota != null ? ((cuota.Total - cuota.CuotaExtraordinaria).Value) : 0;
                                }
                            }
                            else
                            {
                                if (cuota == null || cuota.Total != cuota.CuotaExtraordinaria)
                                {
                                    Registro += "#" + (cuota != null ? String.Format("{0:#,##0.00}", cuota.Total - cuota.CuotaExtraordinaria) : "0");
                                }
                                else
                                {
                                    Registro += "#" + String.Empty;
                                }

                                Total += cuota != null ? ((cuota.Total - cuota.CuotaExtraordinaria).Value) : 0;
                            }

                            if (LstTotalCuadro.ContainsKey(mes.Key))
                            {
                                LstTotalCuadro[mes.Key] += Total;
                            }
                            TotalGeneral += Total;
                        }

                        Registro += "#" + String.Format("{0:#,##0.00}", TotalGeneral);
                        LstTotalCuadro[-1] += TotalGeneral;

                        if (TotalGeneral > 0)
                        {
                            LstCuadro.Add(Registro);
                        }

                        TotalGeneral = 0;

                        LstDepartamentoId.Add(item.DepartamentoId);


                        var lstHistoria = datacontext.context.DepartamentoHistorico.Where(x => x.DepartamentoId == item.DepartamentoId && x.Fecha < objTitular.FechaCreacion).ToList();
                        if (lstHistoria.Count > 0)
                        {
                            var objTitular2 = item.Departamento.Propietario.FirstOrDefault(x => x.ParentescoTitular.Contains("Titular") && x.Estado == ConstantHelpers.EstadoActivo);

                            if (objTitular2 == null)
                                objTitular2 = item.Departamento.Propietario.FirstOrDefault(x => x.Estado == ConstantHelpers.EstadoActivo);

                            foreach (var historia in lstHistoria)
                            {
                                objTitular = historia.Propietario;


                                Registro = item.Departamento.Numero;
                                Registro += "#" + (objTitular != null ? objTitular.Nombres : String.Empty);


                                if (objTitular != null)
                                {
                                    NombreInquilino = objTitular.Inquilino.FirstOrDefault(x => x.Estado == ConstantHelpers.EstadoActivo) == null ? String.Empty : objTitular.Inquilino.FirstOrDefault(x => x.Estado == ConstantHelpers.EstadoActivo).Nombres;
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

                                    var cuota = LstCuotas.FirstOrDefault(x => x.Departamento.Numero == item.Departamento.Numero
                                    && x.UnidadTiempo.Orden == mes.Key);

                                    if (cuota != null)
                                    {
                                        var fechaComparar = new DateTime();

                                        if (cuota.FechaVencimiento.HasValue)
                                        {
                                            try
                                            {
                                                fechaComparar = new DateTime(cuota.FechaVencimiento.Value.Year, cuota.UnidadTiempo.Mes, cuota.FechaVencimiento.Value.Day);
                                            }
                                            catch (Exception ex)
                                            {
                                                try
                                                {
                                                    fechaComparar = new DateTime(cuota.FechaVencimiento.Value.Year, cuota.UnidadTiempo.Mes, cuota.FechaVencimiento.Value.Day - 1);
                                                }
                                                catch (Exception ex2)
                                                {
                                                    fechaComparar = new DateTime(cuota.FechaVencimiento.Value.Year, cuota.UnidadTiempo.Mes, cuota.FechaVencimiento.Value.Day - 3);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (cuota.UnidadTiempo.Mes + 1 >= 13)
                                            {
                                                fechaComparar = new DateTime(cuota.UnidadTiempo.Anio + 1, 1, cuota.Departamento.Edificio.DiaMora ?? 30);
                                            }
                                            else
                                                fechaComparar = new DateTime(cuota.UnidadTiempo.Anio, cuota.UnidadTiempo.Mes + 1, cuota.Departamento.Edificio.DiaMora ?? 30);
                                        }


                                        //if (objTitular2 != null && fechaComparar >= historia.Propietario.FechaCreacion.Value.Date &&
                                        //    (fechaComparar < objTitular2.FechaCreacion.Value.Date
                                        //    || (fechaComparar.Month == objTitular2.FechaCreacion.Value.Month
                                        //    && fechaComparar.Year == objTitular2.FechaCreacion.Value.Year)
                                        //    ))
                                        if (historia.Propietario != null && fechaComparar >= historia.Propietario.FechaCreacion.Value.Date &&
                                            (fechaComparar < objTitular2.FechaCreacion.Value.Date
                                            || (fechaComparar.Month == historia.Propietario.FechaCreacion.Value.Month
                                            && fechaComparar.Year == historia.Propietario.FechaCreacion.Value.Year)
                                            ))
                                        //if (cuota.FechaEmision <= historia.Fecha 
                                        //    && cuota.FechaEmision.Value.Month == historia.Propietario.FechaCreacion.Value.Month)
                                        {
                                            Registro += "#" + (cuota != null ? String.Format("{0:#,##0.00}", cuota.Total - cuota.CuotaExtraordinaria) : "0");
                                            Total += cuota != null ? ((cuota.Total - cuota.CuotaExtraordinaria).Value) : 0;
                                        }
                                        else
                                        {
                                            Registro += "#0";
                                        }
                                    }
                                    else
                                    {
                                        Registro += "#0";
                                    }

                                    if (LstTotalCuadro.ContainsKey(mes.Key))
                                    {
                                        LstTotalCuadro[mes.Key] += Total;
                                    }
                                    TotalGeneral += Total;
                                }

                                Registro += "#" + String.Format("{0:#,##0.00}", TotalGeneral);
                                LstTotalCuadro[-1] += TotalGeneral;
                                TotalGeneral = 0;

                                LstCuadro.Add(Registro);
                            }
                        }
                    }
                }

                LstCuotas = LstCuotas.Where(x => x.CuotaExtraordinaria > 0).OrderBy(x => x.UnidadTiempoId).ToList();

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
                        var objTitular = item.Departamento.Propietario.FirstOrDefault(x => x.ParentescoTitular.Contains("Titular") && x.Estado == ConstantHelpers.EstadoActivo);

                        if (objTitular == null)
                            objTitular = item.Departamento.Propietario.FirstOrDefault(x => x.Estado == ConstantHelpers.EstadoActivo);

                        Registro = item.Departamento.Numero;
                        Registro += "#" + (objTitular != null ? objTitular.Nombres : String.Empty);


                        if (objTitular != null)
                        {
                            NombreInquilino = String.Empty; //objTitular.Inquilino.FirstOrDefault(x => x.Estado == ConstantHelpers.EstadoActivo) == null ? String.Empty : objTitular.Inquilino.FirstOrDefault(x => x.Estado == ConstantHelpers.EstadoActivo).Nombres;
                        }
                        else
                        {
                            NombreInquilino = String.Empty;
                        }
                        if (!String.IsNullOrEmpty(NombreInquilino))
                        {
                            ContNombreInquilino++;
                        }

                        //Registro += "#" + NombreInquilino;

                        foreach (var mes in LstMesesExtraordinaria)
                        {
                            Total = 0;

                            var cuota = LstCuotas.FirstOrDefault(x => x.Departamento.Numero == item.Departamento.Numero && x.UnidadTiempo.Orden == mes.Key);
                            //Registro += "#" + (cuota != null ? String.Format("{0:#,##0.00}", cuota.CuotaExtraordinaria) : "0");

                            if (cuota != null)
                            {
                                if (objTitular.FechaCreacion.HasValue)
                                {
                                    var fechaComparar = new DateTime();
                                    if (cuota.FechaVencimiento.HasValue)
                                    {
                                        try
                                        {
                                            fechaComparar = new DateTime(cuota.FechaVencimiento.Value.Year, cuota.UnidadTiempo.Mes, cuota.FechaVencimiento.Value.Day);
                                        }
                                        catch (Exception ex)
                                        {
                                            try
                                            {
                                                fechaComparar = new DateTime(cuota.FechaVencimiento.Value.Year, cuota.UnidadTiempo.Mes, cuota.FechaVencimiento.Value.Day - 1);
                                            }
                                            catch (Exception ex2)
                                            {
                                                fechaComparar = new DateTime(cuota.FechaVencimiento.Value.Year, cuota.UnidadTiempo.Mes, cuota.FechaVencimiento.Value.Day - 3);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (cuota.UnidadTiempo.Mes + 1 >= 13)
                                        {
                                            fechaComparar = new DateTime(cuota.UnidadTiempo.Anio + 1, 1, cuota.Departamento.Edificio.DiaMora ?? 30);
                                        }
                                        else
                                            fechaComparar = new DateTime(cuota.UnidadTiempo.Anio, cuota.UnidadTiempo.Mes + 1, cuota.Departamento.Edificio.DiaMora ?? 30);
                                    }


                                    if (fechaComparar >= objTitular.FechaCreacion.Value.Date)
                                    {
                                        Registro += "#" + (cuota != null ? String.Format("{0:#,##0.00}", cuota.CuotaExtraordinaria) : "0");
                                        Total += cuota != null ? ((cuota.CuotaExtraordinaria).Value) : 0;
                                    }
                                    else
                                    {
                                        Registro += "#0";
                                    }
                                }
                                else
                                {
                                    Registro += "#" + (cuota != null ? String.Format("{0:#,##0.00}", cuota.CuotaExtraordinaria) : "0");
                                    Total += cuota != null ? ((cuota.CuotaExtraordinaria).Value) : 0;
                                }
                            }
                            else
                            {
                                Registro += "#" + (cuota != null ? String.Format("{0:#,##0.00}", cuota.CuotaExtraordinaria) : "0");
                                Total += cuota != null ? ((cuota.CuotaExtraordinaria).Value) : 0;
                            }


                            //                        Total += cuota != null ? (cuota.CuotaExtraordinaria.Value) : 0;

                            if (LstTotalCuadroExtraordinario.ContainsKey(mes.Key))
                            {
                                LstTotalCuadroExtraordinario[mes.Key] += Total;
                            }
                            TotalGeneral += Total;
                        }

                        if (TotalGeneral > 0)
                        {
                            Registro += "#" + String.Format("{0:#,##0.00}", TotalGeneral);
                            LstTotalCuadroExtraordinario[-1] += TotalGeneral;
                            LstCuadroExtraordinaria.Add(Registro);
                        }

                        TotalGeneral = 0;

                        LstDepartamentoId.Add(item.DepartamentoId);

                        var lstHistoria = datacontext.context.DepartamentoHistorico.Where(x => x.DepartamentoId == item.DepartamentoId && x.Fecha < objTitular.FechaCreacion).ToList();
                        if (lstHistoria.Count > 0)
                        {
                            var objTitular2 = item.Departamento.Propietario.FirstOrDefault(x => x.ParentescoTitular.Contains("Titular") && x.Estado == ConstantHelpers.EstadoActivo);

                            if (objTitular2 == null)
                                objTitular2 = item.Departamento.Propietario.FirstOrDefault(x => x.Estado == ConstantHelpers.EstadoActivo);

                            foreach (var historia in lstHistoria)
                            {
                                objTitular = historia.Propietario;

                                Registro = item.Departamento.Numero;
                                Registro += "#" + (objTitular != null ? objTitular.Nombres : String.Empty);


                                if (objTitular != null)
                                {
                                    NombreInquilino = objTitular.Inquilino.FirstOrDefault(x => x.Estado == ConstantHelpers.EstadoActivo) == null ? String.Empty : objTitular.Inquilino.FirstOrDefault(x => x.Estado == ConstantHelpers.EstadoActivo).Nombres;
                                }
                                else
                                {
                                    NombreInquilino = String.Empty;
                                }
                                if (!String.IsNullOrEmpty(NombreInquilino))
                                {
                                    ContNombreInquilino++;
                                }

                                //Registro += "#" + NombreInquilino;

                                foreach (var mes in LstMesesExtraordinaria)
                                {
                                    Total = 0;

                                    var cuota = LstCuotas.FirstOrDefault(x => x.Departamento.Numero == item.Departamento.Numero && x.UnidadTiempo.Orden == mes.Key);

                                    if (cuota != null)
                                    {
                                        var fechaComparar = new DateTime();
                                        if (cuota.FechaVencimiento.HasValue)
                                        {
                                            try
                                            {
                                                fechaComparar = new DateTime(cuota.FechaVencimiento.Value.Year, cuota.UnidadTiempo.Mes, cuota.FechaVencimiento.Value.Day);
                                            }
                                            catch (Exception ex)
                                            {
                                                try
                                                {
                                                    fechaComparar = new DateTime(cuota.FechaVencimiento.Value.Year, cuota.UnidadTiempo.Mes, cuota.FechaVencimiento.Value.Day - 1);
                                                }
                                                catch (Exception ex2)
                                                {
                                                    fechaComparar = new DateTime(cuota.FechaVencimiento.Value.Year, cuota.UnidadTiempo.Mes, cuota.FechaVencimiento.Value.Day - 3);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (cuota.UnidadTiempo.Mes + 1 >= 13)
                                            {
                                                fechaComparar = new DateTime(cuota.UnidadTiempo.Anio + 1, 1, cuota.Departamento.Edificio.DiaMora ?? 30);
                                            }
                                            else
                                                fechaComparar = new DateTime(cuota.UnidadTiempo.Anio, cuota.UnidadTiempo.Mes + 1, cuota.Departamento.Edificio.DiaMora ?? 30);
                                        }


                                        //if (objTitular2 != null && fechaComparar >= historia.Propietario.FechaCreacion.Value.Date &&
                                        //    (fechaComparar < objTitular2.FechaCreacion.Value.Date
                                        //    || (fechaComparar.Month == objTitular2.FechaCreacion.Value.Month
                                        //    && fechaComparar.Year == objTitular2.FechaCreacion.Value.Year)
                                        //    ))
                                        if (historia.Propietario != null && fechaComparar >= historia.Propietario.FechaCreacion.Value.Date &&
                                            (fechaComparar < objTitular2.FechaCreacion.Value.Date
                                            || (fechaComparar.Month == historia.Propietario.FechaCreacion.Value.Month
                                            && fechaComparar.Year == historia.Propietario.FechaCreacion.Value.Year)
                                            ))
                                        //if (cuota.FechaEmision >= historia.Propietario.FechaCreacion.Value.Date 
                                        //    && cuota.FechaEmision.Value.Month == historia.Fecha.Month)
                                        {
                                            Registro += "#" + (cuota != null ? String.Format("{0:#,##0.00}", cuota.CuotaExtraordinaria) : "0");
                                            Total += cuota != null ? (cuota.CuotaExtraordinaria.Value) : 0;
                                        }
                                        else
                                        {
                                            Registro += "#0";
                                        }
                                    }
                                    else
                                    {
                                        Registro += "#0";
                                    }
                                    if (LstTotalCuadroExtraordinario.ContainsKey(mes.Key))
                                    {
                                        LstTotalCuadroExtraordinario[mes.Key] += Total;
                                    }
                                    TotalGeneral += Total;
                                }
                                if (TotalGeneral > 0)
                                {
                                    Registro += "#" + String.Format("{0:#,##0.00}", TotalGeneral);
                                    LstTotalCuadroExtraordinario[-1] += TotalGeneral;
                                    LstCuadroExtraordinaria.Add(Registro);
                                }
                                TotalGeneral = 0;
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                //throw ex;
            }
            
        }
    }
}