using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VEH.Intranet.Models;
using VEH.Intranet.Models.BE;
using System.Data.Entity;
using VEH.Intranet.Helpers;
using System.IO;
using System.Net.Http.Headers;
using VEH.Intranet.Logic;
using OfficeOpenXml.Style;
using OfficeOpenXml;

namespace VEH.Intranet.Controllers
{
    public class AfariServiceController : ApiController
    {
        public SIVEHEntities context;
        public AfariServiceController()
        {
            context = new SIVEHEntities();
        }
        [Route("api/AfariService/GetEstadosCuentaBancario")]
        [HttpGet]
        public GetEstadosCuentaBancario GetEstadosCuentaBancario(Int32 edificioId, Int32? anio)
        {
            GetEstadosCuentaBancario GetEstadosCuentaBancario = new GetEstadosCuentaBancario();
            var baseRuta = "http://afari.pe/intranet/Files/";
            try
            {
                try
                {
                    var query = context.EstadoCuentaBancario.Where(x => x.EdificioId == edificioId && x.Estado == ConstantHelpers.EstadoActivo).OrderByDescending(x => x.UnidadTiempoId).AsQueryable();
                    if (anio.HasValue)
                    {
                        query = query.Where( x => x.UnidadTiempo.Anio == anio);
                    }

                    GetEstadosCuentaBancario.lstEstadoCuenta = query.Select(x => new EstadoCuentaBancarioBE
                    {
                        estadoCuentaId = x.EstadoCuentaBancarioId,
                        unidadTiempoId = x.UnidadTiempoId,
                        descripcionUnidadTiempo = x.UnidadTiempo.Descripcion,
                        documento = baseRuta + x.Ruta
                    }).ToList();
                }
                catch (Exception ex)
                {
                    GetEstadosCuentaBancario.error = true;
                    GetEstadosCuentaBancario.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
                }
            }
            catch (Exception ex)
            {
                GetEstadosCuentaBancario.error = true;
                GetEstadosCuentaBancario.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
            }

            return GetEstadosCuentaBancario;
        }
        [Route("api/AfariService/GetCronograma")]
        [HttpGet]
        public HttpResponseMessage GetCronograma(Int32 edificioId, Int32 anio)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            try
            {
                ReporteLogic reporteLogic = new ReporteLogic();
                reporteLogic.context = context;

                var objCronograma = context.Cronograma.FirstOrDefault(x => x.Anio == anio && x.EdificioId == edificioId);
                var lstCronograma = context.Cronograma.Where(x => x.Anio == anio && x.EdificioId == edificioId && x.Estado == ConstantHelpers.EstadoActivo).OrderBy(x => x.Orden).ToList();
                var edificioNombre = objCronograma.Edificio.Nombre;
                var titulo = "Cronograma de Mantenimientos\n " + edificioNombre + "\nAño " + objCronograma.Anio.ToString();

                reporteLogic.GetReportMantenimientoAPI(titulo, lstCronograma);

                MemoryStream outputMemoryStream = reporteLogic.getFirstReport();

                if (outputMemoryStream == null)
                {
                    response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }

                response.Content = new StreamContent(outputMemoryStream);

                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = "Cronograma Mantenimiento - " + objCronograma.Edificio.Nombre + " - " + objCronograma.Anio.ToString() + ".pdf"
                };
                response.Content.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");

                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            }
            catch (Exception ex)
            {
                response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }

