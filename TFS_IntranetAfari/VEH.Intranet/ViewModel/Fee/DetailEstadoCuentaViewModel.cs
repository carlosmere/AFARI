using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Models;
using VEH.Intranet.Controllers;
using System.Data.Entity;
using PagedList;
using VEH.Intranet.Helpers;
using System.Web.Mvc;
using System.IO;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Fee
{
    public class DetailEstadoCuentaViewModel : BaseViewModel
    {
        public DetailEstadoCuentaViewModel() { LstAnio = new List<SelectListItem>(); LstMeses = new List<SelectListItem>(); }

        public Int32 DepartamentoId { get; set; }
        public IPagedList<Cuota> LstEstadoCuenta { get; set; }
        public Int32? np { get; set; }
        public Int32? Anio { get; set; }
        public Int32? Mes { get; set; }
        public List<SelectListItem> LstAnio { get; set; }
        public List<SelectListItem> LstMeses { get; set; }
        public Edificio Edificio { get; set; }
        public Departamento Departamento { get; set; }
        public Int32 EdificioId { get; set; }
        public String UltimaPagada { get; set; }
        public Int32? UnidadTiempoId { get; set; }
        public ReporteEdificioUnidadTiempo reporte { get; set; }

        public void Fill(CargarDatosContext datacontext, Int32? _np,Controller c)
        {
            baseFill(datacontext);
            np = _np ?? 1;
            Edificio = datacontext.context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);
            Departamento = datacontext.context.Departamento.FirstOrDefault(x => x.DepartamentoId == DepartamentoId);

            var lstCuotas = datacontext.context.Cuota.OrderByDescending(x => x.UnidadTiempo.Anio).OrderByDescending(x => x.UnidadTiempo.Mes).
                Include(x => x.Departamento).
                Include(x => x.UnidadTiempo).
                Where(x => x.DepartamentoId == DepartamentoId).
                AsQueryable();

            var lstunidadtiempo = datacontext.context.UnidadTiempo.OrderByDescending(X => X.Orden).Where(x => x.Estado == ConstantHelpers.EstadoActivo);
            List<Int32> valAnio = new List<int>();
            List<Int32> valMes = new List<int>();
            var mesActivo = lstunidadtiempo.FirstOrDefault(x => x.EsActivo).Mes;
            foreach (var item in lstunidadtiempo)
            {
                var mes = item.Descripcion.Substring(0, item.Descripcion.Length - 4);

                if (!valAnio.Contains(item.Anio))
                {
                    LstAnio.Add(new SelectListItem { Value = item.Anio.ToString(), Text = item.Anio.ToString() });
                    valAnio.Add(item.Anio);
                }
                if (!valMes.Contains(item.Mes) && item.Mes <= mesActivo)
                {
                    LstMeses.Add(new SelectListItem { Value = item.Mes.ToString(), Text = mes });
                    valMes.Add(item.Mes);
                }
            }
            if (Anio.HasValue && Mes.HasValue == false)
            {
                lstCuotas = lstCuotas.Where(x => x.UnidadTiempo.Anio == Anio.Value && x.DepartamentoId == DepartamentoId);

                var unidadTiempo = datacontext.context.UnidadTiempo.FirstOrDefault(X => X.Anio == Anio);
                if (unidadTiempo != null)
                {
                    var correcion = datacontext.context.ArchivoCorrecionEdificio.FirstOrDefault(X => X.Tipo.Contains(ConstantHelpers.TipoArchivo.ReporteGeneral) && X.EdificioId == EdificioId && X.UnidadTiempo.Anio == Anio);
                    if (correcion != null)
                    {
                        //var ruta = "/intranet/Resources/Files/Corregidos/"+ correcion.RutaArchivo;
                        var ruta = "~/Resources/Files/Corregidos/" + correcion.RutaArchivo;
                        
                        string fileName = "Reporte general - " + datacontext.context.Edificio.FirstOrDefault(X => X.EdificioId == EdificioId).Nombre + " - " + unidadTiempo.Descripcion + ".pdf";
                        reporte = new ReporteEdificioUnidadTiempo();
                        reporte.Ruta = ruta;
                        reporte.Nombre = fileName;
                    }
                    else
                        reporte = datacontext.context.ReporteEdificioUnidadTiempo.FirstOrDefault(X => X.UnidadTiempo.Anio == Anio.Value && X.EdificioId == EdificioId);
                }
            }
            else if (Mes.HasValue && Anio.HasValue == false)
            {
                lstCuotas = lstCuotas.Where(x => x.UnidadTiempo.Mes == Mes.Value && x.DepartamentoId == DepartamentoId);

                var unidadTiempo = datacontext.context.UnidadTiempo.FirstOrDefault(X => X.Mes == Mes);
                if (unidadTiempo != null)
                {
                    var correcion = datacontext.context.ArchivoCorrecionEdificio.FirstOrDefault(X => X.Tipo.Contains(ConstantHelpers.TipoArchivo.ReporteGeneral) && X.EdificioId == EdificioId && X.UnidadTiempo.Mes == Mes);
                    if (correcion != null)
                    {
                        //var ruta = "/intranet/Resources/Files/Corregidos/"+ correcion.RutaArchivo;
                        var ruta = "~/Resources/Files/Corregidos/" + correcion.RutaArchivo;

                        string fileName = "Reporte general - " + datacontext.context.Edificio.FirstOrDefault(X => X.EdificioId == EdificioId).Nombre + " - " + unidadTiempo.Descripcion + ".pdf";
                        reporte = new ReporteEdificioUnidadTiempo();
                        reporte.Ruta = ruta;
                        reporte.Nombre = fileName;
                    }
                    else
                        reporte = datacontext.context.ReporteEdificioUnidadTiempo.FirstOrDefault(X => X.UnidadTiempo.Mes == Mes.Value && X.EdificioId == EdificioId);
                }
            }
            else if (Mes.HasValue && Anio.HasValue)
            {
                lstCuotas = lstCuotas.Where(x => x.UnidadTiempo.Mes == Mes.Value && x.DepartamentoId == DepartamentoId && x.UnidadTiempo.Anio == Anio);

                var unidadTiempo = datacontext.context.UnidadTiempo.FirstOrDefault(X => X.Mes == Mes && X.Anio == Anio);

                

                if (unidadTiempo != null)
                {
                    UnidadTiempoId = unidadTiempo.UnidadTiempoId;
                    var correcion = datacontext.context.ArchivoCorrecionEdificio.FirstOrDefault(X => X.Tipo.Contains(ConstantHelpers.TipoArchivo.ReporteGeneral) && X.EdificioId == EdificioId && X.UnidadTiempo.Mes == Mes && X.UnidadTiempo.Anio == Anio);
                    if (correcion != null)
                    {
                        //var ruta = "/intranet/Resources/Files/Corregidos/"+ correcion.RutaArchivo;
                        var ruta = "~/Resources/Files/Corregidos/" + correcion.RutaArchivo;

                        string fileName = "Reporte general - " + datacontext.context.Edificio.FirstOrDefault(X => X.EdificioId == EdificioId).Nombre + " - " + unidadTiempo.Descripcion + ".pdf";
                        reporte = new ReporteEdificioUnidadTiempo();
                        reporte.Ruta = ruta;
                        reporte.Nombre = fileName;
                    }
                    else
                        reporte = datacontext.context.ReporteEdificioUnidadTiempo.FirstOrDefault(X => X.UnidadTiempo.Mes == Mes.Value && X.EdificioId == EdificioId && X.UnidadTiempo.Anio == Anio);
                }
            }
            else
            {
                Int32 unidadtiempoid = lstunidadtiempo.First().UnidadTiempoId;
                lstCuotas = lstCuotas.Where(x => x.DepartamentoId == DepartamentoId && x.UnidadTiempoId == unidadtiempoid);
            }
            var b= datacontext.context.Cuota.Where(X => X.DepartamentoId == DepartamentoId && X.Pagado).OrderByDescending(X => X.UnidadTiempo.Orden).ToList();
            var ultimaTiempoPagada = b.FirstOrDefault();
            if (ultimaTiempoPagada != null)
            {
                UltimaPagada = ultimaTiempoPagada.UnidadTiempo.Descripcion;

            }
            else
            {
                UltimaPagada = "Nunca pago cuota";
            }

            LstEstadoCuenta = lstCuotas.ToPagedList(np.Value, ConstantHelpers.DEFAULT_PAGE_SIZE);
        }
    }
}