            return response;
        }
        [Route("api/AfariService/GetCronogramasMantenimientos")]
        [HttpGet]
        public ResponseGetGetCronogramasMantenimientos GetCronogramasMantenimientos(Int32 edificioId)
        {
            ResponseGetGetCronogramasMantenimientos ResponseGetGetCronogramasMantenimientos = new ResponseGetGetCronogramasMantenimientos();

            try
            {
                try
                {
                    var query = context.Cronograma.Where(X => X.EdificioId == edificioId && X.Estado == ConstantHelpers.EstadoActivo).AsQueryable();

                    ResponseGetGetCronogramasMantenimientos.lstCronograma = query.Select(x => new CronogramaBE
                    {
                        //cronogramaId = x.CronogramaId,
                        anio = x.Anio
                    }).Distinct().OrderByDescending(x => x.anio).ToList();
                }
                catch (Exception ex)
                {
                    ResponseGetGetCronogramasMantenimientos.error = true;
                    ResponseGetGetCronogramasMantenimientos.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
                }
            }
            catch (Exception ex)
            {
                ResponseGetGetCronogramasMantenimientos.error = true;
                ResponseGetGetCronogramasMantenimientos.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
            }

            return ResponseGetGetCronogramasMantenimientos;
        }
        [Route("api/AfariService/GetCertificadosEquipos")]
        [HttpGet]
        public ResponseGetCertificadosEquipos GetCertificadosEquipos(Int32 edificioId, Int32? anio)
        {
            ResponseGetCertificadosEquipos ResponseGetCertificadosEquipos = new ResponseGetCertificadosEquipos();
            var filtroTipo = "Equipo";
            var baseRuta = "http://afari.pe/intranet/Resources/Files/";
            try
            {
                try
                {
                    var query = context.DatoEdificio.Where(X => X.EdificioId == edificioId && X.Tipo.Contains(filtroTipo)).AsQueryable();
                    if (anio.HasValue)
                    {
                        query = query.Where(x => x.UnidadTiempo.Anio == anio);
                    }

                    ResponseGetCertificadosEquipos.lstCertficadosEquipos = query.Select(x => new ItemOL
                    {
                        id = x.DatoEdificioId,
                        Nombre = x.Nombre,
                        unidadTiempoId = x.UnidadTiempoId,
                        detalleUnidadTiempo = x.UnidadTiempo.Descripcion,
                        documento = baseRuta + x.Dato,
                        tipo = x.Tipo.Replace(filtroTipo, String.Empty)
                    }).OrderBy(x => x.tipo).ToList();
                }
                catch (Exception ex)
                {
                    ResponseGetCertificadosEquipos.error = true;
                    ResponseGetCertificadosEquipos.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
                }
            }
            catch (Exception ex)
            {
                ResponseGetCertificadosEquipos.error = true;
                ResponseGetCertificadosEquipos.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
            }

            return ResponseGetCertificadosEquipos;
        }
        [Route("api/AfariService/GetObligacionesLaborales")]
        [HttpGet]
        public ResponseGetObligacionesLaborales GetObligacionesLaborales(Int32 edificioId, Int32? anio)
        {
            ResponseGetObligacionesLaborales ResponseGetObligacionesLaborales = new ResponseGetObligacionesLaborales();
            var filtroTipo = "ObligacionesLaborales";
            var baseRuta = "http://afari.pe/intranet/Resources/Files/";
            try
            {
                try
                {
                    var query = context.DatoEdificio.Where(X => X.EdificioId == edificioId && X.Tipo.Contains(filtroTipo)).AsQueryable();
                    if (anio.HasValue)
                    {
                        query = query.Where(x => x.UnidadTiempo.Anio == anio);
                    }

                    ResponseGetObligacionesLaborales.lstObligacionesLaborales = query.Select(x => new ItemOL
                    {
                        id = x.DatoEdificioId,
                        Nombre = x.Nombre,
                        unidadTiempoId = x.UnidadTiempoId,
                        detalleUnidadTiempo = x.UnidadTiempo.Descripcion,
                        documento = baseRuta + x.Dato,
                        tipo = x.Tipo.Replace(filtroTipo, String.Empty)
                    }).OrderBy(x => x.tipo).ToList();
                }
                catch (Exception ex)
                {
                    ResponseGetObligacionesLaborales.error = true;
                    ResponseGetObligacionesLaborales.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
                }
            }
            catch (Exception ex)
            {
                ResponseGetObligacionesLaborales.error = true;
                ResponseGetObligacionesLaborales.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
            }

            return ResponseGetObligacionesLaborales;
        }
        [Route("api/AfariService/GetNoticias")]
        [HttpGet]
        public ResponseGetNoticias GetNoticias(Int32 edificioId)
        {
            ResponseGetNoticias ResponseGetNoticias = new ResponseGetNoticias();

            try
            {
                try
                {
                    var query = context.Noticia
                                .OrderByDescending(x => x.Fecha)
                                .Include(x => x.Edificio)
                                .Where(x => x.Estado == ConstantHelpers.EstadoActivo && x.EdificioId == edificioId).ToList();

                    ResponseGetNoticias.lstNoticia = query.Select(x => new NoticiaBE
                    {
                        titulo = x.Titulo,
                        detalle = x.Detalle,
                        fecha = x.Fecha
                    }).OrderByDescending(x => x.fecha).ToList();
                }
                catch (Exception ex)
                {
                    ResponseGetNoticias.error = true;
                    ResponseGetNoticias.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
                }
            }
            catch (Exception ex)
            {
                ResponseGetNoticias.error = true;
                ResponseGetNoticias.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
            }

            return ResponseGetNoticias;
        }
        [Route("api/AfariService/GetTrabajadores")]
        [HttpGet]
        public ResponseGetTrabajadores GetTrabajadores(Int32 edificioId)
        {
            ResponseGetTrabajadores ResponseGetTrabajadores = new ResponseGetTrabajadores();

            try
            {
                try
                {
                    var baseRuta = "http://afari.pe/intranet/Resources/Fotos/";
                    var query = context.Trabajador
                                .Include(x => x.Edificio)
                                .OrderBy(x => x.Nombres)
                                .OrderBy(x => x.Apellidos)
                                .Where(x => x.Estado == ConstantHelpers.EstadoActivo && x.Edificio.Estado == ConstantHelpers.EstadoActivo)
                                .AsQueryable();

                    ResponseGetTrabajadores.lstTrabajador = query.Select( x => new TrabajadorBE { trabajadorId = x.TrabajadorId,
                    nombre = x.Nombres + " " + x.Apellidos , cargo = x.Cargo , dni = x.DNI ,
                    foto = baseRuta + (!String.IsNullOrEmpty(x.Foto) ? x.Foto : ("default_worker.png"))
                    }).OrderBy( x => x.nombre).ToList();
                }
                catch (Exception ex)
                {
                    ResponseGetTrabajadores.error = true;
                    ResponseGetTrabajadores.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
                }
            }
            catch (Exception ex)
            {
                ResponseGetTrabajadores.error = true;
                ResponseGetTrabajadores.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
            }

            return ResponseGetTrabajadores;
        }
        [Route("api/AfariService/GetDetalleIngresos")]
        [HttpGet]
        public ResponseGetDetalleIngresos GetDetalleIngresos(Int32 ingresoId)
        {
            ResponseGetDetalleIngresos ResponseGetDetalleIngresos = new ResponseGetDetalleIngresos();

            try
            {
                try
                {
                    var query = context.DetalleIngreso
                                .Include(x => x.Ingreso)
                                .Include(x => x.Ingreso.Edificio)
                                .OrderByDescending(x => x.FechaRegistro)
                                .Where(x => x.Estado == ConstantHelpers.EstadoActivo && x.IngresoId == ingresoId)
                                .AsQueryable();


                    ResponseGetDetalleIngresos.lstDetalleIngreso = query.Select(x => new DetalleIngresoBE
                    {
                        nombre = x.Concepto,
                        monto = x.Monto
                    }).ToList();

                }
                catch (Exception ex)
                {
                    ResponseGetDetalleIngresos.error = true;
                    ResponseGetDetalleIngresos.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
                }
            }
            catch (Exception ex)
            {
                ResponseGetDetalleIngresos.error = true;
                ResponseGetDetalleIngresos.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
            }

            return ResponseGetDetalleIngresos;
        }
        [Route("api/AfariService/GetIngresos")]
        [HttpGet]
        public ResponseGetIngresos GetIngresos(Int32 edificioId, Int32 anio)
        {
            ResponseGetIngresos ResponseGetIngresos = new ResponseGetIngresos();

            try
            {
                try
                {
                    var query = context.Ingreso.OrderByDescending(x => x.FechaRegistro).Where(x => x.Estado == ConstantHelpers.EstadoActivo && x.EdificioId == edificioId
                    && x.UnidadTiempo.Anio == anio).AsQueryable();

                    ResponseGetIngresos.lstIngreso = query.Select(x => new IngresoBE
                    {
                        ingresoId = x.IngresoId,
                        unidadTiempoId = x.UnidadTiempoId,
                        unidadTiempoDescripcion = x.UnidadTiempo.Descripcion
                    }).ToList();

                }
                catch (Exception ex)
                {
                    ResponseGetIngresos.error = true;
                    ResponseGetIngresos.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
                }
            }
            catch (Exception ex)
            {
                ResponseGetIngresos.error = true;
                ResponseGetIngresos.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
            }

            return ResponseGetIngresos;
        }
        [Route("api/AfariService/GetGastos")]
        [HttpGet]
        public ResponseGetDetalleGastos GetDetalleGastos(Int32 gastoId)
        {
            ResponseGetDetalleGastos ResponseGetDetalleGastos = new ResponseGetDetalleGastos();

            try
            {
                try
                {
                    var query = context.DetalleGasto
                                .Include(x => x.Gasto)
                                .Include(x => x.Gasto.Edificio)
                                .OrderByDescending(x => x.FechaRegistro)
                                .Where(x => x.Estado == ConstantHelpers.EstadoActivo && x.GastoId == gastoId)
                                .AsQueryable();


                    ResponseGetDetalleGastos.lstDetalleGasto = query.Select(x => new DetalleGastoBE
                    {
                        nombre = x.Concepto,
                        monto = x.Monto
                    }).ToList();

                }
                catch (Exception ex)
                {
                    ResponseGetDetalleGastos.error = true;
                    ResponseGetDetalleGastos.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
                }
            }
            catch (Exception ex)
            {
                ResponseGetDetalleGastos.error = true;
                ResponseGetDetalleGastos.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
            }

            return ResponseGetDetalleGastos;
        }
        [Route("api/AfariService/GetGastos")]
        [HttpGet]
        public ResponseGetGastos GetGastos(Int32 edificioId, Int32 anio)
        {
            ResponseGetGastos ResponseGetGastos = new ResponseGetGastos();

            try
            {
                try
                {
                    var query = context.Gasto.OrderByDescending(x => x.FechaRegistro).Where(x => x.Estado == ConstantHelpers.EstadoActivo && x.EdificioId == edificioId
                    && x.UnidadTiempo.Anio == anio).AsQueryable();

                    ResponseGetGastos.lstGasto = query.Select(x => new GastoBE
                    {
                        gastoId = x.GastoId,
                        unidadTiempoId = x.UnidadTiempoId,
                        unidadTiempoDescripcion = x.UnidadTiempo.Descripcion
                    }).ToList();

                }
                catch (Exception ex)
                {
                    ResponseGetGastos.error = true;
                    ResponseGetGastos.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
                }
            }
            catch (Exception ex)
            {
                ResponseGetGastos.error = true;
                ResponseGetGastos.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
            }

            return ResponseGetGastos;
        }
        [Route("api/AfariService/GetCuotaPorDepartamento")]
        public HttpResponseMessage GetCuotaPorDepartamento(Int32 departamentoId, Int32 unidadTiempoId)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            try
            {
                ReporteLogic reporteLogic = new ReporteLogic();
                reporteLogic.context = context;

                var departamento = context.Departamento.Where(x => x.DepartamentoId == departamentoId).FirstOrDefault();
                DateTime? fechaEmision = null;
                DateTime? fechaVencimiento = null;

                List<Cuota> listaCuota = new List<Cuota>();
                var departamentos = departamento.Edificio.Departamento.ToList();
                foreach (var depa in departamentos)
                {
                    Cuota cuota = context.Cuota.FirstOrDefault(x => x.DepartamentoId == depa.DepartamentoId && x.UnidadTiempoId == unidadTiempoId && x.UnidadTiempo.Estado == ConstantHelpers.EstadoActivo);
                    if (cuota == null) continue;
                    if (!fechaEmision.HasValue) fechaEmision = cuota.FechaEmision;
                    if (!fechaVencimiento.HasValue) fechaVencimiento = cuota.FechaVencimiento;
                    //listaCuota.Add(cuota);
                    if (cuota.EsExtraordinaria.HasValue && cuota.EsExtraordinaria.Value)
                    {
                        var validacionExtra = listaCuota.FirstOrDefault(x => x.DepartamentoId == cuota.DepartamentoId);
                        if (validacionExtra != null && cuota.FechaPagado != null)
                        {
                            listaCuota.Remove(validacionExtra);

                            if (cuota.UnidadTiempo.Mes == validacionExtra.UnidadTiempo.Mes)
                            {
                                validacionExtra.CuotaExtraordinaria += cuota.CuotaExtraordinaria;
                                validacionExtra.Total += cuota.CuotaExtraordinaria ?? 0;
                            }
                            else if (cuota.UnidadTiempo.Mes != validacionExtra.UnidadTiempo.Mes)
                            {
                                validacionExtra.Total += cuota.CuotaExtraordinaria ?? 0;
                            }

                            listaCuota.Add(validacionExtra);
                        }
                        else
                        {
                            listaCuota.Add(cuota);
                        }
                    }
                    else
                    {
                        listaCuota.Add(cuota);
                    }
                }

                UnidadTiempo unidad = context.UnidadTiempo.Where(x => x.UnidadTiempoId == unidadTiempoId).FirstOrDefault();

                var name = reporteLogic.GetReportTableAPI(listaCuota, unidad.Descripcion);

                MemoryStream outputMemoryStream = reporteLogic.getFirstReport();
                if (outputMemoryStream == null)
                {
                    response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }

                response.Content = new StreamContent(outputMemoryStream);

                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = name
                };
                response.Content.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");

                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            }
            catch (Exception ex)
            {
                response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            
            return response;
        }
        [Route("api/AfariService/GetCuadroMorosidad")]
        public HttpResponseMessage GetCuadroMorosidad(Int32 edificioId)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            var ruta = System.Web.Hosting.HostingEnvironment.MapPath(@"~\Files\CuadroMoroso.xlsx");
            try
            {
                var Edificio = context.Edificio.FirstOrDefault(x => x.EdificioId == edificioId).Nombre;
                System.Drawing.Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#80cd32");
                using (FileStream fs = System.IO.File.OpenRead(ruta))
                using (ExcelPackage excelPackage = new ExcelPackage(fs))
                {
                    ExcelWorkbook excelWorkBook = excelPackage.Workbook;
                    ExcelWorksheet excelWorksheet = excelWorkBook.Worksheets.FirstOrDefault();

                    if (excelWorksheet != null)
                    {
                        var unidadTiempoActivo = context.UnidadTiempo.FirstOrDefault(X => X.EsActivo);

                        var LstCuotasT = context.Cuota.Include(x => x.Departamento)
                            .Include(x => x.UnidadTiempo)
                            .Include(x => x.Departamento.Propietario)
                            .Where(x => x.Departamento.EdificioId == edificioId && x.Pagado == false && x.UnidadTiempoId < unidadTiempoActivo.UnidadTiempoId && x.UnidadTiempo.Estado == ConstantHelpers.EstadoActivo).OrderBy(x => x.UnidadTiempo.Orden).ThenBy(x => x.CuotaId).ToList();

                        List<Cuota> LstCuotas = new List<Cuota>();

                        foreach (var cuota in LstCuotasT)
                        {
                            if (cuota.EsExtraordinaria.HasValue && cuota.EsExtraordinaria.Value)
                            {
                                var validacionExtra = LstCuotas.FirstOrDefault(x => x.DepartamentoId == cuota.DepartamentoId);
                                if (validacionExtra != null)
                                {

                                    if (validacionExtra.UnidadTiempo.Mes == validacionExtra.UnidadTiempo.Mes)
                                    {
                                        LstCuotas.Remove(validacionExtra);
                                        validacionExtra.CuotaExtraordinaria += cuota.CuotaExtraordinaria;
                                        validacionExtra.Total += cuota.CuotaExtraordinaria ?? 0;
                                        LstCuotas.Add(validacionExtra);
                                    }
                                    else if (validacionExtra.UnidadTiempo.Mes != validacionExtra.UnidadTiempo.Mes)
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

                        //var LstCuotas = context.usp_SelCuotasMoras(EdificioId, unidadTiempoActivo.UnidadTiempoId).ToList();

                        var Mes = String.Empty;
                        var NombreInquilino = String.Empty;
                        var ContNombreInquilino = 0;
                        Int32 Col = 4;
                        Dictionary<Int32, Int32> LstMeses = new Dictionary<Int32, Int32>();
                        foreach (var item in LstCuotas)
                        {
                            if (!LstMeses.ContainsKey(item.UnidadTiempo.Orden.Value))
                            {
                                excelWorksheet.Cells[7, Col].Value = item.UnidadTiempo.Descripcion.Substring(0, 3) + "-" + item.UnidadTiempo.Descripcion.Substring(item.UnidadTiempo.Descripcion.Length - 4);
                                excelWorksheet.Cells[7, Col].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                                excelWorksheet.Cells[7, Col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                excelWorksheet.Cells[7, Col].Style.Fill.BackgroundColor.SetColor(colFromHex);

                                Mes = item.UnidadTiempo.Descripcion;
                                LstMeses.Add(item.UnidadTiempo.Orden.Value, Col);
                                Col++;
                            }
                        }
                        excelWorksheet.Cells[7, Col].Value = "Total";
                        excelWorksheet.Cells[7, Col].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                        excelWorksheet.Cells[7, Col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        excelWorksheet.Cells[7, Col].Style.Fill.BackgroundColor.SetColor(colFromHex);

                        //excelWorksheet.Cells["A5"].Value = "EDIFICIO " + Edificio;
                        excelWorksheet.Cells[4, 1, 4, Col].Merge = true;
                        excelWorksheet.Cells[5, 1, 5, Col].Merge = true;

                        excelWorksheet.Cells["A4"].Value = "MOROSIDAD CUOTAS ORDINARIAS";
                        excelWorksheet.Cells["A4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        excelWorksheet.Cells["A5"].Value = "EDIFICIO " + Edificio;
                        excelWorksheet.Cells["A5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        Int32 i = 8;
                        List<Int32> LstDepartamentoId = new List<Int32>();
                        LstCuotas = LstCuotas.OrderBy(x => x.DepartamentoId).ToList();
                        decimal? TotalGeneral = 0;
                        foreach (var item in LstCuotas)
                        {
                            if (LstDepartamentoId.Contains(item.DepartamentoId) == false)
                            {
                                decimal? Total = 0;
                                var objTitular = item.Departamento.Propietario.FirstOrDefault(x => x.ParentescoTitular.Contains("Titular") && x.Estado == ConstantHelpers.EstadoActivo);

                                if (objTitular == null)
                                    objTitular = item.Departamento.Propietario.FirstOrDefault(x => x.Estado == ConstantHelpers.EstadoActivo);


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

                                foreach (var mes in LstMeses)
                                {
                                    var cuota = LstCuotas.FirstOrDefault(x => x.Departamento.Numero == item.Departamento.Numero && x.UnidadTiempo.Orden == mes.Key);
                                    //excelWorksheet.Cells[i, mes.Value].Value = cuota != null ? String.Format("{0:N}", cuota.Total - cuota.CuotaExtraordinaria) : String.Empty;
                                    excelWorksheet.Cells[i, mes.Value].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                                    //Total += cuota != null ? (cuota.Total - cuota.CuotaExtraordinaria) : 0;

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
                                            //else
                                            //{
                                            //    fechaComparar = new DateTime(cuota.UnidadTiempo.Anio, cuota.UnidadTiempo.Mes + 1, cuota.Departamento.Edificio.DiaMora ?? 30);
                                            //}
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
                                                excelWorksheet.Cells[i, mes.Value].Value = cuota != null ? String.Format("{0:N}", cuota.Total - cuota.CuotaExtraordinaria) : String.Empty;
                                                Total += cuota != null ? ((cuota.Total - cuota.CuotaExtraordinaria).Value) : 0;
                                            }
                                            else
                                            {
                                                excelWorksheet.Cells[i, mes.Value].Value = String.Empty;
                                            }
                                        }
                                        else
                                        {
                                            excelWorksheet.Cells[i, mes.Value].Value = cuota != null ? String.Format("{0:N}", cuota.Total - cuota.CuotaExtraordinaria) : String.Empty;
                                            Total += cuota != null ? ((cuota.Total - cuota.CuotaExtraordinaria).Value) : 0;
                                        }
                                    }
                                    else
                                    {
                                        excelWorksheet.Cells[i, mes.Value].Value = cuota != null ? String.Format("{0:N}", cuota.Total - cuota.CuotaExtraordinaria) : String.Empty;
                                        Total += cuota != null ? ((cuota.Total - cuota.CuotaExtraordinaria).Value) : 0;
                                    }
                                }
                                if (Total > 0)
                                {
                                    excelWorksheet.Cells["A" + i].Value = item.Departamento.Numero;
                                    excelWorksheet.Cells["A" + i].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                                    excelWorksheet.Cells["B" + i].Value = objTitular != null ? objTitular.Nombres : String.Empty;
                                    excelWorksheet.Cells["B" + i].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                                    excelWorksheet.Cells["C" + i].Value = NombreInquilino;
                                    excelWorksheet.Cells["C" + i].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                                    TotalGeneral += Total;
                                    excelWorksheet.Cells[i, Col].Value = Total;
                                    excelWorksheet.Cells[i, Col].Style.Font.Bold = true;
                                    excelWorksheet.Cells[i, Col].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                                    i++;
                                }
                                //TotalGeneral = 0;
                                Total = 0;
                                //LstDepartamentoId.Add(item.DepartamentoId);

                                var lstHistoria = context.DepartamentoHistorico.Where(x => x.DepartamentoId == item.DepartamentoId && x.Fecha < objTitular.FechaCreacion).ToList();
                                if (lstHistoria.Count > 0)
                                {
                                    var objTitular2 = item.Departamento.Propietario.FirstOrDefault(x => x.ParentescoTitular.Contains("Titular") && x.Estado == ConstantHelpers.EstadoActivo);

                                    if (objTitular2 == null)
                                        objTitular2 = item.Departamento.Propietario.FirstOrDefault(x => x.Estado == ConstantHelpers.EstadoActivo);

                                    foreach (var historia in lstHistoria)
                                    {
                                        objTitular = historia.Propietario;

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

                                        foreach (var mes in LstMeses)
                                        {
                                            //Total = 0;
                                            var cuota = LstCuotas.FirstOrDefault(x => x.Departamento.Numero == item.Departamento.Numero && x.UnidadTiempo.Orden == mes.Key);
                                            //excelWorksheet.Cells[i, mes.Value].Value = cuota != null ? String.Format("{0:N}", cuota.Total - cuota.CuotaExtraordinaria) : String.Empty;
                                            excelWorksheet.Cells[i, mes.Value].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                                            //Total += cuota != null ? (cuota.Total - cuota.CuotaExtraordinaria) : 0;

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


                                                if (objTitular.FechaCreacion.HasValue)
                                                {
                                                    //if (cuota.FechaEmision >= objTitular.FechaCreacion.Value.Date && cuota.FechaEmision <= historia.Fecha)
                                                    if (fechaComparar >= historia.Propietario.FechaCreacion.Value.Date &&
                                        (fechaComparar < objTitular2.FechaCreacion.Value.Date
                                        || (fechaComparar.Month == historia.Propietario.FechaCreacion.Value.Month
                                        && fechaComparar.Year == historia.Propietario.FechaCreacion.Value.Year)
                                        ))
                                                    {
                                                        excelWorksheet.Cells[i, mes.Value].Value = cuota != null ? String.Format("{0:N}", cuota.Total - cuota.CuotaExtraordinaria) : String.Empty;
                                                        Total += cuota != null ? ((cuota.Total - cuota.CuotaExtraordinaria).Value) : 0;
                                                    }
                                                    else
                                                    {
                                                        excelWorksheet.Cells[i, mes.Value].Value = String.Empty;
                                                    }
                                                }
                                                else
                                                {
                                                    excelWorksheet.Cells[i, mes.Value].Value = cuota != null ? String.Format("{0:N}", cuota.Total - cuota.CuotaExtraordinaria) : String.Empty;
                                                    Total += cuota != null ? ((cuota.Total - cuota.CuotaExtraordinaria).Value) : 0;
                                                }
                                            }
                                            else
                                            {
                                                excelWorksheet.Cells[i, mes.Value].Value = cuota != null ? String.Format("{0:N}", cuota.Total - cuota.CuotaExtraordinaria) : String.Empty;
                                                Total += cuota != null ? ((cuota.Total - cuota.CuotaExtraordinaria).Value) : 0;
                                            }
                                        }

                                        if (Total > 0)
                                        {
                                            excelWorksheet.Cells["A" + i].Value = item.Departamento.Numero;
                                            excelWorksheet.Cells["A" + i].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                                            excelWorksheet.Cells["B" + i].Value = objTitular != null ? objTitular.Nombres : String.Empty;
                                            excelWorksheet.Cells["B" + i].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                                            excelWorksheet.Cells["C" + i].Value = NombreInquilino;
                                            excelWorksheet.Cells["C" + i].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                                            TotalGeneral += Total;
                                            excelWorksheet.Cells[i, Col].Value = Total;
                                            excelWorksheet.Cells[i, Col].Style.Font.Bold = true;
                                            excelWorksheet.Cells[i, Col].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                                            i++;
                                        }
                                    }
                                }

                                LstDepartamentoId.Add(item.DepartamentoId);
                            }

                        }
                        excelWorksheet.Cells[i, Col].Value = TotalGeneral;
                        excelWorksheet.Cells[i, Col].Style.Font.Bold = true;
                        excelWorksheet.Cells[i, Col].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                        excelWorksheet.Cells["A" + i].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                        excelWorksheet.Cells["B" + i].Value = "Totales:";
                        excelWorksheet.Cells["B" + i].Style.Font.Bold = true;
                        excelWorksheet.Cells["B" + i].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                        excelWorksheet.Cells["C" + i].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                        foreach (var mes in LstMeses)
                        {
                            excelWorksheet.Cells[i, mes.Value].Value = LstCuotas.Where(x => x.UnidadTiempo.Orden == mes.Key).Sum(x => x.Total) - LstCuotas.Where(x => x.UnidadTiempo.Orden == mes.Key).Sum(x => x.CuotaExtraordinaria);
                            excelWorksheet.Cells[i, mes.Value].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                            excelWorksheet.Cells[i, mes.Value].Style.Font.Bold = true;
                        }

                        if (ContNombreInquilino == 0)
                        {
                            excelWorksheet.DeleteColumn(3);
                        }

                        i += 5;
                        LstCuotas = LstCuotas.Where(x => x.CuotaExtraordinaria > 0).ToList();

                        if (LstCuotas.Count > 0)
                        {
                            excelWorksheet.Cells["A" + i].Value = "Dpto.";
                            excelWorksheet.Cells["A" + i].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            excelWorksheet.Cells["A" + i].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            excelWorksheet.Cells["A" + i].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                            excelWorksheet.Cells["A" + i].Style.Fill.BackgroundColor.SetColor(colFromHex);
                            excelWorksheet.Cells["A" + i].Style.Font.Size = 8;
                            excelWorksheet.Cells["A" + i].Style.Font.Bold = true;

                            excelWorksheet.Cells["B" + i].Value = "Propietario";
                            excelWorksheet.Cells["B" + i].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            excelWorksheet.Cells["B" + i].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            excelWorksheet.Cells["B" + i].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                            excelWorksheet.Cells["B" + i].Style.Fill.BackgroundColor.SetColor(colFromHex);
                            excelWorksheet.Cells["B" + i].Style.Font.Size = 8;
                            excelWorksheet.Cells["B" + i].Style.Font.Bold = true;



                            Col = 3;
                            LstMeses = new Dictionary<Int32, Int32>();
                            foreach (var item in LstCuotas)
                            {
                                if (!LstMeses.ContainsKey(item.UnidadTiempo.Orden.Value))
                                {
                                    excelWorksheet.Cells[i, Col].Value = item.UnidadTiempo.Descripcion.Substring(0, 3) + "-" + item.UnidadTiempo.Descripcion.Substring(item.UnidadTiempo.Descripcion.Length - 4);
                                    excelWorksheet.Cells[i, Col].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                                    excelWorksheet.Cells[i, Col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                    excelWorksheet.Cells[i, Col].Style.Fill.BackgroundColor.SetColor(colFromHex);
                                    excelWorksheet.Cells[i, Col].Style.Font.Size = 8;
                                    excelWorksheet.Cells[i, Col].Style.Font.Bold = true;
                                    excelWorksheet.Cells[i, Col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    Mes = item.UnidadTiempo.Descripcion;
                                    LstMeses.Add(item.UnidadTiempo.Orden.Value, Col);
                                    Col++;
                                }
                            }
                            excelWorksheet.Cells[i, Col].Value = "Total";
                            excelWorksheet.Cells[i, Col].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                            excelWorksheet.Cells[i, Col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            excelWorksheet.Cells[i, Col].Style.Fill.BackgroundColor.SetColor(colFromHex);
                            excelWorksheet.Cells[i, Col].Style.Font.Size = 8;
                            excelWorksheet.Cells[i, Col].Style.Font.Bold = true;
                            excelWorksheet.Cells[i, Col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                            excelWorksheet.Cells[(i - 2), 1, (i - 2), Col].Merge = true;
                            excelWorksheet.Cells["A" + (i - 2)].Value = "MOROSIDAD CUOTAS EXTRAORDINARIAS";
                            excelWorksheet.Cells["A" + (i - 2)].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            excelWorksheet.Cells["A" + (i - 2)].Style.Font.Size = 12;
                            excelWorksheet.Cells["A" + (i - 2)].Style.Font.Bold = true;


                            i++;
                            LstDepartamentoId = new List<Int32>();
                            LstCuotas = LstCuotas.OrderBy(x => x.DepartamentoId).ToList();
                            TotalGeneral = 0;
                            foreach (var item in LstCuotas)
                            {
                                if (LstDepartamentoId.Contains(item.DepartamentoId) == false)
                                {
                                    decimal? Total = 0;
                                    var objTitular = item.Departamento.Propietario.FirstOrDefault(x => x.ParentescoTitular.Contains("Titular") && x.Estado == ConstantHelpers.EstadoActivo);
                                    if (objTitular == null)
                                        objTitular = item.Departamento.Propietario.FirstOrDefault(x => x.Estado == ConstantHelpers.EstadoActivo);

                                    excelWorksheet.Cells["A" + i].Value = item.Departamento.Numero;
                                    excelWorksheet.Cells["A" + i].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                                    excelWorksheet.Cells["B" + i].Value = objTitular != null ? objTitular.Nombres : String.Empty;
                                    excelWorksheet.Cells["B" + i].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                                    foreach (var mes in LstMeses)
                                    {
                                        var cuota = LstCuotas.FirstOrDefault(x => x.Departamento.Numero == item.Departamento.Numero && x.UnidadTiempo.Orden == mes.Key);
                                        excelWorksheet.Cells[i, mes.Value].Value = cuota != null ? String.Format("{0:N}", cuota.CuotaExtraordinaria) : String.Empty;
                                        excelWorksheet.Cells[i, mes.Value].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                                        Total += cuota != null ? cuota.CuotaExtraordinaria : 0;

                                        try
                                        {
                                            //var lstHistoria = context.DepartamentoHistorico.Where(x => x.DepartamentoId == cuota.DepartamentoId && x.Propietario.ParentescoTitular == "Titular" && x.Fecha < objTitular.FechaCreacion).ToList();
                                            var lstHistoria = context.DepartamentoHistorico.Where(x => x.DepartamentoId == cuota.DepartamentoId && x.Fecha < objTitular.FechaCreacion).ToList();
                                            if (lstHistoria.Count == 0)
                                            {
                                                lstHistoria = context.DepartamentoHistorico.Where(x => x.DepartamentoId == cuota.DepartamentoId).ToList();
                                            }

                                            var objTitular2 = item.Departamento.Propietario.FirstOrDefault(x => x.ParentescoTitular.Contains("Titular") && x.Estado == ConstantHelpers.EstadoActivo);

                                            if (objTitular2 == null)
                                                objTitular2 = item.Departamento.Propietario.FirstOrDefault(x => x.Estado == ConstantHelpers.EstadoActivo);

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


                                            foreach (var historia in lstHistoria)
                                            {
                                                //if (cuota.FechaEmision <= historia.Fecha)
                                                if (fechaComparar >= historia.Propietario.FechaCreacion.Value.Date &&
                                        (fechaComparar < objTitular2.FechaCreacion.Value.Date
                                        || (fechaComparar.Month == historia.Propietario.FechaCreacion.Value.Month
                                        && fechaComparar.Year == historia.Propietario.FechaCreacion.Value.Year)
                                        ))
                                                {
                                                    excelWorksheet.Cells["B" + i].Value = historia.Propietario.Nombres;
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    }
                                    TotalGeneral += Total;
                                    excelWorksheet.Cells[i, Col].Value = Total;
                                    excelWorksheet.Cells[i, Col].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                                    excelWorksheet.Cells[i, Col].Style.Font.Bold = true;
                                    i++;
                                    LstDepartamentoId.Add(item.DepartamentoId);
                                }

                            }
                            excelWorksheet.Cells[i, Col].Value = TotalGeneral;
                            excelWorksheet.Cells[i, Col].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                            excelWorksheet.Cells[i, Col].Style.Font.Bold = true;
                            //excelWorksheet.Cells["A" + i].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                            excelWorksheet.Cells["B" + i].Value = "Totales:";
                            excelWorksheet.Cells["B" + i].Style.Font.Bold = true;
                            excelWorksheet.Cells["B" + i].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                            excelWorksheet.Cells["C" + i].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                            foreach (var mes in LstMeses)
                            {
                                excelWorksheet.Cells[i, mes.Value].Value = LstCuotas.Where(x => x.UnidadTiempo.Orden == mes.Key).Sum(x => x.CuotaExtraordinaria);
                                excelWorksheet.Cells[i, mes.Value].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                                excelWorksheet.Cells[i, mes.Value].Style.Font.Bold = true;
                            }
                        }

                    }

                    MemoryStream outputMemoryStream = new MemoryStream(excelPackage.GetAsByteArray());
                    if (outputMemoryStream == null)
                    {
                        response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                    }

                    response.Content = new StreamContent(outputMemoryStream);

                    response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = "CuadroMoroso_" + Edificio + "_" + DateTime.Now.ToShortDateString() + ".xlsx"
                    };
                    response.Content.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");

                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

                    return response;
                }
            }
            catch (Exception ex)
            {
                response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                return response;
            }            
        }
        [Route("api/AfariService/GetReciboPorId")]
        public HttpResponseMessage GetReciboPorId(Int32 departamentoId, Int32 unidadTiempoId)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            var departamento = context.Departamento.FirstOrDefault(X => X.DepartamentoId == departamentoId);
            var cantDepartamento = context.Departamento.Count(x => x.EdificioId == departamento.EdificioId);
            var unidadTiempo = context.UnidadTiempo.FirstOrDefault(X => X.UnidadTiempoId == unidadTiempoId);
            if (departamento != null && unidadTiempo != null)
            {
                var correcion = context.ArchivoCorrecionEdificio.FirstOrDefault(X => X.Tipo.Contains(ConstantHelpers.TipoArchivo.Recibo + "/" + departamento.Numero) && X.EdificioId == departamento.EdificioId && X.UnidadTiempoId == unidadTiempoId);
                if (correcion != null)
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/Files/Corregidos"), correcion.RutaArchivo));
                    string fileName = "Estado de cuenta " + unidadTiempo.Descripcion + ".pdf";
                    response.Content = new StreamContent(new MemoryStream(fileBytes));
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

                    response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = fileName
                    };
                    response.Content.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");

                    return response;
                }
            }
            //Chequear>

            ReporteLogic reporteLogic = new ReporteLogic();
            reporteLogic.context = context;

            var cuotaDepa = context.Cuota.FirstOrDefault(X => X.DepartamentoId == departamentoId && X.UnidadTiempoId == unidadTiempoId);
            if (cuotaDepa == null)
            {
                response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                //PostMessage(MessageType.Error, "No se encontró recibo para este periodo.");
            }
            var presupuestoMes = context.Cuota.Where(X => X.Departamento.EdificioId == cuotaDepa.Departamento.EdificioId && X.UnidadTiempoId == unidadTiempoId).ToList().Sum(X => X.Total);
            long? NumeroRecibo = 0;
            var nroRecibo = departamento.UnidadTiempoReciboDepartamento.FirstOrDefault(x => x.UnidadTiempoId == unidadTiempoId
                && x.DepartamentoId == departamento.DepartamentoId);
            if (nroRecibo != null)
            {
                NumeroRecibo = nroRecibo.NumeroRecibo;
            }
            else
            {
                var ultimoNumeroRecibo = departamento.UnidadTiempoReciboDepartamento.Where(x => x.DepartamentoId == departamento.DepartamentoId).Max(x => x.NumeroRecibo);
                var ultimoRecibo = departamento.UnidadTiempoReciboDepartamento.FirstOrDefault(x => x.NumeroRecibo == ultimoNumeroRecibo);
                var diferenciaMes = ultimoRecibo.UnidadTiempo.Orden - unidadTiempo.Orden;

                if (unidadTiempo.Orden > ultimoRecibo.UnidadTiempo.Orden)
                {
                    NumeroRecibo = ultimoRecibo.NumeroRecibo + (cantDepartamento * Math.Abs(diferenciaMes.Value));

                    var utRecibo = new UnidadTiempoReciboDepartamento();
                    utRecibo.DepartamentoId = departamento.DepartamentoId;
                    utRecibo.UnidadTiempoId = unidadTiempo.UnidadTiempoId;
                    utRecibo.NumeroRecibo = NumeroRecibo.Value;
                    context.UnidadTiempoReciboDepartamento.Add(utRecibo);
                }
                else
                {
                    NumeroRecibo = ultimoRecibo.NumeroRecibo - (cantDepartamento * Math.Abs(diferenciaMes.Value));

                    var utRecibo = new UnidadTiempoReciboDepartamento();
                    utRecibo.DepartamentoId = departamento.DepartamentoId;
                    utRecibo.UnidadTiempoId = unidadTiempo.UnidadTiempoId;
                    utRecibo.NumeroRecibo = NumeroRecibo.Value;
                    context.UnidadTiempoReciboDepartamento.Add(utRecibo);
                }
            }

            var nombreArchivo = reporteLogic.GetReportAPI(cuotaDepa, cuotaDepa.FechaEmision ?? DateTime.Now, cuotaDepa.FechaVencimiento ?? DateTime.Now, presupuestoMes, cuotaDepa.Departamento.DepartamentoM2 ?? (decimal)0, context.UnidadTiempo.FirstOrDefault(X => X.UnidadTiempoId == unidadTiempoId), null, null, NumeroRecibo);


            MemoryStream outputMemoryStream = reporteLogic.getFirstReport();
            if (outputMemoryStream == null)
            {
                response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }

            response.Content = new StreamContent(outputMemoryStream);

            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = nombreArchivo
            };
            response.Content.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

            return response;
        }
        [Route("api/AfariService/GetPropietariosPorEdificio")]
        [HttpGet]
        public ResponseGetPropietarios GetPropietariosPorEdificio(Int32 edificioId)
        {
            ResponseGetPropietarios ResponseGetPropietarios = new ResponseGetPropietarios();

            try
            {
                try
                {
                    ResponseGetPropietarios.lstPropietario = context.Propietario.Where(x => x.Estado == ConstantHelpers.EstadoActivo
                    && x.Departamento.EdificioId == edificioId).OrderBy( x => x.DepartamentoId).Select(x => new PropietarioBE { nombreDepartamento =  x.Departamento.TipoInmueble.Nombre + " " + x.Departamento.Numero,
                        nombrePropietario = x.Nombres + " " + x.ApellidoPaterno + " " + x.ApellidoMaterno,
                        telefono = x.Telefono,
                    email = x.Email}).ToList();
                }
                catch (Exception ex)
                {
                    ResponseGetPropietarios.error = true;
                    ResponseGetPropietarios.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
                }
            }
            catch (Exception ex)
            {
                ResponseGetPropietarios.error = true;
                ResponseGetPropietarios.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
            }

            return ResponseGetPropietarios;
        }
        [Route("api/AfariService/GetRecibosMantenimiento")]
        [HttpGet]
        public ResponseGetRecibosMantenimiento GetRecibosMantenimiento(Int32 edificioId, Int32? departamentoId, Int32? unidadTiempoId)
        {
            ResponseGetRecibosMantenimiento ResponseGetRecibosMantenimiento = new ResponseGetRecibosMantenimiento();

            try
            {
                try
                {
                    var lstCuotas = context.Cuota.OrderByDescending(x => x.UnidadTiempo.Anio).OrderByDescending(x => x.UnidadTiempo.Mes).
                        Include(x => x.Departamento).
                        Include(x => x.UnidadTiempo).
                        Where(x => x.Departamento.EdificioId == edificioId).
                        AsQueryable();

                    if (departamentoId.HasValue)
                    {
                        lstCuotas = lstCuotas.Where( x => x.DepartamentoId == departamentoId);
                    }
                    if (unidadTiempoId.HasValue)
                    {
                        lstCuotas = lstCuotas.Where(x => x.UnidadTiempoId == unidadTiempoId);
                    }

                    ResponseGetRecibosMantenimiento.lstRecibosMantenimiento = lstCuotas.Select(x => new ReciboMantenimientoBE
                    {
                        departamentoId = x.DepartamentoId,
                        departamentoDescripcion = x.Departamento.TipoInmueble.Nombre + " " + x.Departamento.Numero,
                        unidadTiempoId = x.UnidadTiempoId,
                        unidadTiempoDescripcion = x.UnidadTiempo.Descripcion
                    }).ToList();

                    //ResponseGetRecibosMantenimiento.lstRecibosMantenimiento = context.rec.Where(x => x.Estado == ConstantHelpers.EstadoActivo
                    //&& x.EdificioId == edificioId).Select(x => new DepartamentoBE { departamentoId = x.DepartamentoId, numero = x.Numero, tipo = x.TipoInmueble.Nombre }).OrderBy(x => x.departamentoId).ToList();
                }
                catch (Exception ex)
                {
                    ResponseGetRecibosMantenimiento.error = true;
                    ResponseGetRecibosMantenimiento.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
                }
            }
            catch (Exception ex)
            {
                ResponseGetRecibosMantenimiento.error = true;
                ResponseGetRecibosMantenimiento.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
            }

            return ResponseGetRecibosMantenimiento;
        }
        [Route("api/AfariService/GetDepartamentos")]
        [HttpGet]
        public ResponseGetDepartamentos GetDepartamentos(Int32 edificioId)
        {
            ResponseGetDepartamentos ResponseGetDepartamentos = new ResponseGetDepartamentos();

            try
            {
                try
                {
                    ResponseGetDepartamentos.lstDepartamento = context.Departamento.Where(x => x.Estado == ConstantHelpers.EstadoActivo
                    && x.EdificioId == edificioId).Select(x => new DepartamentoBE { departamentoId = x.DepartamentoId, numero = x.Numero, tipo = x.TipoInmueble.Nombre }).OrderBy(x => x.departamentoId).ToList();
                }
                catch (Exception ex)
                {
                    ResponseGetDepartamentos.error = true;
                    ResponseGetDepartamentos.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
                }
            }
            catch (Exception ex)
            {
                ResponseGetDepartamentos.error = true;
                ResponseGetDepartamentos.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
            }

            return ResponseGetDepartamentos;
        }
        [Route("api/AfariService/GetCerrarCuotas")]
        [HttpGet]
        public ResponseGetCerrarCuotas GetCerrarCuotas(Int32 edificioId,Int32 unidadTiempoIdInicio, Int32 unidadTiempoIdFin, Int32? departamentoId, String estado)
        {
            ResponseGetCerrarCuotas ResponseGetCerrarCuotas = new ResponseGetCerrarCuotas();

            try
            {
                try
                {
                    var edificio = context.Edificio.FirstOrDefault(x => x.EdificioId == edificioId);
                    ResponseGetCerrarCuotas.mora = edificio.PMora;
                    ResponseGetCerrarCuotas.tipoMora = edificio.TipoMora;
                    ResponseGetCerrarCuotas.diaMesConsiderar = (edificio.DiaMora ?? 15).ToString() + " / Mes";

                    if (departamentoId.HasValue)
                    {
                        var query = context.Cuota.Where(x => x.DepartamentoId == departamentoId && x.UnidadTiempo.Estado == ConstantHelpers.EstadoActivo && x.UnidadTiempoId <= unidadTiempoIdFin
                        && x.UnidadTiempoId >= unidadTiempoIdInicio).AsQueryable();
                        if (!String.IsNullOrEmpty(estado))
                        {
                            var e = estado == "0" ? false : true;
                            query = query.Where(x => x.Pagado == e);
                        }
                        ResponseGetCerrarCuotas.lstCuota = query.OrderBy(x => x.DepartamentoId).ThenByDescending(x => x.UnidadTiempo.Orden)
                            .Select(x => new CuotaBE { cuotaId = x.CuotaId, estado = x.Estado, unidadTiempoId = x.UnidadTiempoId,
                                unidadTiempoDescripcion = x.UnidadTiempo.Descripcion,
                                esPagoAdelantado = x.EsAdelantado,
                                total = x.Total,
                                totalConMora = x.Total + x.Mora,
                                departamentoId = x.DepartamentoId,
                                departamentoDescripcion = x.Departamento.TipoInmueble.Nombre + " " +x.Departamento.Numero
                            }).ToList();
                    }
                    else
                    {
                        var query = context.Cuota.Where(x => x.UnidadTiempoId <= unidadTiempoIdFin && x.UnidadTiempoId >= unidadTiempoIdInicio && x.UnidadTiempo.Estado == ConstantHelpers.EstadoActivo && x.Departamento.EdificioId == edificioId).AsQueryable();
                        if (!String.IsNullOrEmpty(estado))
                        {
                            var e = estado == "0" ? false : true;
                            query = query.Where(x => x.Pagado == e);
                        }

                        ResponseGetCerrarCuotas.lstCuota = query.OrderBy(x => x.DepartamentoId).ThenByDescending(x => x.UnidadTiempo.Orden)
                            .Select(x => new CuotaBE
                            {
                                cuotaId = x.CuotaId,
                                estado = x.Estado,
                                unidadTiempoId = x.UnidadTiempoId,
                                unidadTiempoDescripcion = x.UnidadTiempo.Descripcion,
                                esPagoAdelantado = x.EsAdelantado,
                                total = x.Total,
                                totalConMora = x.Total + x.Mora,
                                departamentoId = x.DepartamentoId,
                                departamentoDescripcion = x.Departamento.TipoInmueble.Nombre + " " + x.Departamento.Numero
                            }).ToList();
                    }
                }
                catch (Exception ex)
                {
                    ResponseGetCerrarCuotas.error = true;
                    ResponseGetCerrarCuotas.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
                }
            }
            catch (Exception ex)
            {
                ResponseGetCerrarCuotas.error = true;
                ResponseGetCerrarCuotas.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
            }

            return ResponseGetCerrarCuotas;
        }
        [Route("api/AfariService/GetUnidadTiempo")]
        [HttpGet]
        public ResponseGetUnidadTiempo GetUnidadTiempo(bool FlagEsActivo)
        {
            ResponseGetUnidadTiempo ResponseGetEdificios = new ResponseGetUnidadTiempo();

            try
            {
                try
                {
                    var query  = context.UnidadTiempo.Where(x => x.Estado == ConstantHelpers.EstadoActivo).Select(x => new UnidadTiempoBE { unidadTiempoId = x.UnidadTiempoId, nombre = x.Descripcion, orden = x.Orden }).OrderBy(x => x.orden).AsQueryable();
                    if (FlagEsActivo)
                    {
                        var unidadTiempoActual = context.UnidadTiempo.FirstOrDefault(x => x.Estado == ConstantHelpers.EstadoActivo
                    && x.EsActivo == true).UnidadTiempoId;

                        query = query.Where(x => x.unidadTiempoId <= unidadTiempoActual);
                    }
                    ResponseGetEdificios.lstUnidadTiempo = query.ToList();
                }
                catch (Exception ex)
                {
                    ResponseGetEdificios.error = true;
                    ResponseGetEdificios.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
                }
            }
            catch (Exception ex)
            {
                ResponseGetEdificios.error = true;
                ResponseGetEdificios.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
            }

            return ResponseGetEdificios;
        }
        [Route("api/AfariService/GetUnidadTiempoActual")]
        [HttpGet]
        public ResponseGetUnidadTiempoActual GetUnidadTiempoActual()
        {
            ResponseGetUnidadTiempoActual ResponseGetUnidadTiempoActual = new ResponseGetUnidadTiempoActual();

            try
            {
                try
                {
                    ResponseGetUnidadTiempoActual.unidadTiempoActual = context.UnidadTiempo.Where(x => x.Estado == ConstantHelpers.EstadoActivo
                    && x.EsActivo == true).Select(x => new UnidadTiempoBE { unidadTiempoId = x.UnidadTiempoId, nombre = x.Descripcion, orden = x.Orden }).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    ResponseGetUnidadTiempoActual.error = true;
                    ResponseGetUnidadTiempoActual.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
                }
            }
            catch (Exception ex)
            {
                ResponseGetUnidadTiempoActual.error = true;
                ResponseGetUnidadTiempoActual.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
            }

            return ResponseGetUnidadTiempoActual;
        }
        [Route("api/AfariService/GetEstadoEdificio")]
        [HttpGet]
        public ResponseGetEstadoEdificio GetEstadoEdificio(Int32 edificioId, Int32? unidadTiempoId)
        {
            ResponseGetEstadoEdificio ResponseGetEdificios = new ResponseGetEstadoEdificio();

            try
            {
                try
                {
                    if (!unidadTiempoId.HasValue)
                    {
                        unidadTiempoId = context.UnidadTiempo.First(X => X.Estado == ConstantHelpers.EstadoActivo && X.EsActivo == true).UnidadTiempoId;
                    }

                    var edificio = context.Edificio.FirstOrDefault(x => x.EdificioId == edificioId);
                    if (edificio != null)
                    {
                        Int32 OrdenCalculo = context.Cuota.Where(X => X.Departamento.EdificioId == edificioId && X.Pagado).Select(X => X.UnidadTiempo).ToList().Max(X => X.Orden) ?? -1;

                        if (OrdenCalculo == -1)
                        {
                            OrdenCalculo = context.UnidadTiempo.First(X => X.Estado == ConstantHelpers.EstadoActivo).Orden.Value;
                        }

                        var UnidadTiempo = context.UnidadTiempo.FirstOrDefault(x => x.UnidadTiempoId == unidadTiempoId);

                        List<Cuota> ListCuotas = new List<Cuota>();
                        var cuotas = context.Cuota.Where(X => X.Departamento.EdificioId == edificio.EdificioId && X.Pagado);


                        Decimal TotalSaldoAnterior = 0;
                        Int32 Orden = UnidadTiempo.Orden ?? 0;
                        TotalSaldoAnterior += context.DetalleGasto.Where(X => X.Gasto.EdificioId == edificioId && X.Gasto.UnidadTiempo.Orden < Orden && X.Pagado && X.Estado == ConstantHelpers.EstadoActivo).ToList().Sum(X => X.Monto);
                        TotalSaldoAnterior += context.DetalleIngreso.Where(X => X.Ingreso.EdificioId == edificioId && X.Ingreso.UnidadTiempo.Orden < Orden && X.Pagado && X.Estado == ConstantHelpers.EstadoActivo).ToList().Sum(X => X.Monto);
                        TotalSaldoAnterior += context.Cuota.Where(X => X.Departamento.EdificioId == edificioId && X.UnidadTiempo.Orden < Orden && X.Pagado && X.Estado == ConstantHelpers.EstadoActivo).ToList().Sum(X => X.Total);



                        Decimal TotalPagosCuotasMes = 0;//ListCuotas.Sum(X => X.Total + X.Mora);
                        Decimal TotalPagosCuotasAnterior = context.Cuota.Where(X => X.Pagado && X.Departamento.EdificioId == edificioId && X.Estado == ConstantHelpers.EstadoActivo).ToList().Sum(X => X.Total + X.Mora) - TotalPagosCuotasMes;
                        Decimal TotalIngresosAdicionales = context.Ingreso.Where(X => X.EdificioId == edificioId && X.Estado == ConstantHelpers.EstadoActivo).ToList().Sum(X => X.DetalleIngreso.ToList().Sum(Y => Y.Monto));
                        Decimal TotalGasto = context.Gasto.Where(X => X.EdificioId == edificioId && X.Estado == ConstantHelpers.EstadoActivo).ToList().Sum(X => X.DetalleGasto.ToList().Sum(Y => Y.Monto));

                        var LstUnidadTiempo = context.UnidadTiempo.Where(x => x.Estado == ConstantHelpers.EstadoActivo && x.Orden <= UnidadTiempo.Orden).OrderBy(x => x.Orden).ToList();

                        Decimal SaldoAnterior = 0;
                        Decimal SaldoAcumulado = 0;
                        foreach (var item in LstUnidadTiempo)
                        {
                            ListCuotas = new List<Cuota>();
                            foreach (var cuota in cuotas)
                            {
                                //Si no existe la fecha de pagado, añadir si cumple con la unidad de tiempo
                                if (!cuota.FechaPagado.HasValue && cuota.UnidadTiempoId == item.UnidadTiempoId)
                                    ListCuotas.Add(cuota);
                                else
                                    //Si existe la fecha de pagado, comprar el mes y el año , si encajan con esta unidad de tiempo, entonces son parte del reporte
                                    if (cuota.FechaPagado.HasValue && (cuota.FechaPagado.Value.Month == item.Mes && cuota.FechaPagado.Value.Year == item.Anio))
                                {
                                    ListCuotas.Add(cuota);
                                }
                            }
                            TotalPagosCuotasMes = ListCuotas.Sum(X => X.Total + X.Mora);
                            SaldoAnterior = SaldoAcumulado;

                            if (item.UnidadTiempoId == edificio.SaldoAnteriorUnidadTiempo)
                            {
                                SaldoAnterior += edificio.SaldoAnteriorHistorico ?? 0;
                            }


                            var GastoTemp = context.Gasto.Where(X => X.UnidadTiempoId == item.UnidadTiempoId && edificioId == X.EdificioId && X.Estado == ConstantHelpers.EstadoActivo).ToList().Sum(X => X.DetalleGasto.Where(Y => Y.Pagado == true).ToList().Sum(Y => Y.Monto));
                            var IngresoTemp = TotalPagosCuotasMes + context.Ingreso.Where(X => X.UnidadTiempoId == item.UnidadTiempoId && edificioId == X.EdificioId && X.Estado == ConstantHelpers.EstadoActivo).ToList().Sum(X => X.DetalleIngreso.ToList().Sum(Y => Y.Monto));
                            var SaldoTemp = IngresoTemp - GastoTemp;
                            SaldoAcumulado = SaldoAnterior + SaldoTemp;
                        }

                        var GastosActual = context.Gasto.Where(X => X.UnidadTiempoId == unidadTiempoId && edificioId == X.EdificioId && X.Estado == ConstantHelpers.EstadoActivo).ToList().Sum(X => X.DetalleGasto.Where(Y => Y.Pagado == true).ToList().Sum(Y => Y.Monto));
                        var IngresosActual = context.Ingreso.Where(X => X.UnidadTiempoId == unidadTiempoId && edificioId == X.EdificioId && X.Estado == ConstantHelpers.EstadoActivo).ToList().Sum(X => X.DetalleIngreso.ToList().Sum(Y => Y.Monto));
                        var Acumulado = TotalPagosCuotasMes + TotalPagosCuotasAnterior + TotalIngresosAdicionales - TotalGasto;
                        IngresosActual += TotalPagosCuotasMes;

                        var SaldoActual = IngresosActual - GastosActual;
                        var AcumuladoActual = Acumulado - SaldoActual;

                        ResponseGetEdificios.saldoAnterior = SaldoAnterior;
                        ResponseGetEdificios.saldo = SaldoActual;
                        ResponseGetEdificios.ingresos = IngresosActual;
                        ResponseGetEdificios.saldoAcumulado = SaldoAcumulado;
                        ResponseGetEdificios.gastos = GastosActual;
                    }
                    else
                    {
                        ResponseGetEdificios.error = true;
                        ResponseGetEdificios.mensaje = "Edificio no existe.";
                    }
                }
                catch (Exception ex)
                {
                    ResponseGetEdificios.error = true;
                    ResponseGetEdificios.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
                }
            }
            catch (Exception ex)
            {
                ResponseGetEdificios.error = true;
                ResponseGetEdificios.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
            }

            return ResponseGetEdificios;
        }
        [Route("api/AfariService/GetEdificios")]
        [HttpGet]
        public ResponseGetEdificios GetEdificios(Int32 usuarioId)
        {
            ResponseGetEdificios ResponseGetEdificios = new ResponseGetEdificios();

            try
            {
                try
                {
                    var usuario = context.Usuario.FirstOrDefault( x => x.UsuarioId == usuarioId);
                    if (usuario != null)
                    {
                        var query = context.Edificio.Where(x => x.Estado != ConstantHelpers.EstadoEliminado)
                       .OrderBy(x => x.Estado).ThenBy(x => x.Orden).AsQueryable();

                        if (usuario != null && usuario.EsAdmin == false)
                        {
                            var lstPermiso = context.PermisoEdificio.Where(x => x.UsuarioId == usuarioId).Select(x => x.EdificioId).ToList();
                            query = query.Where(x => lstPermiso.Contains(x.EdificioId));
                        }

                        ResponseGetEdificios.lstEdificios = query.Select( x => new EdificioBE { edificioId = x.EdificioId, nombre = x.Nombre}).OrderByDescending( x => x.nombre).ToList();
                    }
                    else
                    {
                        ResponseGetEdificios.error = true;
                        ResponseGetEdificios.mensaje = "Usuario no existe.";
                    }
                }
                catch (Exception ex)
                {
                    ResponseGetEdificios.error = true;
                    ResponseGetEdificios.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
                }
            }
            catch (Exception ex)
            {
                ResponseGetEdificios.error = true;
                ResponseGetEdificios.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
            }

            return ResponseGetEdificios;
        }
        [Route("api/AfariService/Login")]
        [HttpPost]
        public ResponseLogin Login(RequestLogin data)
        {
            ResponseLogin ResponseLogin = new ResponseLogin();

            try
            {
                try
                {
                    var usuario = context.Usuario.Include(x => x.Departamento).Include(x => x.Departamento.Edificio).FirstOrDefault(x => x.Codigo == data.usuario && x.Password == data.password);

                    if (usuario == null)
                    {
                        ResponseLogin.error = true;
                        ResponseLogin.mensaje = "Usuario y/o Contraseña Incorrectos";
                    }
                    else
                    {

                        if (usuario.Estado.Equals(ConstantHelpers.EstadoInactivo))
                        {
                            ResponseLogin.error = true;
                            ResponseLogin.mensaje = "Su cuenta no se encuentra habilitada. Consulte con su administrador";

                        }
                        if (usuario.Estado == "TEM")
                        {
                            ResponseLogin.error = true;
                            ResponseLogin.mensaje = "Debe cambiar su contraseña en el portal web";
                        }
                        if (usuario.Estado.Equals(ConstantHelpers.EstadoActivo))
                        {
                            ResponseLogin.error = false;
                            switch (usuario.Rol)
                            {
                                case ConstantHelpers.ROL_PROPIETARIO: ResponseLogin.rol = "PRO"; break;
                                case ConstantHelpers.ROL_ADMINISTRADOR: ResponseLogin.rol = "ADM"; break;
                            }

                            ResponseLogin.nombre = usuario.Nombres + " " + usuario.Apellidos;
                            ResponseLogin.correo = usuario.Email;
                            ResponseLogin.usuarioId = usuario.UsuarioId;

                            if (usuario.Rol.ToLower().Equals(ConstantHelpers.ROL_PROPIETARIO.ToLower()))
                            {
                                if (usuario.Departamento.Estado.Equals(ConstantHelpers.EstadoInactivo) || usuario.Departamento.Edificio.Estado.Equals(ConstantHelpers.EstadoInactivo))
                                {
                                    ResponseLogin.error = true;
                                    ResponseLogin.mensaje = "Su cuenta no se encuentra habilitada. Consulte con su administrador";
                                }

                                ResponseLogin.departamentoId = usuario.DepartamentoId;
                                ResponseLogin.edificioId = usuario.Departamento.EdificioId;

                                try
                                {
                                    var visita = new Visita();
                                    visita.Fecha = DateTime.Now;
                                    visita.DepartamentoId = usuario.DepartamentoId.Value;
                                    visita.Tipo = "APP";
                                    visita.EdificioId = usuario.Departamento.EdificioId;
                                    visita.UsuarioId = usuario.UsuarioId;
                                    context.Visita.Add(visita);
                                    context.SaveChanges();
                                }
                                catch (Exception ex)
                                {
                                    ResponseLogin.error = true;
                                    ResponseLogin.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
                                }

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ResponseLogin.error = true;
                    ResponseLogin.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
                }
            }
            catch (Exception ex)
            {
                ResponseLogin.error = true;
                ResponseLogin.mensaje = ex.Message + (ex.InnerException != null ? ex.InnerException.Message : String.Empty);
            }

            return ResponseLogin;
        }
    }
}
