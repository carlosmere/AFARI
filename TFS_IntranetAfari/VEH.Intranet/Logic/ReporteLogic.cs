using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.DataSets;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using System.Data.Entity;
using System.Web.Hosting;
using Xceed.Words.NET;
using System.Drawing;

namespace VEH.Intranet.Logic
{
    public class ReporteLogic
    {
        public HttpServerUtilityBase Server { get; set; }
        public SIVEHEntities context { get; set; }
        public List<MemoryStream> lstMemoryStream;
        public List<byte[]> lstMemoryStreamPDF;
        private byte[] ExcelArchivo;
        public List<String> lstNombreDOC;
        private List<String> lstNombrePDF;
        private PdfReader pdfFile;
        private Document doc;
        private PdfWriter pCopy;
        private ReportViewer rv;
        public Int32 CantidadReporte { get; set; } = 0;
        private String[] Meses = { "", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };
        public Int32 UnidadTiempoActualId { get; set; } = 1;
        public ReporteLogic()
        {
            lstMemoryStream = new List<MemoryStream>();
            lstMemoryStreamPDF = new List<byte[]>();
            lstNombreDOC = new List<String>();
            lstNombrePDF = new List<String>();
            rv = new ReportViewer();
        }

        private String dateTimeToString(DateTime date)
        {
            String year = date.Year.ToString(), month = "00" + date.Month.ToString(), day = "00" + date.Day.ToString();
            month = month.Substring(month.Length - 2, 2);
            day = day.Substring(day.Length - 2, 2);
            return year + month + day;
        }

        public MemoryStream ExportToBankFormat(List<Cuota> cuotas)
        {
            try
            {
                if (cuotas.Count < 1) throw new Exception();
                Edificio edificio = cuotas[0].Departamento.Edificio;
                //UnidadTiempo unidadTiempo = cuotas[0].UnidadTiempo;
                MemoryStream ms = new MemoryStream();
                ms.Position = 0;
                StreamWriter sw = new StreamWriter(ms);
                String cabecera = makeCabecera((int)cuotas.Sum(x => ((Int32)(x.Total * 100))), cuotas.Count, edificio);
                sw.WriteLine(cabecera);
                string temp = edificio.NroCuenta.Substring(0, 11).ToString();
                temp = temp.Replace('-', '0');
                foreach (var cuota in cuotas)
                {
                    String line = "DD" + temp;
                    line += fixString(cuota.Departamento.Numero, 14, "0", false);
                    Propietario propietario = ConstantHelpers.getTitularDepartamento(cuota.Departamento) ?? new Propietario();
                    String nombreCompleto = propietario.Nombres + " " + propietario.ApellidoPaterno + " " + propietario.ApellidoMaterno;
                    nombreCompleto = nombreCompleto.ToUpper();
                    nombreCompleto = nombreCompleto.Replace("Ñ", "N");
                    nombreCompleto = RemoveDiacritics(nombreCompleto);
                    line += fixString(nombreCompleto, 40, " ", true);

                    line += fixString(cuota.Departamento.Numero + " " + cuota.UnidadTiempo.Descripcion.Substring(0, 3), 30, " ", true);
                    line += dateTimeToString(cuota.FechaEmision.Value) + dateTimeToString(cuota.FechaVencimiento.Value);// "2015033020150415";
                    line += fixString(((int)(cuota.Total * 100)).ToString(), 15, "0", false);
                    line += fixString("0", 71, "0", true);
                    line += "A";
                    line = line.ToUpper();
                    sw.WriteLine(line);
                }

                sw.Flush();
                ms.Position = 0;
                return ms;
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        private String makeCabecera(Int32 MontoTotal, Int32 NroRegistros, Edificio edificio)
        {

            string temp = edificio.NroCuenta.Substring(0, 11).ToString();
            temp = temp.Replace('-', '0');
            String header = "CC" + temp + "C";
            header += fixString("EDIFICIO " + edificio.Nombre.ToUpper(), 40, " ", true);
            header += "20121204";
            header += fixString(NroRegistros.ToString(), 9, "0", false);
            header += fixString(MontoTotal.ToString(), 15, "0", false);
            header += "A";
            //header += fixString("10", 114, " ", false);
            return header;
        }

        private String fixString(String cad, Int32 MaxLength, String pad, bool startWithIt)
        {
            String res = "";
            if (startWithIt) res += cad;
            for (int i = 0; i < MaxLength; ++i) res += pad;
            if (!startWithIt) res += cad;
            return (startWithIt) ? res.Substring(0, MaxLength) : res.Substring(res.Length - MaxLength, MaxLength);
        }
        //RENZO
        public MemoryStream GetReportInspecciones(String Titulo, Dictionary<int, Dictionary<DateTime, int>> LstHistorialPreguntas, Int32 EdificioId)
        {
            // byte[] bytes = (byte)0;
            try
            {

                rv.Clear();
                rv.LocalReport.DataSources.Clear();
                DSInfoInspecciones ds = new DSInfoInspecciones();

                DataRow titulo = ds.Tables["DTInfo"].NewRow();
                titulo["Titulo"] = Titulo;
                ds.Tables["DTInfo"].Rows.Add(titulo);

                ReportDataSource rdsInfo = new ReportDataSource("DSInfo", ds.Tables["DTInfo"].DefaultView);
                rv.ProcessingMode = ProcessingMode.Local;
                rv.LocalReport.EnableExternalImages = true;
                rv.LocalReport.ReportEmbeddedResource = "VEH.Intranet.Report.ReporteInspecciones.rdlc";
                rv.LocalReport.DataSources.Add(rdsInfo);

                //Preguntas
                foreach (var pregunta in LstHistorialPreguntas[1])
                {
                    DataRow rowPreguntaUnitario = ds.Tables["DTInspeccionUnitario1"].NewRow();
                    rowPreguntaUnitario["Fecha"] = pregunta.Key.ToShortDateString();
                    rowPreguntaUnitario["Valor"] = pregunta.Value;
                    rowPreguntaUnitario["NumeroPregunta"] = "Estado del Jardín";
                    ds.Tables["DTInspeccionUnitario1"].Rows.Add(rowPreguntaUnitario);
                }
                //ReportDataSource rdsPregunta1 = new ReportDataSource("DSInspeccionUnitario1", ds.Tables["DTInspeccionUnitario1"].DefaultView);
                //rv.LocalReport.DataSources.Add(rdsPregunta1);

                for (int i = 1; i <= 36; i++)
                {
                    foreach (var pregunta in LstHistorialPreguntas[i])
                    {
                        DataRow rowPreguntaUnitario = ds.Tables["DTInspeccionUnitario" + i].NewRow();
                        rowPreguntaUnitario["Fecha"] = pregunta.Key.ToShortDateString();
                        rowPreguntaUnitario["Valor"] = pregunta.Value;
                        switch (i)
                        {
                            case 2: rowPreguntaUnitario["NumeroPregunta"] = "Estado pintado fachada"; break;
                            case 3: rowPreguntaUnitario["NumeroPregunta"] = "Vestimenta de trabajadores"; break;
                            case 4: rowPreguntaUnitario["NumeroPregunta"] = "Horario de trabajadores"; break;
                            case 5: rowPreguntaUnitario["NumeroPregunta"] = "Limpieza de Lobby"; break;
                            case 6: rowPreguntaUnitario["NumeroPregunta"] = "Estado pintado de Lobby"; break;
                            case 7: rowPreguntaUnitario["NumeroPregunta"] = "Limpieza de ascensores"; break;
                            case 8: rowPreguntaUnitario["NumeroPregunta"] = "Estado de ascensores"; break;
                            case 9: rowPreguntaUnitario["NumeroPregunta"] = "Motor puertas garaje"; break;
                            case 10: rowPreguntaUnitario["NumeroPregunta"] = "Resortes garaje"; break;
                            case 11: rowPreguntaUnitario["NumeroPregunta"] = "Limpieza de puertas garaje"; break;
                            case 12: rowPreguntaUnitario["NumeroPregunta"] = "Limpieza garaje"; break;
                            case 13: rowPreguntaUnitario["NumeroPregunta"] = "Estado pintado garajes"; break;
                            case 14: rowPreguntaUnitario["NumeroPregunta"] = "Limpieza paredes"; break;
                            case 15: rowPreguntaUnitario["NumeroPregunta"] = "Limpieza pisos"; break;
                            case 16: rowPreguntaUnitario["NumeroPregunta"] = "Estado pintado pisos"; break;
                            case 17: rowPreguntaUnitario["NumeroPregunta"] = "Limpieza baño invitados"; break;
                            case 18: rowPreguntaUnitario["NumeroPregunta"] = "Estado cuarto de servicios"; break;
                            case 19: rowPreguntaUnitario["NumeroPregunta"] = "Limpieza baño trabajadores"; break;
                            case 20: rowPreguntaUnitario["NumeroPregunta"] = "Limpieza baños area social"; break;
                            case 21: rowPreguntaUnitario["NumeroPregunta"] = "Limpieza area social"; break;
                            case 22: rowPreguntaUnitario["NumeroPregunta"] = "Limpieza Salon de usos multiples"; break;
                            case 23: rowPreguntaUnitario["NumeroPregunta"] = "Estado parrillas"; break;
                            case 24: rowPreguntaUnitario["NumeroPregunta"] = "Limpieza escaleras"; break;
                            case 25: rowPreguntaUnitario["NumeroPregunta"] = "Limpieza barandas escalera"; break;
                            case 26: rowPreguntaUnitario["NumeroPregunta"] = "Limpieza lunas"; break;
                            case 27: rowPreguntaUnitario["NumeroPregunta"] = "Limpieza piscina"; break;
                            case 28: rowPreguntaUnitario["NumeroPregunta"] = "Bomba piscina"; break;
                            case 29: rowPreguntaUnitario["NumeroPregunta"] = "Limpieza de gimnasio"; break;
                            case 30: rowPreguntaUnitario["NumeroPregunta"] = "Limpieza maquinas del gimnasio"; break;
                            case 31: rowPreguntaUnitario["NumeroPregunta"] = "Cuarto de bombas"; break;
                            case 32: rowPreguntaUnitario["NumeroPregunta"] = "Cuarto de maquinas"; break;
                            case 33: rowPreguntaUnitario["NumeroPregunta"] = "Extractores de monoxido"; break;
                            case 34: rowPreguntaUnitario["NumeroPregunta"] = "Camaras"; break;
                            case 35: rowPreguntaUnitario["NumeroPregunta"] = "Estado Sauna"; break;
                            case 36: rowPreguntaUnitario["NumeroPregunta"] = "Estado pintado areas comunces"; break;

                        }

                        ds.Tables["DTInspeccionUnitario" + i].Rows.Add(rowPreguntaUnitario);
                    }
                    ReportDataSource rdsPregunta = new ReportDataSource("DSInspeccionUnitario" + i, ds.Tables["DTInspeccionUnitario" + i].DefaultView);
                    rv.LocalReport.DataSources.Add(rdsPregunta);
                }


                Warning[] warnings;
                string[] streamids;
                string mimeType;
                string encoding;
                string filenameExtension;

                byte[] bytes = rv.LocalReport.Render(
                    "PDF", null, out mimeType, out encoding, out filenameExtension,
                    out streamids, out warnings);

                Warning[] warnings_excel;
                string[] streamids_excel;
                string mimeType_excel;
                string encoding_excel;
                string filenameExtension_excel;

                byte[] bytes_excel = rv.LocalReport.Render(
                    "Excel", null, out mimeType_excel, out encoding_excel, out filenameExtension_excel,
                    out streamids_excel, out warnings_excel);


                String fileName = Server.MapPath("~/Resources") + "\\EstadísticaInspecciones.zip";
                MemoryStream outputMemStream = new MemoryStream();
                ZipOutputStream zipStream = new ZipOutputStream(outputMemStream);

                zipStream.SetLevel(3); //0-9, 9 being the highest level of compression

                ZipEntry entry_pdf = new ZipEntry("EstadísticaInspecciones.pdf");
                entry_pdf.DateTime = DateTime.Now;
                zipStream.PutNextEntry(entry_pdf);
                StreamUtils.Copy(new MemoryStream(bytes), zipStream, new byte[4096]);
                zipStream.CloseEntry();

                ZipEntry entry_excel = new ZipEntry("EstadísticaInspecciones.xls");
                entry_excel.DateTime = DateTime.Now;
                zipStream.PutNextEntry(entry_excel);
                StreamUtils.Copy(new MemoryStream(bytes_excel), zipStream, new byte[4096]);
                zipStream.CloseEntry();

                zipStream.IsStreamOwner = false;
                zipStream.Close();

                outputMemStream.Position = 0;

                return outputMemStream;
            }
            catch (Exception ex)
            {
                throw;
            }
            //  return new MemoryStream(null);
        }
        public static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
        public MemoryStream GetReportIngresosGastos(String Titulo, List<DetalleGasto> lstGastos, List<DetalleIngreso> lstIngresosComunes, List<Cuota> lstIngresos, Decimal SaldoAnterior, Int32 EdificioId, Int32 UnidadTiempo, bool exportadoAntes, DateTime fechaRegistro, List<Leyenda> LstLeyendas, bool EsAdministrador, List<Int32> LstAdelantado)
        {
            // byte[] bytes = (byte)0;
            try
            {
                UnidadTiempo unidadTiempoActual = context.UnidadTiempo.FirstOrDefault(x => x.UnidadTiempoId == UnidadTiempo);
                Decimal TotalIngresosTotal = 0M;
                Decimal TotalIngresosMora = 0M;
                Decimal TotalIngresosCuota = 0M;
                Decimal TotalGastos = 0M;
                Decimal TotalExtraordinarias = 0M;

                rv.Clear();
                rv.LocalReport.DataSources.Clear();
                DSInfoIngresosGastos ds = new DSInfoIngresosGastos();


                List<Departamento> LstDepartamentos = new List<Departamento>();
                //LstDepartamentos = context.Departamento.Where(x => x.EdificioId == EdificioId && x.Estado.Equals(ConstantHelpers.EstadoActivo)).ToList();
                LstDepartamentos = context.Departamento.Where(x => x.EdificioId == EdificioId).ToList();
                List<DateTime> LstFechasEmision = new List<DateTime>();
                Edificio edificio = context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);



                UnidadTiempo unidadTiempoAnterior = unidadTiempoActual;
                List<Cuota> ListIngresosTemp = lstIngresos;
                List<Decimal> LstMontoTotalDepa = new List<decimal>();
                List<Boolean> LstEncontrado = new List<bool>();
                lstIngresos.ForEach(x => LstMontoTotalDepa.Add(0M));
                lstIngresos.ForEach(x => LstFechasEmision.Add(DateTime.MinValue));
                lstIngresos.ForEach(x => LstEncontrado.Add(false));
                for (int i = 0; i < lstIngresos.Count; i++)
                    if (lstIngresos[i].Pagado)
                    {
                        DateTime fechaEmision = DateTime.Now;
                        DateTime.TryParseExact(unidadTiempoActual.Anio.ToString() + unidadTiempoActual.Mes.ToString()
                            + edificio.DiaEmisionCuota.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out fechaEmision);

                        //LstFechasEmision.Add(new DateTime(unidadTiempoActual.Anio, unidadTiempoActual.Mes, edificio.DiaEmisionCuota));
                        LstFechasEmision.Add(fechaEmision);
                        LstMontoTotalDepa[i] += ListIngresosTemp[i].Total;
                        //ListIngresosTemp[i].Estado = ConstantHelpers.EstadoCerrado;
                    }
                //else
                for (int i = 0; i < LstDepartamentos.Count; i++)
                {
                    // int DiasTranscurridosMora = LstFechasEmision[i].Equals(DateTime.MinValue) ? 0 : (DateTime.Now.Date - LstFechasEmision[i].Date).Days - 1; ;

                    int DiasTranscurridosMora = 0;
                    if (LstFechasEmision.Count > i)
                        DiasTranscurridosMora = LstFechasEmision[i].Equals(DateTime.MinValue) ? 0 : (fechaRegistro.Date - LstFechasEmision[i].Date).Days - 1; ;
                    Decimal moraUnitaria = edificio.TipoMora.Equals(ConstantHelpers.TipoMoraPorcentual) ? edificio.MontoCuota * edificio.PMora.Value / 100M : edificio.PMora.Value;
                    LstDepartamentos[i].MontoMora = (DiasTranscurridosMora <= 0 || i >= LstMontoTotalDepa.Count || LstMontoTotalDepa[i] == 0M) ? 0M : moraUnitaria * DiasTranscurridosMora;
                }
                Int32 Numeracion = 1;
                var lstGastosCorrientes = lstGastos.Where(X => X.Ordinario && X.Pagado).ToList();

                DataRow rowDepartamentoTotal = ds.Tables["DSGastos"].NewRow();
                rowDepartamentoTotal["Concepto"] = "TOTAL";
                rowDepartamentoTotal["EsTitulo"] = "1";
                rowDepartamentoTotal["Valor"] = lstGastos.Sum(X => X.Pagado ? X.Monto : 0).ToString("#,##0.00");
                ds.Tables["DSGastos"].Rows.Add(rowDepartamentoTotal);

                DataRow rowTitulo = ds.Tables["DSGastos"].NewRow();
                rowTitulo["Concepto"] = "       ";
                rowTitulo["EsTitulo"] = "1";
                rowTitulo["Valor"] = "========";
                ds.Tables["DSGastos"].Rows.Add(rowTitulo);

                rowTitulo = ds.Tables["DSGastos"].NewRow();
                rowTitulo["Concepto"] = "Gastos Corrientes";
                rowTitulo["EsTitulo"] = "1";
                rowTitulo["Valor"] = lstGastosCorrientes.Sum(X => X.Monto).ToString("#,##0.00");

                ds.Tables["DSGastos"].Rows.Add(rowTitulo);

                //rowTitulo = ds.Tables["DSGastos"].NewRow();
                //rowTitulo["Concepto"] = "       ";
                //rowTitulo["EsTitulo"] = "1";
                //rowTitulo["Valor"] = "_________";
                //ds.Tables["DSGastos"].Rows.Add(rowTitulo);

                foreach (var gasto in lstGastosCorrientes)
                {

                    DataRow rowGasto = ds.Tables["DSGastos"].NewRow();
                    var desfase = String.Empty;
                    var cantLinea = (Numeracion.ToString() + ". ").Length;
                    desfase = "\n " + ("").PadLeft(cantLinea, ' ');
                    var concepto = gasto.Concepto.Replace("\n", desfase);//String.Empty;

                    rowGasto["Concepto"] = Numeracion.ToString() + ". " + concepto;
                    rowGasto["Valor"] = gasto.Monto.ToString("#,##0.00");
                    rowGasto["Detalle"] = "";
                    ds.Tables["DSGastos"].Rows.Add(rowGasto);
                    TotalGastos += gasto.Monto;
                    Int32 Subnum = 1;
                    if (!String.IsNullOrWhiteSpace(gasto.Detalle))
                    {
                        var subDetallesGastos = gasto.Detalle.Split('|').Where(X => !String.IsNullOrWhiteSpace(X)).Select(X => new Tuple<String, String>(X.Split(';').First(), X.Split(';').Last())).ToList();
                        foreach (var subGasto in subDetallesGastos)
                        {
                            var desfaseSub = String.Empty;
                            var cantLineaSub = ("    " + Numeracion.ToString() + "." + Subnum.ToString() + " ").Length;
                            desfaseSub = "\n   " + ("").PadLeft(cantLineaSub, ' ');
                            var conceptoSub = subGasto.Item1.Replace("\n", desfaseSub);//String.Empty;
                            //for (int i = 0; i < subGasto.Item1.Length; i++)
                            //{
                            //    conceptoSub += subGasto.Item1[i] + ((i + 1) % cantLineaSub == 0 ? "\n".PadRight(desfaseSub, ' ') : String.Empty);
                            //}

                            DataRow rowDubGasto = ds.Tables["DSGastos"].NewRow();
                            rowDubGasto["Concepto"] = "    " + Numeracion.ToString() + "." + Subnum.ToString() + " " + conceptoSub;
                            Subnum++;
                            rowDubGasto["Detalle"] = subGasto.Item2;
                            rowDubGasto["Valor"] = "";
                            ds.Tables["DSGastos"].Rows.Add(rowDubGasto);
                        }
                    }

                    Numeracion++;

                }

                var lstGastosExtraordinarios = lstGastos.Where(X => !X.Ordinario && X.Pagado).ToList();
                Numeracion = 1;


                rowTitulo = ds.Tables["DSGastos"].NewRow();
                rowTitulo["Concepto"] = "Gastos No Corrientes";
                rowTitulo["EsTitulo"] = "1";
                rowTitulo["Valor"] = lstGastosExtraordinarios.Sum(X => X.Monto).ToString("#,##0.00");
                ds.Tables["DSGastos"].Rows.Add(rowTitulo);

                //rowTitulo = ds.Tables["DSGastos"].NewRow();
                //rowTitulo["Concepto"] = "       ";
                //rowTitulo["Valor"] = "_________";
                //rowTitulo["EsTitulo"] = "1";
                //ds.Tables["DSGastos"].Rows.Add(rowTitulo);

                foreach (var gasto in lstGastosExtraordinarios)
                {

                    DataRow rowGasto = ds.Tables["DSGastos"].NewRow();
                    var desfase = String.Empty;
                    var cantLinea = (Numeracion.ToString() + ". ").Length;
                    desfase = "\n " + ("").PadLeft(cantLinea, ' ');
                    var concepto = gasto.Concepto.Replace("\n", desfase);//String.Empty;
                    //for (int i = 0; i < gasto.Concepto.Length; i++)
                    //{
                    //    concepto += gasto.Concepto[i] + ((i + 1) % cantLinea == 0 ? "\n".PadRight(desfase, ' ') : String.Empty);
                    //}

                    rowGasto["Concepto"] = Numeracion.ToString() + ". " + concepto;
                    rowGasto["Valor"] = gasto.Monto.ToString("#,##0.00");
                    ds.Tables["DSGastos"].Rows.Add(rowGasto);
                    TotalGastos += gasto.Monto;
                    Int32 Subnum = 1;
                    if (!String.IsNullOrWhiteSpace(gasto.Detalle))
                    {
                        var subDetallesGastos = gasto.Detalle.Split('|').Where(X => !String.IsNullOrWhiteSpace(X)).Select(X => new Tuple<String, String>(X.Split(';').First(), X.Split(';').Last())).ToList();
                        foreach (var subGasto in subDetallesGastos)
                        {
                            var desfaseSub = String.Empty;
                            var cantLineaSub = ("    " + Numeracion.ToString() + "." + Subnum.ToString() + " ").Length;
                            desfaseSub = "\n   " + ("").PadLeft(cantLineaSub, ' ');
                            var conceptoSub = subGasto.Item1.Replace("\n", desfaseSub);//String.Empty;
                            //for (int i = 0; i < subGasto.Item1.Length; i++)
                            //{
                            //    conceptoSub += subGasto.Item1[i] + ((i + 1) % cantLineaSub == 0 ? "\n".PadRight(desfaseSub, ' ') : String.Empty);
                            //}

                            DataRow rowDubGasto = ds.Tables["DSGastos"].NewRow();
                            rowDubGasto["Concepto"] = "    " + Numeracion.ToString() + "." + Subnum.ToString() + " " + conceptoSub;
                            Subnum++;
                            rowDubGasto["Detalle"] = subGasto.Item2;
                            rowDubGasto["Valor"] = "";
                            ds.Tables["DSGastos"].Rows.Add(rowDubGasto);
                        }
                    }
                    Numeracion++;
                }

                var CuentasPorCobrarO = lstGastos.FirstOrDefault().Gasto.CuentasPorCobrarO;
                var CuentasPorCobrarE = lstGastos.FirstOrDefault().Gasto.CuentasPorCobrarE;

                rowTitulo = ds.Tables["DSGastos"].NewRow();
                rowTitulo["Concepto"] = "       ";
                rowTitulo["Valor"] = "        ";
                rowTitulo["EsTitulo"] = "1";
                ds.Tables["DSGastos"].Rows.Add(rowTitulo);

                rowTitulo = ds.Tables["DSGastos"].NewRow();
                rowTitulo["Concepto"] = "CUENTAS POR COBRAR";
                rowTitulo["EsTitulo"] = "1";
                var MontoCuentasPorCobrar = ((CuentasPorCobrarO ?? 0) + (CuentasPorCobrarE ?? 0));
                rowTitulo["Valor"] = MontoCuentasPorCobrar.ToString("#,##0.00");
                ds.Tables["DSGastos"].Rows.Add(rowTitulo);


                rowTitulo = ds.Tables["DSGastos"].NewRow();
                rowTitulo["Concepto"] = "1. Cuotas Ordinarias ";
                rowTitulo["Valor"] = (CuentasPorCobrarO ?? 0).ToString("#,##0.00");
                ds.Tables["DSGastos"].Rows.Add(rowTitulo);

                Decimal TotalCuentasPorCobrar = (lstIngresos.Sum(X => !X.Pagado ? X.Monto : 0));

                if (CuentasPorCobrarE.HasValue)
                {
                    rowTitulo = ds.Tables["DSGastos"].NewRow();
                    rowTitulo["Concepto"] = "2. Cuotas Extraordinarias ";
                    rowTitulo["Valor"] = CuentasPorCobrarE.Value.ToString("#,##0.00");
                    ds.Tables["DSGastos"].Rows.Add(rowTitulo);

                    TotalCuentasPorCobrar += ((CuentasPorCobrarO ?? 0) + (CuentasPorCobrarE ?? 0));
                }

                var lstCuentasPorPagar = lstGastos.Where(X => !X.Pagado).ToList();

                rowTitulo = ds.Tables["DSGastos"].NewRow();
                rowTitulo["Concepto"] = "CUENTAS POR PAGAR";
                rowTitulo["EsTitulo"] = "1";
                rowTitulo["Valor"] = lstCuentasPorPagar.Sum(X => X.Monto).ToString("#,##0.00");
                ds.Tables["DSGastos"].Rows.Add(rowTitulo);

                //rowTitulo = ds.Tables["DSGastos"].NewRow();
                //rowTitulo["Concepto"] = "       ";
                //rowTitulo["Valor"] = "========";
                //rowTitulo["EsTitulo"] = "1";
                //ds.Tables["DSGastos"].Rows.Add(rowTitulo);

                Numeracion = 1;
                Decimal TotalCuentasPorPagar = lstCuentasPorPagar.Sum(X => X.Monto);
                foreach (var gasto in lstCuentasPorPagar)
                {

                    DataRow rowGasto = ds.Tables["DSGastos"].NewRow();
                    var desfase = String.Empty;
                    var cantLinea = (Numeracion.ToString() + ". ").Length;
                    desfase = "\n " + ("").PadLeft(cantLinea, ' ');
                    var concepto = gasto.Concepto.Replace("\n", desfase);//String.Empty;
                    //for(int i = 0; i < gasto.Concepto.Length; i++)
                    //{
                    //    concepto += gasto.Concepto[i] + ((i + 1) % cantLinea == 0 ? "\n".PadRight(desfase,' ') : String.Empty);
                    //}
                    rowGasto["Concepto"] = Numeracion.ToString() + ". " + concepto;
                    rowGasto["Valor"] = gasto.Monto.ToString("#,##0.00");
                    ds.Tables["DSGastos"].Rows.Add(rowGasto);
                    TotalGastos += gasto.Monto;
                    Int32 Subnum = 1;
                    if (!String.IsNullOrWhiteSpace(gasto.Detalle))
                    {
                        var subDetallesGastos = gasto.Detalle.Split('|').Where(X => !String.IsNullOrWhiteSpace(X)).Select(X => new Tuple<String, String>(X.Split(';').First(), X.Split(';').Last())).ToList();
                        foreach (var subGasto in subDetallesGastos)
                        {
                            var desfaseSub = String.Empty;
                            var cantLineaSub = ("    " + Numeracion.ToString() + "." + Subnum.ToString() + " ").Length;
                            desfaseSub = "\n   " + ("").PadLeft(cantLineaSub, ' ');
                            var conceptoSub = subGasto.Item1.Replace("\n", desfaseSub);//String.Empty;
                            //for (int i = 0; i < subGasto.Item1.Length; i++)
                            //{
                            //    conceptoSub += subGasto.Item1[i] + ((i + 1) % cantLineaSub == 0 ? "\n".PadRight(desfaseSub, ' ') : String.Empty);
                            //}

                            DataRow rowDubGasto = ds.Tables["DSGastos"].NewRow();
                            rowDubGasto["Concepto"] = "    " + Numeracion.ToString() + "." + Subnum.ToString() + " " + conceptoSub;
                            Subnum++;
                            rowDubGasto["Detalle"] = subGasto.Item2;
                            rowDubGasto["Valor"] = "";
                            ds.Tables["DSGastos"].Rows.Add(rowDubGasto);
                        }
                    }
                    Numeracion++;
                }
                var ListNumeroLeyenda = LstLeyendas.Select(x => x.Numero).ToList();
                var TieneExtraOrdinaria = lstIngresos.Where( x => x.FechaEmision.Value.Month == x.FechaPagado.Value.Month).Sum(X => X.CuotaExtraordinaria) ?? 0;
                decimal sumExtraordinaria = 0;
                bool mostrarExtraordinaria = false;
                int cantCuotaExtraUnidad = 0;

                for (int i = 0; i < LstDepartamentos.Count; i++)
                {
                    decimal cuotaExtraordinaria = 0;
                    DataRow rowDepartamento = ds.Tables["DSIngresos"].NewRow();
                    rowDepartamento["Departamento"] = LstDepartamentos[i].TipoInmueble.Acronimo + "-" + LstDepartamentos[i].Numero;
                    var listaCuotasPagadas = lstIngresos.Where(X => X.DepartamentoId == LstDepartamentos[i].DepartamentoId).ToList();
                    var cantCuotasPagadas = listaCuotasPagadas.Count();
                    Decimal mora = listaCuotasPagadas.Sum(X => LstDepartamentos[i].OmitirMora ? 0 : X.Mora);
                    Decimal totalCuota = listaCuotasPagadas.Sum(X => X.Total);
                    Decimal Extraordinaria = listaCuotasPagadas.Sum(x => x.CuotaExtraordinaria) ?? 0;
                    var cantExtraEmitida = listaCuotasPagadas.Count(x => x.UnidadTiempoId == UnidadTiempo && x.CuotaExtraordinaria > 0);
                    cantExtraEmitida += listaCuotasPagadas.Count(x => x.EsExtraordinaria.HasValue && x.EsExtraordinaria == true);

                    if (totalCuota == 0 && LstDepartamentos[i].Estado != ConstantHelpers.EstadoActivo)
                        continue;
                    //if (totalCuota == Extraordinaria)
                    //{
                    //    totalCuota = 0;
                    if (cantExtraEmitida > 0)
                    {
                        mostrarExtraordinaria = true;
                    }

                    //}

                    Decimal totalMonto = listaCuotasPagadas.Sum(X => X.Monto);
                    Int32 cantCuotasPagas = listaCuotasPagadas.Count(x => x.FechaPagado != null);
                    rowDepartamento["Mora"] = mora == 0 ? "" : mora.ToString("#,##0.00");

                    var cantSeparada = lstIngresos.Count(X => X.DepartamentoId == LstDepartamentos[i].DepartamentoId && X.EsExtraordinaria == true);

                    if (TieneExtraOrdinaria > 0 && (totalMonto > 0 || cantSeparada > 0))
                    {
                        rowDepartamento["CuotaExtraordinaria"] = (Extraordinaria).ToString("#,##0.00");
                        cuotaExtraordinaria = Extraordinaria;
                    }
                    if (totalMonto == 0 && cantSeparada == 0 && sumExtraordinaria == 0)
                    {
                        TieneExtraOrdinaria = 0;
                        Extraordinaria = 0;
                        rowDepartamento["CuotaExtraordinaria"] = "0.00";
                    }
                    var calCuota = totalCuota - Extraordinaria;
                    if (calCuota == 0)
                    {
                        totalCuota = 0;

                        if (LstAdelantado.Contains(LstDepartamentos[i].DepartamentoId) == false)
                        {
                            rowDepartamento["Cuota"] = "0.00";


                        }
                        else
                        {
                            rowDepartamento["Cuota"] = "PAGO ADELANTADO";
                            rowDepartamento["CuotaExtraordinaria"] = "PAGO ADELANTADO";
                        }
                    }
                    else
                    {
                        decimal? preCalCuota = 0M;
                        decimal? preCalCuotaExtraordinaria = 0M;
                        if (listaCuotasPagadas.Count() == listaCuotasPagadas.Count(x => x.CuotaExtraordinaria > 0))
                        {
                            if (listaCuotasPagadas.FirstOrDefault().UnidadTiempoId == UnidadTiempo)
                            {
                                rowDepartamento["Cuota"] = (calCuota).ToString("#,##0.00");
                                rowDepartamento["CuotaExtraordinaria"] = (Extraordinaria).ToString("#,##0.00");
                            }
                            else
                            {
                                rowDepartamento["Cuota"] = (calCuota + Extraordinaria).ToString("#,##0.00");
                                calCuota = calCuota + Extraordinaria;
                                Extraordinaria = 0;
                            }

                            TotalIngresosTotal += calCuota;
                            TotalIngresosTotal += Extraordinaria;

                            totalCuota = calCuota;
                        }
                        else
                        {
                            foreach (var item in listaCuotasPagadas)
                            {
                                if (item.UnidadTiempoId == UnidadTiempo)
                                {
                                    preCalCuota += item.Total - item.CuotaExtraordinaria;
                                    preCalCuotaExtraordinaria += item.CuotaExtraordinaria;
                                    cantCuotaExtraUnidad++;
                                }
                                else
                                {
                                    preCalCuota += item.Total; //+ item.CuotaExtraordinaria;// + item.CuotaExtraordinaria;
                                }
                            }

                            totalCuota = preCalCuota.Value;

                            if (preCalCuotaExtraordinaria == 0 && cuotaExtraordinaria != Extraordinaria)
                            {
                                cuotaExtraordinaria = 0;
                                Extraordinaria = 0;
                            }

                            Extraordinaria = preCalCuotaExtraordinaria ?? 0;

                            rowDepartamento["Cuota"] = (preCalCuota.Value).ToString("#,##0.00");
                            rowDepartamento["CuotaExtraordinaria"] = (preCalCuotaExtraordinaria.Value).ToString("#,##0.00");

                            TotalIngresosTotal += preCalCuota ?? 0 + preCalCuotaExtraordinaria ?? 0;
                        }


                        /*
                        if (listaCuotasPagadas.Count(x => x.UnidadTiempoId == UnidadTiempo) == cantCuotasPagadas)
                        {
                            rowDepartamento["Cuota"] = (calCuota).ToString("#,##0.00");
                        }
                        else
                        {
                            rowDepartamento["Cuota"] = (calCuota + Extraordinaria).ToString("#,##0.00");
                            rowDepartamento["CuotaExtraordinaria"] = "0.00";
                            cuotaExtraordinaria = 0;
                        }
                        */
                    }
                    rowDepartamento["Total"] = (mora + totalCuota + Extraordinaria).ToString("#,##0.00");
                    //var balance = context.BalanceUnidadTiempoEdificio.FirstOrDefault( x => x.UnidadDeTiempoId == UnidadTiempo && x.EdificioId == EdificioId);
                    var leyenda = listaCuotasPagadas.Where(X => X.Leyenda != 0 && X.UnidadTiempoId <= UnidadTiempo).OrderByDescending(x => x.UnidadTiempoId).FirstOrDefault();
                    //rowDepartamento["Leyenda"] = leyenda != null ? leyenda.Leyenda.ToString() : String.Empty;
                    if (leyenda != null && leyenda.Leyenda != null)
                    {
                        if (ListNumeroLeyenda.Contains(leyenda.Leyenda.Value))
                            rowDepartamento["Leyenda"] += leyenda.Leyenda.ToString();
                    }
                    else
                    {
                        rowDepartamento["Leyenda"] = String.Empty;
                    }
                    //if (TieneExtraOrdinaria > 0)
                    //{
                    //    rowDepartamento["CuotaExtraordinaria"] = (Extraordinaria).ToString("#,##0.00");
                    //
                    //    if (calCuota == 0)
                    //    {
                    //        if (cantCuotasPagas != 0)
                    //            rowDepartamento["CuotaExtraordinaria"] = "PAGO ADELANTADO";
                    //    }
                    //}
                    //if (mora > 0)
                    //{
                    // var a = 0;
                    //}
                    ds.Tables["DSIngresos"].Rows.Add(rowDepartamento);
                    TotalIngresosMora += mora;
                    TotalIngresosCuota += totalCuota;

                    //if (totalCuota - Extraordinaria == 0)
                    //{
                    //    totalCuota = 0;
                    //}


                    TotalIngresosTotal += mora;//(mora + totalCuota);
                    //if (Extraordinaria != cuotaExtraordinaria)
                    //{
                    //    var a = 0;
                    //}
                    TotalExtraordinarias += (Extraordinaria);

                    sumExtraordinaria += cuotaExtraordinaria;
                }

                DataRow rowDepartamentoTotal2 = ds.Tables["DSIngresos"].NewRow();
                rowDepartamentoTotal2["Departamento"] = "TOTAL";
                rowDepartamentoTotal2["Mora"] = TotalIngresosMora.ToString("#,##0.00");
                rowDepartamentoTotal2["Cuota"] = (TotalIngresosCuota).ToString("#,##0.00");//(TotalIngresosCuota - TotalExtraordinarias).ToString("#,##0.00");
                rowDepartamentoTotal2["Total"] = (TotalIngresosCuota + TotalIngresosMora + TotalExtraordinarias).ToString("#,##0.00");
                //if (TieneExtraOrdinaria > 0)
                //{
                //    if (sumExtraordinaria == TotalExtraordinarias || cantCuotaExtraUnidad > 0)
                //    {
                //        rowDepartamentoTotal2["CuotaExtraordinaria"] = TotalExtraordinarias.ToString("#,##0.00");
                //        rowDepartamentoTotal2["Cuota"] = (TotalIngresosCuota).ToString("#,##0.00");
                //    }
                //    else
                //    {
                //        rowDepartamentoTotal2["CuotaExtraordinaria"] = "0.00";
                //        //mostrarExtraordinaria = false;
                //    }
                //}
                rowDepartamentoTotal2["CuotaExtraordinaria"] = TotalExtraordinarias.ToString("#,##0.00");
                rowDepartamentoTotal2["Cuota"] = (TotalIngresosCuota).ToString("#,##0.00");

                ds.Tables["DSIngresos"].Rows.Add(rowDepartamentoTotal2);

                Decimal SaldoActual = TotalIngresosTotal - TotalGastos;
                var Gasto = context.Gasto.FirstOrDefault(x => x.EdificioId == EdificioId && x.UnidadTiempoId == UnidadTiempo && x.Estado.Equals(ConstantHelpers.EstadoActivo));
                // Gasto.SaldoMes = SaldoActual;


                Decimal TotalIngresosComunes = 0;

                //Seteamos la tabla de ingresos comunes
                foreach (var ingresoComun in lstIngresosComunes)
                {
                    DataRow rowIngresosComunes = ds.Tables["DSIngresosComunes"].NewRow();
                    rowIngresosComunes["Descripcion"] = ingresoComun.Concepto;
                    rowIngresosComunes["Monto"] = (ingresoComun.Monto).ToString("#,##0.00");
                    ds.Tables["DSIngresosComunes"].Rows.Add(rowIngresosComunes);
                    TotalIngresosComunes += ingresoComun.Monto;
                }

                DataRow titulo = ds.Tables["DTInfo"].NewRow();
                titulo["Titulo"] = Titulo.ToUpper();
                titulo["TotalIngresos"] = ((Decimal)TotalIngresosComunes + TotalIngresosTotal).ToString("#,##0.00");
                titulo["ExisteAdicionales"] = TotalIngresosComunes != 0 ? true : false;
                ds.Tables["DTInfo"].Rows.Add(titulo);

                //Seteamos la tabla de leyendas

                foreach (var ley in LstLeyendas.OrderBy(x => x.Numero))
                {
                    DataRow rowLeyendas = ds.Tables["DSLeyendas"].NewRow();
                    rowLeyendas["Descripcion"] = ley.Descripcion;
                    rowLeyendas["Numero"] = ley.Numero;
                    ds.Tables["DSLeyendas"].Rows.Add(rowLeyendas);
                }

                var cuotas = context.Cuota.Where(X => X.Departamento.EdificioId == edificio.EdificioId && X.Pagado);
                var LstUnidadTiempo = context.UnidadTiempo.Where(x => x.Estado == ConstantHelpers.EstadoActivo && x.Orden <= unidadTiempoActual.Orden).OrderBy(x => x.Orden).ToList();

                SaldoAnterior = 0;
                Decimal SaldoAcumulado = 0;
                Decimal TotalPagosCuotasMes = 0;
                foreach (var item in LstUnidadTiempo)
                {
                    var ListCuotasT = new List<Cuota>();
                    foreach (var cuota in cuotas)
                    {
                        //Si no existe la fecha de pagado, añadir si cumple con la unidad de tiempo
                        if (!cuota.FechaPagado.HasValue && cuota.UnidadTiempoId == item.UnidadTiempoId)
                            ListCuotasT.Add(cuota);
                        else
                            //Si existe la fecha de pagado, comprar el mes y el año , si encajan con esta unidad de tiempo, entonces son parte del reporte
                            if (cuota.FechaPagado.HasValue && (cuota.FechaPagado.Value.Month == item.Mes && cuota.FechaPagado.Value.Year == item.Anio))
                        {
                            ListCuotasT.Add(cuota);
                        }
                    }
                    TotalPagosCuotasMes = ListCuotasT.Sum(X => X.Total + X.Mora);
                    SaldoAnterior = SaldoAcumulado;
                    if (item.UnidadTiempoId == edificio.SaldoAnteriorUnidadTiempo)
                    {
                        SaldoAnterior += edificio.SaldoAnteriorHistorico ?? 0;
                    }
                    var GastoTemp = context.Gasto.Where(X => X.UnidadTiempoId == item.UnidadTiempoId && EdificioId == X.EdificioId && X.Estado == ConstantHelpers.EstadoActivo).ToList().Sum(X => X.DetalleGasto.Where(Y => Y.Pagado == true).ToList().Sum(Y => Y.Monto));
                    var IngresoTemp = TotalPagosCuotasMes + context.Ingreso.Where(X => X.UnidadTiempoId == item.UnidadTiempoId && EdificioId == X.EdificioId && X.Estado == ConstantHelpers.EstadoActivo).ToList().Sum(X => X.DetalleIngreso.ToList().Sum(Y => Y.Monto));
                    var SaldoTemp = IngresoTemp - GastoTemp;
                    SaldoAcumulado = SaldoAnterior + SaldoTemp;
                }

                //var SaldoAcumulado = SaldoAnterior + (TotalIngresosTotal + TotalIngresosComunes - TotalGastos);
                //Seteamos la tabla de info


                var ListCuotas = new List<Cuota>();
                edificio = context.Edificio.First(X => X.EdificioId == EdificioId);
                cuotas = context.Cuota.Where(X => X.Departamento.EdificioId == edificio.EdificioId && X.Pagado);

                TotalPagosCuotasMes = 0;//ListCuotas.Sum(X => X.Total + X.Mora);
                Decimal TotalPagosCuotasAnterior = context.Cuota.Where(X => X.Pagado && X.Departamento.EdificioId == EdificioId && X.Estado == ConstantHelpers.EstadoActivo).ToList().Sum(X => X.Total + X.Mora) - TotalPagosCuotasMes;
                Decimal TotalIngresosAdicionales = context.Ingreso.Where(X => X.EdificioId == EdificioId && X.Estado == ConstantHelpers.EstadoActivo).ToList().Sum(X => X.DetalleIngreso.ToList().Sum(Y => Y.Monto));
                Decimal TotalGasto = context.Gasto.Where(X => X.EdificioId == EdificioId && X.Estado == ConstantHelpers.EstadoActivo).ToList().Sum(X => X.DetalleGasto.ToList().Sum(Y => Y.Monto));

                LstUnidadTiempo = context.UnidadTiempo.Where(x => x.Estado == ConstantHelpers.EstadoActivo && x.Orden <= unidadTiempoActual.Orden).OrderBy(x => x.Orden).ToList();

                SaldoAnterior = 0;
                SaldoAcumulado = 0;
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


                    var GastoTemp = context.Gasto.Where(X => X.UnidadTiempoId == item.UnidadTiempoId && EdificioId == X.EdificioId && X.Estado == ConstantHelpers.EstadoActivo).ToList().Sum(X => X.DetalleGasto.Where(Y => Y.Pagado == true).ToList().Sum(Y => Y.Monto));
                    var IngresoTemp = TotalPagosCuotasMes + context.Ingreso.Where(X => X.UnidadTiempoId == item.UnidadTiempoId && EdificioId == X.EdificioId && X.Estado == ConstantHelpers.EstadoActivo).ToList().Sum(X => X.DetalleIngreso.ToList().Sum(Y => Y.Monto));
                    var SaldoTemp = IngresoTemp - GastoTemp;
                    SaldoAcumulado = SaldoAnterior + SaldoTemp;
                }

                DataRow rowInfoTotales = ds.Tables["DSTotales"].NewRow();

                var GastosActual = context.Gasto.Where(X => X.UnidadTiempoId == UnidadTiempo && EdificioId == X.EdificioId && X.Estado == ConstantHelpers.EstadoActivo).ToList().Sum(X => X.DetalleGasto.Where(Y => Y.Pagado == true).ToList().Sum(Y => Y.Monto));
                var IngresosActual = context.Ingreso.Where(X => X.UnidadTiempoId == UnidadTiempo && EdificioId == X.EdificioId && X.Estado == ConstantHelpers.EstadoActivo).ToList().Sum(X => X.DetalleIngreso.ToList().Sum(Y => Y.Monto));
                var Acumulado = TotalPagosCuotasMes + TotalPagosCuotasAnterior + TotalIngresosAdicionales - TotalGasto;
                IngresosActual += TotalPagosCuotasMes;

                SaldoActual = IngresosActual - GastosActual;

                rowInfoTotales["Saldo"] = "S/ " + (SaldoActual).ToString("#,##0.00");
                rowInfoTotales["SaldoAnterior"] = "S/ " + SaldoAnterior.ToString("#,##0.00");
                rowInfoTotales["SaldoAcumulado"] = "S/ " + SaldoAcumulado.ToString("#,##0.00");
                rowInfoTotales["TotalGastos"] = "S/ " + GastosActual.ToString("#,##0.00");
                rowInfoTotales["TotalIngresos"] = "S/ " + (IngresosActual).ToString("#,##0.00");

                rowInfoTotales["CuentasPorCobrar"] = "S/ " + MontoCuentasPorCobrar.ToString("#,##0.00");//"S/ " + TotalCuentasPorCobrar.ToString("#,##0.00"); 
                rowInfoTotales["CuentasPorPagar"] = "S/ " + TotalCuentasPorPagar.ToString("#,##0.00");
                //rowInfoTotales["SaldoReal"] = "S/ " + (SaldoAcumulado + TotalCuentasPorCobrar - TotalCuentasPorPagar).ToString("#,##0.00");
                rowInfoTotales["SaldoReal"] = "S/ " + (SaldoAcumulado + MontoCuentasPorCobrar - TotalCuentasPorPagar).ToString("#,##0.00");
                ds.Tables["DSTotales"].Rows.Add(rowInfoTotales);
                //guardamos en db el saldo del mes y el acumulado

                // Edificio edificio = context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);
                edificio.SaldoAcumulado = SaldoAcumulado;
                context.SaveChanges();

                ReportDataSource rdsIngresos = new ReportDataSource("DSIngresosComunes", ds.Tables["DSIngresosComunes"].DefaultView);
                ReportDataSource rdsLeyendas = new ReportDataSource("DSLeyendas", ds.Tables["DSLeyendas"].DefaultView);
                ReportDataSource rdsInfo = new ReportDataSource("DSInfo", ds.Tables["DTInfo"].DefaultView);
                ReportDataSource rdsDepartamento = new ReportDataSource("DSIngresos", ds.Tables["DSIngresos"].DefaultView);
                ReportDataSource rdsGastos = new ReportDataSource("DSGastos", ds.Tables["DSGastos"].DefaultView);
                ReportDataSource rdsTotales = new ReportDataSource("DSTotales", ds.Tables["DSTotales"].DefaultView);
                rv.ProcessingMode = ProcessingMode.Local;
                rv.LocalReport.EnableExternalImages = true;
                if (TieneExtraOrdinaria > 0 && mostrarExtraordinaria == true)
                    rv.LocalReport.ReportEmbeddedResource = "VEH.Intranet.Report.ReporteIngresosGastos.rdlc";
                else
                    rv.LocalReport.ReportEmbeddedResource = "VEH.Intranet.Report.ReporteIngresosGastosSinExtraOrdinaria.rdlc";
                rv.LocalReport.DataSources.Add(rdsInfo);
                rv.LocalReport.DataSources.Add(rdsDepartamento);
                rv.LocalReport.DataSources.Add(rdsGastos);
                rv.LocalReport.DataSources.Add(rdsTotales);
                rv.LocalReport.DataSources.Add(rdsLeyendas);
                rv.LocalReport.DataSources.Add(rdsIngresos);

                rv.LocalReport.SetParameters(new ReportParameter("OtrosIngresos", TotalIngresosComunes != 0 ? "1" : "0"));
                rv.LocalReport.SetParameters(new ReportParameter("FlagLeyendas", LstLeyendas.Count > 0 ? "T" : "F"));

                Warning[] warnings;
                string[] streamids;
                string mimeType;
                string encoding;
                string filenameExtension;

                byte[] bytes = rv.LocalReport.Render(
                    "PDF", null, out mimeType, out encoding, out filenameExtension,
                    out streamids, out warnings);


                String fileName = Server.MapPath("~/Resources") + "\\ingresosYgastos.zip";
                MemoryStream outputMemStream = null;


                if (EsAdministrador)
                {
                    outputMemStream = new MemoryStream();

                    ZipOutputStream zipStream = new ZipOutputStream(outputMemStream);

                    zipStream.SetLevel(3); //0-9, 9 being the highest level of compression

                    ZipEntry entry_pdf = new ZipEntry("ingresosYgastos.pdf");
                    entry_pdf.DateTime = DateTime.Now;
                    zipStream.PutNextEntry(entry_pdf);
                    StreamUtils.Copy(new MemoryStream(bytes), zipStream, new byte[4096]);
                    zipStream.CloseEntry();

                    Warning[] warnings_excel;
                    string[] streamids_excel;
                    string mimeType_excel;
                    string encoding_excel;
                    string filenameExtension_excel;

                    byte[] bytes_excel = rv.LocalReport.Render(
                        "Excel", null, out mimeType_excel, out encoding_excel, out filenameExtension_excel,
                        out streamids_excel, out warnings_excel);

                    ZipEntry entry_excel = new ZipEntry("ingresosYgastos.xls");
                    entry_excel.DateTime = DateTime.Now;
                    zipStream.PutNextEntry(entry_excel);
                    StreamUtils.Copy(new MemoryStream(bytes_excel), zipStream, new byte[4096]);
                    zipStream.CloseEntry();

                    zipStream.IsStreamOwner = false;
                    zipStream.Close();

                }
                else
                {
                    outputMemStream = new MemoryStream(bytes);
                }

                outputMemStream.Position = 0;

                return outputMemStream;
            }
            catch (Exception ex)
            {
                throw;
            }
            //  return new MemoryStream(null);
        }
        public MemoryStream GetReportIngresosGastosAPI(String Titulo, List<DetalleGasto> lstGastos, List<DetalleIngreso> lstIngresosComunes, List<Cuota> lstIngresos, Decimal SaldoAnterior, Int32 EdificioId, Int32 UnidadTiempo, bool exportadoAntes, DateTime fechaRegistro, List<Leyenda> LstLeyendas, bool EsAdministrador, List<Int32> LstAdelantado)
        {
            // byte[] bytes = (byte)0;
            try
            {
                UnidadTiempo unidadTiempoActual = context.UnidadTiempo.FirstOrDefault(x => x.UnidadTiempoId == UnidadTiempo);
                Decimal TotalIngresosTotal = 0M;
                Decimal TotalIngresosMora = 0M;
                Decimal TotalIngresosCuota = 0M;
                Decimal TotalGastos = 0M;
                Decimal TotalExtraordinarias = 0M;

                rv.Clear();
                rv.LocalReport.DataSources.Clear();
                DSInfoIngresosGastos ds = new DSInfoIngresosGastos();


                List<Departamento> LstDepartamentos = new List<Departamento>();
                //LstDepartamentos = context.Departamento.Where(x => x.EdificioId == EdificioId && x.Estado.Equals(ConstantHelpers.EstadoActivo)).ToList();
                LstDepartamentos = context.Departamento.Where(x => x.EdificioId == EdificioId).ToList();
                List<DateTime> LstFechasEmision = new List<DateTime>();
                Edificio edificio = context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);



                UnidadTiempo unidadTiempoAnterior = unidadTiempoActual;
                List<Cuota> ListIngresosTemp = lstIngresos;
                List<Decimal> LstMontoTotalDepa = new List<decimal>();
                List<Boolean> LstEncontrado = new List<bool>();
                lstIngresos.ForEach(x => LstMontoTotalDepa.Add(0M));
                lstIngresos.ForEach(x => LstFechasEmision.Add(DateTime.MinValue));
                lstIngresos.ForEach(x => LstEncontrado.Add(false));
                for (int i = 0; i < lstIngresos.Count; i++)
                    if (lstIngresos[i].Pagado)
                    {
                        DateTime fechaEmision = DateTime.Now;
                        DateTime.TryParseExact(unidadTiempoActual.Anio.ToString() + unidadTiempoActual.Mes.ToString()
                            + edificio.DiaEmisionCuota.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out fechaEmision);

                        //LstFechasEmision.Add(new DateTime(unidadTiempoActual.Anio, unidadTiempoActual.Mes, edificio.DiaEmisionCuota));
                        LstFechasEmision.Add(fechaEmision);
                        LstMontoTotalDepa[i] += ListIngresosTemp[i].Total;
                        //ListIngresosTemp[i].Estado = ConstantHelpers.EstadoCerrado;
                    }
                //else
                for (int i = 0; i < LstDepartamentos.Count; i++)
                {
                    // int DiasTranscurridosMora = LstFechasEmision[i].Equals(DateTime.MinValue) ? 0 : (DateTime.Now.Date - LstFechasEmision[i].Date).Days - 1; ;

                    int DiasTranscurridosMora = 0;
                    if (LstFechasEmision.Count > i)
                        DiasTranscurridosMora = LstFechasEmision[i].Equals(DateTime.MinValue) ? 0 : (fechaRegistro.Date - LstFechasEmision[i].Date).Days - 1; ;
                    Decimal moraUnitaria = edificio.TipoMora.Equals(ConstantHelpers.TipoMoraPorcentual) ? edificio.MontoCuota * edificio.PMora.Value / 100M : edificio.PMora.Value;
                    LstDepartamentos[i].MontoMora = (DiasTranscurridosMora <= 0 || i >= LstMontoTotalDepa.Count || LstMontoTotalDepa[i] == 0M) ? 0M : moraUnitaria * DiasTranscurridosMora;
                }
                Int32 Numeracion = 1;
                var lstGastosCorrientes = lstGastos.Where(X => X.Ordinario && X.Pagado).ToList();

                DataRow rowDepartamentoTotal = ds.Tables["DSGastos"].NewRow();
                rowDepartamentoTotal["Concepto"] = "TOTAL";
                rowDepartamentoTotal["EsTitulo"] = "1";
                rowDepartamentoTotal["Valor"] = lstGastos.Sum(X => X.Pagado ? X.Monto : 0).ToString("#,##0.00");
                ds.Tables["DSGastos"].Rows.Add(rowDepartamentoTotal);

                DataRow rowTitulo = ds.Tables["DSGastos"].NewRow();
                rowTitulo["Concepto"] = "       ";
                rowTitulo["EsTitulo"] = "1";
                rowTitulo["Valor"] = "========";
                ds.Tables["DSGastos"].Rows.Add(rowTitulo);

                rowTitulo = ds.Tables["DSGastos"].NewRow();
                rowTitulo["Concepto"] = "Gastos Corrientes";
                rowTitulo["EsTitulo"] = "1";
                rowTitulo["Valor"] = lstGastosCorrientes.Sum(X => X.Monto).ToString("#,##0.00");

                ds.Tables["DSGastos"].Rows.Add(rowTitulo);

                //rowTitulo = ds.Tables["DSGastos"].NewRow();
                //rowTitulo["Concepto"] = "       ";
                //rowTitulo["EsTitulo"] = "1";
                //rowTitulo["Valor"] = "_________";
                //ds.Tables["DSGastos"].Rows.Add(rowTitulo);

                foreach (var gasto in lstGastosCorrientes)
                {

                    DataRow rowGasto = ds.Tables["DSGastos"].NewRow();
                    var desfase = String.Empty;
                    var cantLinea = (Numeracion.ToString() + ". ").Length;
                    desfase = "\n " + ("").PadLeft(cantLinea, ' ');
                    var concepto = gasto.Concepto.Replace("\n", desfase);//String.Empty;

                    rowGasto["Concepto"] = Numeracion.ToString() + ". " + concepto;
                    rowGasto["Valor"] = gasto.Monto.ToString("#,##0.00");
                    rowGasto["Detalle"] = "";
                    ds.Tables["DSGastos"].Rows.Add(rowGasto);
                    TotalGastos += gasto.Monto;
                    Int32 Subnum = 1;
                    if (!String.IsNullOrWhiteSpace(gasto.Detalle))
                    {
                        var subDetallesGastos = gasto.Detalle.Split('|').Where(X => !String.IsNullOrWhiteSpace(X)).Select(X => new Tuple<String, String>(X.Split(';').First(), X.Split(';').Last())).ToList();
                        foreach (var subGasto in subDetallesGastos)
                        {
                            var desfaseSub = String.Empty;
                            var cantLineaSub = ("    " + Numeracion.ToString() + "." + Subnum.ToString() + " ").Length;
                            desfaseSub = "\n   " + ("").PadLeft(cantLineaSub, ' ');
                            var conceptoSub = subGasto.Item1.Replace("\n", desfaseSub);//String.Empty;
                            //for (int i = 0; i < subGasto.Item1.Length; i++)
                            //{
                            //    conceptoSub += subGasto.Item1[i] + ((i + 1) % cantLineaSub == 0 ? "\n".PadRight(desfaseSub, ' ') : String.Empty);
                            //}

                            DataRow rowDubGasto = ds.Tables["DSGastos"].NewRow();
                            rowDubGasto["Concepto"] = "    " + Numeracion.ToString() + "." + Subnum.ToString() + " " + conceptoSub;
                            Subnum++;
                            rowDubGasto["Detalle"] = subGasto.Item2;
                            rowDubGasto["Valor"] = "";
                            ds.Tables["DSGastos"].Rows.Add(rowDubGasto);
                        }
                    }

                    Numeracion++;

                }

                var lstGastosExtraordinarios = lstGastos.Where(X => !X.Ordinario && X.Pagado).ToList();
                Numeracion = 1;


                rowTitulo = ds.Tables["DSGastos"].NewRow();
                rowTitulo["Concepto"] = "Gastos No Corrientes";
                rowTitulo["EsTitulo"] = "1";
                rowTitulo["Valor"] = lstGastosExtraordinarios.Sum(X => X.Monto).ToString("#,##0.00");
                ds.Tables["DSGastos"].Rows.Add(rowTitulo);

                //rowTitulo = ds.Tables["DSGastos"].NewRow();
                //rowTitulo["Concepto"] = "       ";
                //rowTitulo["Valor"] = "_________";
                //rowTitulo["EsTitulo"] = "1";
                //ds.Tables["DSGastos"].Rows.Add(rowTitulo);

                foreach (var gasto in lstGastosExtraordinarios)
                {

                    DataRow rowGasto = ds.Tables["DSGastos"].NewRow();
                    var desfase = String.Empty;
                    var cantLinea = (Numeracion.ToString() + ". ").Length;
                    desfase = "\n " + ("").PadLeft(cantLinea, ' ');
                    var concepto = gasto.Concepto.Replace("\n", desfase);//String.Empty;
                    //for (int i = 0; i < gasto.Concepto.Length; i++)
                    //{
                    //    concepto += gasto.Concepto[i] + ((i + 1) % cantLinea == 0 ? "\n".PadRight(desfase, ' ') : String.Empty);
                    //}

                    rowGasto["Concepto"] = Numeracion.ToString() + ". " + concepto;
                    rowGasto["Valor"] = gasto.Monto.ToString("#,##0.00");
                    ds.Tables["DSGastos"].Rows.Add(rowGasto);
                    TotalGastos += gasto.Monto;
                    Int32 Subnum = 1;
                    if (!String.IsNullOrWhiteSpace(gasto.Detalle))
                    {
                        var subDetallesGastos = gasto.Detalle.Split('|').Where(X => !String.IsNullOrWhiteSpace(X)).Select(X => new Tuple<String, String>(X.Split(';').First(), X.Split(';').Last())).ToList();
                        foreach (var subGasto in subDetallesGastos)
                        {
                            var desfaseSub = String.Empty;
                            var cantLineaSub = ("    " + Numeracion.ToString() + "." + Subnum.ToString() + " ").Length;
                            desfaseSub = "\n   " + ("").PadLeft(cantLineaSub, ' ');
                            var conceptoSub = subGasto.Item1.Replace("\n", desfaseSub);//String.Empty;
                            //for (int i = 0; i < subGasto.Item1.Length; i++)
                            //{
                            //    conceptoSub += subGasto.Item1[i] + ((i + 1) % cantLineaSub == 0 ? "\n".PadRight(desfaseSub, ' ') : String.Empty);
                            //}

                            DataRow rowDubGasto = ds.Tables["DSGastos"].NewRow();
                            rowDubGasto["Concepto"] = "    " + Numeracion.ToString() + "." + Subnum.ToString() + " " + conceptoSub;
                            Subnum++;
                            rowDubGasto["Detalle"] = subGasto.Item2;
                            rowDubGasto["Valor"] = "";
                            ds.Tables["DSGastos"].Rows.Add(rowDubGasto);
                        }
                    }
                    Numeracion++;
                }

                var CuentasPorCobrarO = lstGastos.FirstOrDefault().Gasto.CuentasPorCobrarO;
                var CuentasPorCobrarE = lstGastos.FirstOrDefault().Gasto.CuentasPorCobrarE;

                rowTitulo = ds.Tables["DSGastos"].NewRow();
                rowTitulo["Concepto"] = "       ";
                rowTitulo["Valor"] = "        ";
                rowTitulo["EsTitulo"] = "1";
                ds.Tables["DSGastos"].Rows.Add(rowTitulo);

                rowTitulo = ds.Tables["DSGastos"].NewRow();
                rowTitulo["Concepto"] = "CUENTAS POR COBRAR";
                rowTitulo["EsTitulo"] = "1";
                var MontoCuentasPorCobrar = ((CuentasPorCobrarO ?? 0) + (CuentasPorCobrarE ?? 0));
                rowTitulo["Valor"] = MontoCuentasPorCobrar.ToString("#,##0.00");
                ds.Tables["DSGastos"].Rows.Add(rowTitulo);


                rowTitulo = ds.Tables["DSGastos"].NewRow();
                rowTitulo["Concepto"] = "1. Cuotas Ordinarias ";
                rowTitulo["Valor"] = (CuentasPorCobrarO ?? 0).ToString("#,##0.00");
                ds.Tables["DSGastos"].Rows.Add(rowTitulo);

                Decimal TotalCuentasPorCobrar = (lstIngresos.Sum(X => !X.Pagado ? X.Monto : 0));

                if (CuentasPorCobrarE.HasValue)
                {
                    rowTitulo = ds.Tables["DSGastos"].NewRow();
                    rowTitulo["Concepto"] = "2. Cuotas Extraordinarias ";
                    rowTitulo["Valor"] = CuentasPorCobrarE.Value.ToString("#,##0.00");
                    ds.Tables["DSGastos"].Rows.Add(rowTitulo);

                    TotalCuentasPorCobrar += ((CuentasPorCobrarO ?? 0) + (CuentasPorCobrarE ?? 0));
                }

                var lstCuentasPorPagar = lstGastos.Where(X => !X.Pagado).ToList();

                rowTitulo = ds.Tables["DSGastos"].NewRow();
                rowTitulo["Concepto"] = "CUENTAS POR PAGAR";
                rowTitulo["EsTitulo"] = "1";
                rowTitulo["Valor"] = lstCuentasPorPagar.Sum(X => X.Monto).ToString("#,##0.00");
                ds.Tables["DSGastos"].Rows.Add(rowTitulo);

                //rowTitulo = ds.Tables["DSGastos"].NewRow();
                //rowTitulo["Concepto"] = "       ";
                //rowTitulo["Valor"] = "========";
                //rowTitulo["EsTitulo"] = "1";
                //ds.Tables["DSGastos"].Rows.Add(rowTitulo);

                Numeracion = 1;
                Decimal TotalCuentasPorPagar = lstCuentasPorPagar.Sum(X => X.Monto);
                foreach (var gasto in lstCuentasPorPagar)
                {

                    DataRow rowGasto = ds.Tables["DSGastos"].NewRow();
                    var desfase = String.Empty;
                    var cantLinea = (Numeracion.ToString() + ". ").Length;
                    desfase = "\n " + ("").PadLeft(cantLinea, ' ');
                    var concepto = gasto.Concepto.Replace("\n", desfase);//String.Empty;
                    //for(int i = 0; i < gasto.Concepto.Length; i++)
                    //{
                    //    concepto += gasto.Concepto[i] + ((i + 1) % cantLinea == 0 ? "\n".PadRight(desfase,' ') : String.Empty);
                    //}
                    rowGasto["Concepto"] = Numeracion.ToString() + ". " + concepto;
                    rowGasto["Valor"] = gasto.Monto.ToString("#,##0.00");
                    ds.Tables["DSGastos"].Rows.Add(rowGasto);
                    TotalGastos += gasto.Monto;
                    Int32 Subnum = 1;
                    if (!String.IsNullOrWhiteSpace(gasto.Detalle))
                    {
                        var subDetallesGastos = gasto.Detalle.Split('|').Where(X => !String.IsNullOrWhiteSpace(X)).Select(X => new Tuple<String, String>(X.Split(';').First(), X.Split(';').Last())).ToList();
                        foreach (var subGasto in subDetallesGastos)
                        {
                            var desfaseSub = String.Empty;
                            var cantLineaSub = ("    " + Numeracion.ToString() + "." + Subnum.ToString() + " ").Length;
                            desfaseSub = "\n   " + ("").PadLeft(cantLineaSub, ' ');
                            var conceptoSub = subGasto.Item1.Replace("\n", desfaseSub);//String.Empty;
                            //for (int i = 0; i < subGasto.Item1.Length; i++)
                            //{
                            //    conceptoSub += subGasto.Item1[i] + ((i + 1) % cantLineaSub == 0 ? "\n".PadRight(desfaseSub, ' ') : String.Empty);
                            //}

                            DataRow rowDubGasto = ds.Tables["DSGastos"].NewRow();
                            rowDubGasto["Concepto"] = "    " + Numeracion.ToString() + "." + Subnum.ToString() + " " + conceptoSub;
                            Subnum++;
                            rowDubGasto["Detalle"] = subGasto.Item2;
                            rowDubGasto["Valor"] = "";
                            ds.Tables["DSGastos"].Rows.Add(rowDubGasto);
                        }
                    }
                    Numeracion++;
                }
                var ListNumeroLeyenda = LstLeyendas.Select(x => x.Numero).ToList();
                var TieneExtraOrdinaria = lstIngresos.Sum(X => X.CuotaExtraordinaria) ?? 0;
                decimal sumExtraordinaria = 0;
                bool mostrarExtraordinaria = false;
                int cantCuotaExtraUnidad = 0;

                for (int i = 0; i < LstDepartamentos.Count; i++)
                {
                    decimal cuotaExtraordinaria = 0;
                    DataRow rowDepartamento = ds.Tables["DSIngresos"].NewRow();
                    rowDepartamento["Departamento"] = LstDepartamentos[i].TipoInmueble.Acronimo + "-" + LstDepartamentos[i].Numero;
                    var listaCuotasPagadas = lstIngresos.Where(X => X.DepartamentoId == LstDepartamentos[i].DepartamentoId).ToList();
                    var cantCuotasPagadas = listaCuotasPagadas.Count();
                    Decimal mora = listaCuotasPagadas.Sum(X => LstDepartamentos[i].OmitirMora ? 0 : X.Mora);
                    Decimal totalCuota = listaCuotasPagadas.Sum(X => X.Total);
                    Decimal Extraordinaria = listaCuotasPagadas.Sum(x => x.CuotaExtraordinaria) ?? 0;
                    var cantExtraEmitida = listaCuotasPagadas.Count(x => x.UnidadTiempoId == UnidadTiempo && x.CuotaExtraordinaria > 0);
                    cantExtraEmitida += listaCuotasPagadas.Count(x => x.EsExtraordinaria.HasValue && x.EsExtraordinaria == true);

                    if (totalCuota == 0 && LstDepartamentos[i].Estado != ConstantHelpers.EstadoActivo)
                        continue;
                    //if (totalCuota == Extraordinaria)
                    //{
                    //    totalCuota = 0;
                    if (cantExtraEmitida > 0)
                    {
                        mostrarExtraordinaria = true;
                    }

                    //}

                    Decimal totalMonto = listaCuotasPagadas.Sum(X => X.Monto);
                    Int32 cantCuotasPagas = listaCuotasPagadas.Count(x => x.FechaPagado != null);
                    rowDepartamento["Mora"] = mora == 0 ? "" : mora.ToString("#,##0.00");

                    var cantSeparada = lstIngresos.Count(X => X.DepartamentoId == LstDepartamentos[i].DepartamentoId && X.EsExtraordinaria == true);

                    if (TieneExtraOrdinaria > 0 && (totalMonto > 0 || cantSeparada > 0))
                    {
                        rowDepartamento["CuotaExtraordinaria"] = (Extraordinaria).ToString("#,##0.00");
                        cuotaExtraordinaria = Extraordinaria;
                    }
                    if (totalMonto == 0 && cantSeparada == 0 && sumExtraordinaria == 0)
                    {
                        TieneExtraOrdinaria = 0;
                        Extraordinaria = 0;
                        rowDepartamento["CuotaExtraordinaria"] = "0.00";
                    }
                    var calCuota = totalCuota - Extraordinaria;
                    if (calCuota == 0)
                    {
                        totalCuota = 0;

                        if (LstAdelantado.Contains(LstDepartamentos[i].DepartamentoId) == false)
                        {
                            rowDepartamento["Cuota"] = "0.00";


                        }
                        else
                        {
                            rowDepartamento["Cuota"] = "PAGO ADELANTADO";
                            rowDepartamento["CuotaExtraordinaria"] = "PAGO ADELANTADO";
                        }
                    }
                    else
                    {
                        decimal? preCalCuota = 0M;
                        decimal? preCalCuotaExtraordinaria = 0M;
                        if (listaCuotasPagadas.Count() == listaCuotasPagadas.Count(x => x.CuotaExtraordinaria > 0))
                        {
                            if (listaCuotasPagadas.FirstOrDefault().UnidadTiempoId == UnidadTiempo)
                            {
                                rowDepartamento["Cuota"] = (calCuota).ToString("#,##0.00");
                                rowDepartamento["CuotaExtraordinaria"] = (Extraordinaria).ToString("#,##0.00");
                            }
                            else
                            {
                                rowDepartamento["Cuota"] = (calCuota + Extraordinaria).ToString("#,##0.00");
                                calCuota = calCuota + Extraordinaria;
                                Extraordinaria = 0;
                            }

                            TotalIngresosTotal += calCuota;
                            TotalIngresosTotal += Extraordinaria;

                            totalCuota = calCuota;
                        }
                        else
                        {
                            foreach (var item in listaCuotasPagadas)
                            {
                                if (item.UnidadTiempoId == UnidadTiempo)
                                {
                                    preCalCuota += item.Total - item.CuotaExtraordinaria;
                                    preCalCuotaExtraordinaria += item.CuotaExtraordinaria;
                                    cantCuotaExtraUnidad++;
                                }
                                else
                                {
                                    preCalCuota += item.Total; //+ item.CuotaExtraordinaria;// + item.CuotaExtraordinaria;
                                }
                            }

                            totalCuota = preCalCuota.Value;

                            if (preCalCuotaExtraordinaria == 0 && cuotaExtraordinaria != Extraordinaria)
                            {
                                cuotaExtraordinaria = 0;
                                Extraordinaria = 0;
                            }

                            Extraordinaria = preCalCuotaExtraordinaria ?? 0;

                            rowDepartamento["Cuota"] = (preCalCuota.Value).ToString("#,##0.00");
                            rowDepartamento["CuotaExtraordinaria"] = (preCalCuotaExtraordinaria.Value).ToString("#,##0.00");

                            TotalIngresosTotal += preCalCuota ?? 0 + preCalCuotaExtraordinaria ?? 0;
                        }


                        /*
                        if (listaCuotasPagadas.Count(x => x.UnidadTiempoId == UnidadTiempo) == cantCuotasPagadas)
                        {
                            rowDepartamento["Cuota"] = (calCuota).ToString("#,##0.00");
                        }
                        else
                        {
                            rowDepartamento["Cuota"] = (calCuota + Extraordinaria).ToString("#,##0.00");
                            rowDepartamento["CuotaExtraordinaria"] = "0.00";
                            cuotaExtraordinaria = 0;
                        }
                        */
                    }
                    rowDepartamento["Total"] = (mora + totalCuota + Extraordinaria).ToString("#,##0.00");
                    //var balance = context.BalanceUnidadTiempoEdificio.FirstOrDefault( x => x.UnidadDeTiempoId == UnidadTiempo && x.EdificioId == EdificioId);
                    var leyenda = listaCuotasPagadas.Where(X => X.Leyenda != 0 && X.UnidadTiempoId <= UnidadTiempo).OrderByDescending(x => x.UnidadTiempoId).FirstOrDefault();
                    //rowDepartamento["Leyenda"] = leyenda != null ? leyenda.Leyenda.ToString() : String.Empty;
                    if (leyenda != null && leyenda.Leyenda != null)
                    {
                        if (ListNumeroLeyenda.Contains(leyenda.Leyenda.Value))
                            rowDepartamento["Leyenda"] += leyenda.Leyenda.ToString();
                    }
                    else
                    {
                        rowDepartamento["Leyenda"] = String.Empty;
                    }
                    //if (TieneExtraOrdinaria > 0)
                    //{
                    //    rowDepartamento["CuotaExtraordinaria"] = (Extraordinaria).ToString("#,##0.00");
                    //
                    //    if (calCuota == 0)
                    //    {
                    //        if (cantCuotasPagas != 0)
                    //            rowDepartamento["CuotaExtraordinaria"] = "PAGO ADELANTADO";
                    //    }
                    //}
                    //if (mora > 0)
                    //{
                    // var a = 0;
                    //}
                    ds.Tables["DSIngresos"].Rows.Add(rowDepartamento);
                    TotalIngresosMora += mora;
                    TotalIngresosCuota += totalCuota;

                    //if (totalCuota - Extraordinaria == 0)
                    //{
                    //    totalCuota = 0;
                    //}


                    TotalIngresosTotal += mora;//(mora + totalCuota);
                    //if (Extraordinaria != cuotaExtraordinaria)
                    //{
                    //    var a = 0;
                    //}
                    TotalExtraordinarias += (Extraordinaria);

                    sumExtraordinaria += cuotaExtraordinaria;
                }

                DataRow rowDepartamentoTotal2 = ds.Tables["DSIngresos"].NewRow();
                rowDepartamentoTotal2["Departamento"] = "TOTAL";
                rowDepartamentoTotal2["Mora"] = TotalIngresosMora.ToString("#,##0.00");
                rowDepartamentoTotal2["Cuota"] = (TotalIngresosCuota).ToString("#,##0.00");//(TotalIngresosCuota - TotalExtraordinarias).ToString("#,##0.00");
                rowDepartamentoTotal2["Total"] = (TotalIngresosCuota + TotalIngresosMora + TotalExtraordinarias).ToString("#,##0.00");
                //if (TieneExtraOrdinaria > 0)
                //{
                //    if (sumExtraordinaria == TotalExtraordinarias || cantCuotaExtraUnidad > 0)
                //    {
                //        rowDepartamentoTotal2["CuotaExtraordinaria"] = TotalExtraordinarias.ToString("#,##0.00");
                //        rowDepartamentoTotal2["Cuota"] = (TotalIngresosCuota).ToString("#,##0.00");
                //    }
                //    else
                //    {
                //        rowDepartamentoTotal2["CuotaExtraordinaria"] = "0.00";
                //        //mostrarExtraordinaria = false;
                //    }
                //}
                rowDepartamentoTotal2["CuotaExtraordinaria"] = TotalExtraordinarias.ToString("#,##0.00");
                rowDepartamentoTotal2["Cuota"] = (TotalIngresosCuota).ToString("#,##0.00");

                ds.Tables["DSIngresos"].Rows.Add(rowDepartamentoTotal2);

                Decimal SaldoActual = TotalIngresosTotal - TotalGastos;
                var Gasto = context.Gasto.FirstOrDefault(x => x.EdificioId == EdificioId && x.UnidadTiempoId == UnidadTiempo && x.Estado.Equals(ConstantHelpers.EstadoActivo));
                // Gasto.SaldoMes = SaldoActual;


                Decimal TotalIngresosComunes = 0;

                //Seteamos la tabla de ingresos comunes
                foreach (var ingresoComun in lstIngresosComunes)
                {
                    DataRow rowIngresosComunes = ds.Tables["DSIngresosComunes"].NewRow();
                    rowIngresosComunes["Descripcion"] = ingresoComun.Concepto;
                    rowIngresosComunes["Monto"] = (ingresoComun.Monto).ToString("#,##0.00");
                    ds.Tables["DSIngresosComunes"].Rows.Add(rowIngresosComunes);
                    TotalIngresosComunes += ingresoComun.Monto;
                }

                DataRow titulo = ds.Tables["DTInfo"].NewRow();
                titulo["Titulo"] = Titulo.ToUpper();
                titulo["TotalIngresos"] = ((Decimal)TotalIngresosComunes + TotalIngresosTotal).ToString("#,##0.00");
                titulo["ExisteAdicionales"] = TotalIngresosComunes != 0 ? true : false;
                ds.Tables["DTInfo"].Rows.Add(titulo);

                //Seteamos la tabla de leyendas

                foreach (var ley in LstLeyendas.OrderBy(x => x.Numero))
                {
                    DataRow rowLeyendas = ds.Tables["DSLeyendas"].NewRow();
                    rowLeyendas["Descripcion"] = ley.Descripcion;
                    rowLeyendas["Numero"] = ley.Numero;
                    ds.Tables["DSLeyendas"].Rows.Add(rowLeyendas);
                }

                var cuotas = context.Cuota.Where(X => X.Departamento.EdificioId == edificio.EdificioId && X.Pagado);
                var LstUnidadTiempo = context.UnidadTiempo.Where(x => x.Estado == ConstantHelpers.EstadoActivo && x.Orden <= unidadTiempoActual.Orden).OrderBy(x => x.Orden).ToList();

                SaldoAnterior = 0;
                Decimal SaldoAcumulado = 0;
                Decimal TotalPagosCuotasMes = 0;
                foreach (var item in LstUnidadTiempo)
                {
                    var ListCuotas = new List<Cuota>();
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
                    var GastoTemp = context.Gasto.Where(X => X.UnidadTiempoId == item.UnidadTiempoId && EdificioId == X.EdificioId && X.Estado == ConstantHelpers.EstadoActivo).ToList().Sum(X => X.DetalleGasto.Where(Y => Y.Pagado == true).ToList().Sum(Y => Y.Monto));
                    var IngresoTemp = TotalPagosCuotasMes + context.Ingreso.Where(X => X.UnidadTiempoId == item.UnidadTiempoId && EdificioId == X.EdificioId && X.Estado == ConstantHelpers.EstadoActivo).ToList().Sum(X => X.DetalleIngreso.ToList().Sum(Y => Y.Monto));
                    var SaldoTemp = IngresoTemp - GastoTemp;
                    SaldoAcumulado = SaldoAnterior + SaldoTemp;
                }

                //var SaldoAcumulado = SaldoAnterior + (TotalIngresosTotal + TotalIngresosComunes - TotalGastos);
                //Seteamos la tabla de info
                var ti = lstGastos.Sum(X => X.Pagado ? X.Monto : 0).ToString("#,##0.00");
                DataRow rowInfoTotales = ds.Tables["DSTotales"].NewRow();
                rowInfoTotales["TotalIngresos"] = "S/ " + (TotalIngresosTotal + TotalIngresosComunes).ToString("#,##0.00");
                rowInfoTotales["TotalGastos"] = "S/ " + ti;//TotalGastos.ToString("#,##0.00");
                rowInfoTotales["Saldo"] = "S/ " + (TotalIngresosTotal + TotalIngresosComunes - TotalGastos + TotalCuentasPorPagar).ToString("#,##0.00");
                rowInfoTotales["SaldoAnterior"] = "S/ " + SaldoAnterior.ToString("#,##0.00");
                rowInfoTotales["SaldoAcumulado"] = "S/ " + SaldoAcumulado.ToString("#,##0.00");
                rowInfoTotales["CuentasPorCobrar"] = "S/ " + MontoCuentasPorCobrar.ToString("#,##0.00");//"S/ " + TotalCuentasPorCobrar.ToString("#,##0.00"); 
                rowInfoTotales["CuentasPorPagar"] = "S/ " + TotalCuentasPorPagar.ToString("#,##0.00");
                //rowInfoTotales["SaldoReal"] = "S/ " + (SaldoAcumulado + TotalCuentasPorCobrar - TotalCuentasPorPagar).ToString("#,##0.00");
                rowInfoTotales["SaldoReal"] = "S/ " + (SaldoAcumulado + MontoCuentasPorCobrar - TotalCuentasPorPagar).ToString("#,##0.00");
                ds.Tables["DSTotales"].Rows.Add(rowInfoTotales);
                //guardamos en db el saldo del mes y el acumulado

                // Edificio edificio = context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);
                edificio.SaldoAcumulado = SaldoAcumulado;
                context.SaveChanges();

                ReportDataSource rdsIngresos = new ReportDataSource("DSIngresosComunes", ds.Tables["DSIngresosComunes"].DefaultView);
                ReportDataSource rdsLeyendas = new ReportDataSource("DSLeyendas", ds.Tables["DSLeyendas"].DefaultView);
                ReportDataSource rdsInfo = new ReportDataSource("DSInfo", ds.Tables["DTInfo"].DefaultView);
                ReportDataSource rdsDepartamento = new ReportDataSource("DSIngresos", ds.Tables["DSIngresos"].DefaultView);
                ReportDataSource rdsGastos = new ReportDataSource("DSGastos", ds.Tables["DSGastos"].DefaultView);
                ReportDataSource rdsTotales = new ReportDataSource("DSTotales", ds.Tables["DSTotales"].DefaultView);
                rv.ProcessingMode = ProcessingMode.Local;
                rv.LocalReport.EnableExternalImages = true;
                if (TieneExtraOrdinaria > 0 && mostrarExtraordinaria == true)
                    rv.LocalReport.ReportEmbeddedResource = "VEH.Intranet.Report.ReporteIngresosGastos.rdlc";
                else
                    rv.LocalReport.ReportEmbeddedResource = "VEH.Intranet.Report.ReporteIngresosGastosSinExtraOrdinaria.rdlc";
                rv.LocalReport.DataSources.Add(rdsInfo);
                rv.LocalReport.DataSources.Add(rdsDepartamento);
                rv.LocalReport.DataSources.Add(rdsGastos);
                rv.LocalReport.DataSources.Add(rdsTotales);
                rv.LocalReport.DataSources.Add(rdsLeyendas);
                rv.LocalReport.DataSources.Add(rdsIngresos);

                rv.LocalReport.SetParameters(new ReportParameter("OtrosIngresos", TotalIngresosComunes != 0 ? "1" : "0"));
                rv.LocalReport.SetParameters(new ReportParameter("FlagLeyendas", LstLeyendas.Count > 0 ? "T" : "F"));

                Warning[] warnings;
                string[] streamids;
                string mimeType;
                string encoding;
                string filenameExtension;

                byte[] bytes = rv.LocalReport.Render(
                    "PDF", null, out mimeType, out encoding, out filenameExtension,
                    out streamids, out warnings);


                //String fileName = Server.MapPath("~/Resources") + "\\ingresosYgastos.zip";
                MemoryStream outputMemStream = null;


                if (EsAdministrador)
                {
                    outputMemStream = new MemoryStream();

                    ZipOutputStream zipStream = new ZipOutputStream(outputMemStream);

                    zipStream.SetLevel(3); //0-9, 9 being the highest level of compression

                    ZipEntry entry_pdf = new ZipEntry("ingresosYgastos.pdf");
                    entry_pdf.DateTime = DateTime.Now;
                    zipStream.PutNextEntry(entry_pdf);
                    StreamUtils.Copy(new MemoryStream(bytes), zipStream, new byte[4096]);
                    zipStream.CloseEntry();

                    Warning[] warnings_excel;
                    string[] streamids_excel;
                    string mimeType_excel;
                    string encoding_excel;
                    string filenameExtension_excel;

                    byte[] bytes_excel = rv.LocalReport.Render(
                        "Excel", null, out mimeType_excel, out encoding_excel, out filenameExtension_excel,
                        out streamids_excel, out warnings_excel);

                    ZipEntry entry_excel = new ZipEntry("ingresosYgastos.xls");
                    entry_excel.DateTime = DateTime.Now;
                    zipStream.PutNextEntry(entry_excel);
                    StreamUtils.Copy(new MemoryStream(bytes_excel), zipStream, new byte[4096]);
                    zipStream.CloseEntry();

                    zipStream.IsStreamOwner = false;
                    zipStream.Close();

                }
                else
                {
                    outputMemStream = new MemoryStream(bytes);
                }

                outputMemStream.Position = 0;

                return outputMemStream;
            }
            catch (Exception ex)
            {
                throw;
            }
            //  return new MemoryStream(null);
        }

        public MemoryStream GetReportMensual(String Titulo, List<Planilla> lstplanillas)
        {
            rv.Clear();
            rv.LocalReport.DataSources.Clear();
            DSInfoPlanilla ds = new DSInfoPlanilla();
            DataRow titulo = ds.Tables["DTInfo"].NewRow();
            titulo["Titulo"] = Titulo;
            ds.Tables["DTInfo"].Rows.Add(titulo);

            foreach (Planilla planilla in lstplanillas)
            {
                DataRow rowPlanilla = ds.Tables["DTPlanilla"].NewRow();
                Trabajador trabajador = planilla.Trabajador;
                String NombreCompleto = trabajador.Nombres + " " + trabajador.Apellidos;

                rowPlanilla["Trabajador"] = NombreCompleto;
                rowPlanilla["Puesto"] = trabajador.Cargo;
                rowPlanilla["SueldoBase"] = trabajador.SueldoBase;
                rowPlanilla["HorasExtra25"] = planilla.HorasExtras25;
                rowPlanilla["MontoExtra25"] = planilla.MontoHorasExtras25;
                rowPlanilla["HorasExtra35"] = planilla.HorasExtras35;
                rowPlanilla["MontoExtra35"] = planilla.MontoHorasExtras35;
                rowPlanilla["FeriadosTrabajados"] = planilla.Feriado;
                rowPlanilla["MontoFeriados"] = planilla.MontoFeriados;
                rowPlanilla["DescuentoAusencia"] = planilla.DescuenoAusencia;
                rowPlanilla["AumentoReemplazo"] = planilla.AumentoReemplazo;
                rowPlanilla["TotalMes"] = planilla.TotalMes;
                rowPlanilla["AdelantoQuincena"] = planilla.AdelantoQuincena;
                rowPlanilla["SegundaQuincena"] = planilla.SegundaQuincena;
                rowPlanilla["Essalud"] = planilla.ESSALUD;
                String nombreAFP = (planilla.Trabajador.AFP == null) ? "ONP" : planilla.Trabajador.AFP.Nombre;
                rowPlanilla["AFP"] = nombreAFP;
                rowPlanilla["TotalDescuentos"] = planilla.TotalDescuentos;
                rowPlanilla["SueldoTotalNeto"] = planilla.SueldoTotalNeto;
                rowPlanilla["SegundaQuincenaNeto"] = planilla.SegundaQuincenaNeto;
                rowPlanilla["GratificacionesMes"] = planilla.GratificacionesMes;
                rowPlanilla["CTS"] = planilla.CTSMes;
                rowPlanilla["Reemplazo"] = planilla.ReemplazoVacaciones;

                ds.Tables["DTPlanilla"].Rows.Add(rowPlanilla);
            }

            ReportDataSource rdsInfo = new ReportDataSource("DSInfo", ds.Tables["DTInfo"].DefaultView);
            ReportDataSource rdsPlanilla = new ReportDataSource("DSPlanilla", ds.Tables["DTPlanilla"].DefaultView);
            rv.ProcessingMode = ProcessingMode.Local;
            rv.LocalReport.EnableExternalImages = true;
            rv.LocalReport.ReportEmbeddedResource = "VEH.Intranet.Report.ReportePlanilla.rdlc";
            rv.LocalReport.DataSources.Add(rdsInfo);
            rv.LocalReport.DataSources.Add(rdsPlanilla);

            Warning[] warnings;
            string[] streamids;
            string mimeType;
            string encoding;
            string filenameExtension;

            byte[] bytes = rv.LocalReport.Render(
                "Excel", null, out mimeType, out encoding, out filenameExtension,
                out streamids, out warnings);

            return new MemoryStream(bytes);
        }

        public MemoryStream GetReportQuincena(String Titulo, String UnidadTiempo, List<PlanillaQuincena> lstplantillas)
        {
            ReportViewer rv = new ReportViewer();
            DSInfoQuincena ds = new DSInfoQuincena();
            DataRow titulo = ds.Tables["DTInfo"].NewRow();
            titulo["Titulo"] = Titulo;
            ds.Tables["DTInfo"].Rows.Add(titulo);

            foreach (PlanillaQuincena planilla in lstplantillas)
            {
                DataRow rowPlanilla = ds.Tables["DTQuincena"].NewRow();
                Trabajador trabajador = planilla.Trabajador;
                String NombreCompleto = trabajador.Nombres + " " + trabajador.Apellidos;

                rowPlanilla["Trabajador"] = NombreCompleto;
                rowPlanilla["Puesto"] = trabajador.Cargo;
                rowPlanilla["SueldoBase"] = trabajador.SueldoBase;
                rowPlanilla["Movilidad"] = planilla.BonoPorMovilidad;
                rowPlanilla["Bonificacion"] = planilla.Bonificacion;
                rowPlanilla["TotalQuincena"] = planilla.TotalQuincena;
                rowPlanilla["Seguro"] = planilla.Seguro;

                ds.Tables["DTQuincena"].Rows.Add(rowPlanilla);
            }

            ReportDataSource rdsInfo = new ReportDataSource("DSInfo", ds.Tables["DTInfo"].DefaultView);
            ReportDataSource rdsDepartamento = new ReportDataSource("DSQuincena", ds.Tables["DTQuincena"].DefaultView);
            rv.ProcessingMode = ProcessingMode.Local;
            rv.LocalReport.EnableExternalImages = true;
            rv.LocalReport.ReportEmbeddedResource = "VEH.Intranet.Report.ReporteQuincena.rdlc";
            rv.LocalReport.DataSources.Add(rdsInfo);
            rv.LocalReport.DataSources.Add(rdsDepartamento);

            Warning[] warnings;
            string[] streamids;
            string mimeType;
            string encoding;
            string filenameExtension;

            byte[] bytes = rv.LocalReport.Render(
                "PDF", null, out mimeType, out encoding, out filenameExtension,
                out streamids, out warnings);

            Warning[] warnings_excel;
            string[] streamids_excel;
            string mimeType_excel;
            string encoding_excel;
            string filenameExtension_excel;

            byte[] bytes_excel = rv.LocalReport.Render(
                "Excel", null, out mimeType_excel, out encoding_excel, out filenameExtension_excel,
                out streamids_excel, out warnings_excel);


            String fileName = Server.MapPath("~/Resources") + "\\informe-quincenal.zip";
            MemoryStream outputMemStream = new MemoryStream();
            ZipOutputStream zipStream = new ZipOutputStream(outputMemStream);

            zipStream.SetLevel(3); //0-9, 9 being the highest level of compression

            ZipEntry entry_pdf = new ZipEntry("Informe-quincenal.pdf");
            entry_pdf.DateTime = DateTime.Now;
            zipStream.PutNextEntry(entry_pdf);
            StreamUtils.Copy(new MemoryStream(bytes), zipStream, new byte[4096]);
            zipStream.CloseEntry();

            ZipEntry entry_excel = new ZipEntry("Informe-quincenal.xls");
            entry_excel.DateTime = DateTime.Now;
            zipStream.PutNextEntry(entry_excel);
            StreamUtils.Copy(new MemoryStream(bytes_excel), zipStream, new byte[4096]);
            zipStream.CloseEntry();

            zipStream.IsStreamOwner = false;
            zipStream.Close();

            outputMemStream.Position = 0;

            return outputMemStream;
            //return new MemoryStream(bytes);
        }

        public MemoryStream ZipFiles()
        {
            try
            {
                String fileName = Server.MapPath("~/Resources") + "\\reportes.zip";
                MemoryStream outputMemStream = new MemoryStream();
                ZipOutputStream zipStream = new ZipOutputStream(outputMemStream);

                zipStream.SetLevel(3); //0-9, 9 being the highest level of compression

                for (int i = 0; i < lstNombreDOC.Count; ++i)
                {
                    ZipEntry newEntry = new ZipEntry(lstNombreDOC[i]);
                    newEntry.DateTime = DateTime.Now;

                    zipStream.PutNextEntry(newEntry);

                    StreamUtils.Copy(lstMemoryStream[i], zipStream, new byte[4096]);
                    zipStream.CloseEntry();

                }


                ZipEntry newEntry2 = new ZipEntry("REPORTE GENERAL.pdf");
                newEntry2.DateTime = DateTime.Now;

                zipStream.PutNextEntry(newEntry2);

                MemoryStream general = MergePdfForms(lstMemoryStreamPDF);
                //pdfFile.Close();


                StreamUtils.Copy(general, zipStream, new byte[4096]);
                //pCopy.Close();
                //doc.Close();

                zipStream.CloseEntry();


                ZipEntry entry_excel = new ZipEntry("REPORTE GENERAL.xls");
                entry_excel.DateTime = DateTime.Now;
                zipStream.PutNextEntry(entry_excel);
                StreamUtils.Copy(new MemoryStream(ExcelArchivo), zipStream, new byte[4096]);
                zipStream.CloseEntry();





                zipStream.IsStreamOwner = false;    // False stops the Close also Closing the underlying stream.
                zipStream.Close();                 // Must finish the ZipOutputStream before using outputMemStream.

                outputMemStream.Position = 0;
                /*zipStream.IsStreamOwner = true;
                byte[] buffer = new byte[16 * 1024];
                using (MemoryStream ms = new MemoryStream())
                {
                    int read;
                    while ((read = outputMemStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                    buffer = ms.ToArray();
                }

                using (FileStream fs = new FileStream(fileName, FileMode.Create))
                {
                    fs.Write(buffer, 0, buffer.Length);
                }*/
                return outputMemStream;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public MemoryStream MergePdfForms(List<byte[]> files)
        {
            /*if (files.Count > 1)
            {
                
                MemoryStream msOutput = new MemoryStream();

                pdfFile = new PdfReader(files[0]);

                doc = new Document();
                pCopy = new PdfSmartCopy(doc, msOutput);

                doc.Open();

                for (int k = 0; k < files.Count; k++)
                {
                    pdfFile = new PdfReader(files[k]);
                    for (int i = 1; i < pdfFile.NumberOfPages + 1; i++)
                    {
                        ((PdfSmartCopy)pCopy).AddPage(pCopy.GetImportedPage(pdfFile, i));
                    }
                    pCopy.FreeReader(pdfFile);
                }

                return new MemoryStream(msOutput.ToArray());
            }
            else if (files.Count == 1)
            {
                return new MemoryStream(files[0]);
            }

            return null;*/

            byte[] mergedPdf = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (Document document = new Document())
                {
                    using (PdfCopy copy = new PdfCopy(document, ms))
                    {
                        document.Open();

                        for (int i = 0; i < files.Count; ++i)
                        {
                            PdfReader reader = new PdfReader(files[i]);
                            // loop over the pages in that document
                            int n = reader.NumberOfPages;
                            for (int page = 0; page < n;)
                            {
                                copy.AddPage(copy.GetImportedPage(reader, ++page));
                            }
                        }
                    }
                }
                mergedPdf = ms.ToArray();
            }
            return new MemoryStream(mergedPdf);
        }
        public MemoryStream GetReportMantenimiento(String Titulo, List<Cronograma> LstCronograma, String NombreEdificio)
        {
            try
            {
                rv.Clear();
                rv.LocalReport.DataSources.Clear();
                DSInfoReporteMantenimiento ds = new DSInfoReporteMantenimiento();

                foreach (var crono in LstCronograma)
                {
                    DataRow rowCronograma = ds.Tables["DTCronograma"].NewRow();

                    rowCronograma["Nombre"] = crono.Nombre;
                    for (int i = 1; i <= 12; i++)
                    {
                        var detalle = crono.DetalleCronograma.FirstOrDefault(x => x.Mes == i);
                        rowCronograma[ConstantHelpers.ObtenerMesPorValorId(i.ToString())] = detalle.EsMarcado ? "X" : detalle.EsRealizado ? "." : String.Empty;
                    }

                    ds.Tables["DTCronograma"].Rows.Add(rowCronograma);
                }

                ReportDataSource rdsCronograma = new ReportDataSource("DSInfoReporteMantenimiento", ds.Tables["DTCronograma"].DefaultView);
                rv.ProcessingMode = ProcessingMode.Local;
                rv.LocalReport.EnableExternalImages = true;

                rv.LocalReport.ReportEmbeddedResource = "VEH.Intranet.Report.ReporteMantenimiento.rdlc";

                rv.LocalReport.DataSources.Add(rdsCronograma);

                rv.LocalReport.SetParameters(new ReportParameter("Titulo", Titulo));


                Warning[] warnings;
                string[] streamids;
                string mimeType;
                string encoding;
                string filenameExtension;

                byte[] bytes = rv.LocalReport.Render(
                    "PDF", null, out mimeType, out encoding, out filenameExtension,
                    out streamids, out warnings);

                Warning[] warnings_excel;
                string[] streamids_excel;
                string mimeType_excel;
                string encoding_excel;
                string filenameExtension_excel;

                byte[] bytes_excel = rv.LocalReport.Render(
                    "Excel", null, out mimeType_excel, out encoding_excel, out filenameExtension_excel,
                    out streamids_excel, out warnings_excel);


                String fileName = Server.MapPath("~/Resources") + "\\Mantenimientos " + NombreEdificio + ".zip";
                MemoryStream outputMemStream = new MemoryStream();
                ZipOutputStream zipStream = new ZipOutputStream(outputMemStream);

                zipStream.SetLevel(3); //0-9, 9 being the highest level of compression

                ZipEntry entry_pdf = new ZipEntry("Mantenimientos " + NombreEdificio + ".pdf");
                entry_pdf.DateTime = DateTime.Now;
                zipStream.PutNextEntry(entry_pdf);
                StreamUtils.Copy(new MemoryStream(bytes), zipStream, new byte[4096]);
                zipStream.CloseEntry();

                ZipEntry entry_excel = new ZipEntry("Mantenimientos " + NombreEdificio + ".xls");
                entry_excel.DateTime = DateTime.Now;
                zipStream.PutNextEntry(entry_excel);
                StreamUtils.Copy(new MemoryStream(bytes_excel), zipStream, new byte[4096]);
                zipStream.CloseEntry();

                zipStream.IsStreamOwner = false;
                zipStream.Close();

                outputMemStream.Position = 0;

                return outputMemStream;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public void GetReportMantenimientoAPI(String Titulo, List<Cronograma> LstCronograma)
        {
            try
            {
                rv.Clear();
                rv.LocalReport.DataSources.Clear();
                DSInfoReporteMantenimiento ds = new DSInfoReporteMantenimiento();

                foreach (var crono in LstCronograma)
                {
                    DataRow rowCronograma = ds.Tables["DTCronograma"].NewRow();

                    rowCronograma["Nombre"] = crono.Nombre;
                    for (int i = 1; i <= 12; i++)
                    {
                        var detalle = crono.DetalleCronograma.FirstOrDefault(x => x.Mes == i);
                        rowCronograma[ConstantHelpers.ObtenerMesPorValorId(i.ToString())] = detalle.EsMarcado ? "X" : detalle.EsRealizado ? "." : String.Empty;
                    }

                    ds.Tables["DTCronograma"].Rows.Add(rowCronograma);
                }

                ReportDataSource rdsCronograma = new ReportDataSource("DSInfoReporteMantenimiento", ds.Tables["DTCronograma"].DefaultView);
                rv.ProcessingMode = ProcessingMode.Local;
                rv.LocalReport.EnableExternalImages = true;

                rv.LocalReport.ReportEmbeddedResource = "VEH.Intranet.Report.ReporteMantenimiento.rdlc";

                rv.LocalReport.DataSources.Add(rdsCronograma);

                rv.LocalReport.SetParameters(new ReportParameter("Titulo", Titulo));


                Warning[] warnings;
                string[] streamids;
                string mimeType;
                string encoding;
                string filenameExtension;

                byte[] bytes = rv.LocalReport.Render(
                    "PDF", null, out mimeType, out encoding, out filenameExtension,
                    out streamids, out warnings);


                lstMemoryStreamPDF.Add(bytes);                
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public String GetReportTable(List<Cuota> listaCuota, String UnidadTiempo)
        {
            try
            {
                rv.Clear();
                rv.LocalReport.DataSources.Clear();
                DSInfoReporteEdificio ds = new DSInfoReporteEdificio();

                DataRow titulo = ds.Tables["DTInfo"].NewRow();
                titulo["UnidadTiempo"] = " " + UnidadTiempo;
                //" " + UnidadTiempo + "\n\r EDIFICIO " + listaCuota.FirstOrDefault().Departamento.Edificio.Nombre;
                ds.Tables["DTInfo"].Rows.Add(titulo);
                Boolean EsDiferenteCuotatTotal = false;
                foreach (var cuota in listaCuota)
                {
                    DataRow rowDepartamento = ds.Tables["DTDepartamento"].NewRow();
                    String nombrePropietario = "SIN PROPIETARIO";
                    if (cuota.Departamento.Propietario.Count > 0)
                    {
                        Propietario p = ConstantHelpers.getTitularDepartamento(cuota.Departamento);
                        nombrePropietario = p.Nombres + " " + p.ApellidoPaterno + " " + p.ApellidoMaterno;

                        if (listaCuota.FirstOrDefault().Departamento.Edificio.UsarInquilinoCCPD == true)
                        {
                            var i = p.Inquilino.FirstOrDefault(x => x.Estado == ConstantHelpers.EstadoActivo);
                            if (i != null)
                            {
                                nombrePropietario = i.Nombres;
                            }
                        }                            
                    }
                    rowDepartamento["NroDepartamento"] = cuota.Departamento.Numero;
                    rowDepartamento["Propietario"] = nombrePropietario;
                    rowDepartamento["LecturaAnterior"] = cuota.LecturaAgua - cuota.ConsumoAgua;
                    rowDepartamento["LecturaActual"] = cuota.LecturaAgua;
                    rowDepartamento["ConsumoDelMes"] = cuota.ConsumoMes.ToString("#,##0.00");
                    rowDepartamento["AreaComun"] = cuota.AreaComun.ToString("#,##0.00");
                    rowDepartamento["Alcantarillado"] = cuota.Alcantarillado.ToString("#,##0.00");
                    rowDepartamento["CargoFijo"] = cuota.CargoFijo.ToString("#,##0.00");
                    rowDepartamento["IGV"] = cuota.IGV.ToString("#,##0.00");
                    rowDepartamento["ConsumoAgua"] = cuota.ConsumoAguaTotal.ToString("#,##0.00");
                    rowDepartamento["ConsumoSoles"] = cuota.ConsumoSoles.ToString("#,##0.00");
                    rowDepartamento["Cuota"] = cuota.Monto.ToString("#,##0.00");
                    //rowDepartamento["Extraordinaria"] = (cuota.CuotaExtraordinaria ?? 0).ToString("#,##0.00");
                    rowDepartamento["Extraordinaria"] = (cuota.CuotaExtraordinaria ?? 0).ToString("#,##0.00");
                    //rowDepartamento["CuotaExtraordinaria"] = (cuota.CuotaExtraordinaria ?? 0).ToString("#,##0.00");89
                    rowDepartamento["Otros"] = cuota.ConsumoIndividual.Where(x => x.Estado == ConstantHelpers.EstadoActivo).Sum(x => x.Monto).ToString("#,##0.00");
                    rowDepartamento["Total"] = (cuota.Total).ToString("#,##0.00");

                    if (cuota.Monto != cuota.Total)
                    {
                        EsDiferenteCuotatTotal = true;
                    }

                    ds.Tables["DTDepartamento"].Rows.Add(rowDepartamento);
                }
                Int32 ContColumnNoVisible = 0;
                String FlagLecturaAnterior = "T";
                String FlagLecturaActual = "T";
                String FlagConsumoDelMes = "T";
                String FlagAreaComun = "T";
                String FlagAlcantarillado = "T";
                String FlagCargoFijo = "T";
                String FlagIGV = "T";
                String FlagConsumoAgua = "T";
                String FlagConsumoSoles = "T";
                String FlagCuota = "T";
                String FlagExtraordinaria = "T";
                String FlagOtros = "T";
                String ColumnaProInq = "Propietario";
                if (listaCuota.FirstOrDefault().Departamento.Edificio.UsarInquilinoCCPD == true)
                {
                    ColumnaProInq = "Propietario o Inquilino";
                }
                DataRow rowDepartamentoTotal = ds.Tables["DTTotalesDepartamento"].NewRow();
                rowDepartamentoTotal["NroDepartamento"] = "";
                rowDepartamentoTotal["Propietario"] = "";
                rowDepartamentoTotal["LecturaAnterior"] = "";
                var OtrosSum = listaCuota.Sum(X => X.ConsumoIndividual.Where(y => y.Estado == ConstantHelpers.EstadoActivo).Sum(y => y.Monto));
                rowDepartamentoTotal["Otros"] = OtrosSum.ToString("#,##0.00");

                if (OtrosSum == 0)
                {
                    FlagOtros = "F";
                    ContColumnNoVisible++;
                }
                if (listaCuota.Sum(X => X.LecturaAgua) - listaCuota.Sum(X => X.ConsumoAgua) == 0)
                {
                    FlagLecturaAnterior = "F";
                    ContColumnNoVisible++;
                }
                rowDepartamentoTotal["LecturaActual"] = "";
                if (listaCuota.Sum(X => X.LecturaAgua) == 0)
                {
                    FlagLecturaActual = "F";
                    ContColumnNoVisible++;
                }
                rowDepartamentoTotal["ConsumoDelMes"] = listaCuota.Sum(X => X.ConsumoMes).ToString("#,##0.00");
                if (listaCuota.Sum(X => X.ConsumoMes) == 0)
                {
                    FlagConsumoDelMes = "F";
                    ContColumnNoVisible++;
                }
                rowDepartamentoTotal["AreaComun"] = listaCuota.Sum(X => X.AreaComun).ToString("#,##0.00");
                if (listaCuota.Sum(X => X.AreaComun) == 0)
                {
                    FlagAreaComun = "F";
                    ContColumnNoVisible++;
                }
                rowDepartamentoTotal["Alcantarillado"] = listaCuota.Sum(X => X.Alcantarillado).ToString("#,##0.00");
                if (listaCuota.Sum(X => X.Alcantarillado) == 0)
                {
                    FlagAlcantarillado = "F";
                    ContColumnNoVisible++;
                }
                rowDepartamentoTotal["CargoFijo"] = listaCuota.Sum(X => X.CargoFijo).ToString("#,##0.00");
                if (listaCuota.Sum(X => X.CargoFijo) == 0)
                {
                    FlagCargoFijo = "F";
                    ContColumnNoVisible++;
                }
                rowDepartamentoTotal["IGV"] = listaCuota.Sum(X => X.IGV).ToString("#,##0.00");
                if (listaCuota.Sum(X => X.IGV) == 0)
                {
                    FlagIGV = "F";
                    ContColumnNoVisible++;
                }
                rowDepartamentoTotal["ConsumoAgua"] = listaCuota.Sum(X => X.ConsumoAguaTotal).ToString("#,##0.00");
                if (listaCuota.Sum(X => X.ConsumoAguaTotal) == 0)
                {
                    FlagConsumoAgua = "F";
                    ContColumnNoVisible++;
                }
                rowDepartamentoTotal["ConsumoSoles"] = listaCuota.Sum(X => X.ConsumoSoles).ToString("#,##0.00");
                if (listaCuota.Sum(X => X.ConsumoSoles) == 0)
                {
                    FlagConsumoSoles = "F";
                    ContColumnNoVisible++;
                }
                rowDepartamentoTotal["Cuota"] = listaCuota.Sum(X => X.Monto).ToString("#,##0.00");
                if (listaCuota.Sum(X => X.Monto) == 0)
                {
                    FlagCuota = "F";
                }
                var totalExtraordinarias = (listaCuota.Sum(X => X.CuotaExtraordinaria) ?? 0);
                rowDepartamentoTotal["Extraordinaria"] = totalExtraordinarias.ToString("#,##0.00");
                if (totalExtraordinarias == 0)
                {
                    FlagExtraordinaria = "F";
                }

                if (EsDiferenteCuotatTotal == false)
                {
                    FlagCuota = "F";
                }
                rowDepartamentoTotal["Total"] = (listaCuota.Sum(X => X.Total)).ToString("#,##0.00");
                ds.Tables["DTTotalesDepartamento"].Rows.Add(rowDepartamentoTotal);

                ReportDataSource rdsInfo = new ReportDataSource("DSInfo", ds.Tables["DTInfo"].DefaultView);
                ReportDataSource rdsDepartamento = new ReportDataSource("DSInfoReporteEdificio", ds.Tables["DTDepartamento"].DefaultView);
                ReportDataSource rdsTotales = new ReportDataSource("DataSetTotales", ds.Tables["DTTotalesDepartamento"].DefaultView);
                rv.ProcessingMode = ProcessingMode.Local;
                rv.LocalReport.EnableExternalImages = true;

                if (ContColumnNoVisible >= 8)
                {
                    if (ContColumnNoVisible == 1)
                    {
                        rv.LocalReport.ReportEmbeddedResource = "VEH.Intranet.Report.ReporteEdificio3.rdlc";
                    }
                    else
                    {
                        rv.LocalReport.ReportEmbeddedResource = "VEH.Intranet.Report.ReporteEdificio2.rdlc";
                    }
                }
                else
                {
                    rv.LocalReport.ReportEmbeddedResource = "VEH.Intranet.Report.ReporteEdificio.rdlc";
                }

                rv.LocalReport.DataSources.Add(rdsInfo);
                rv.LocalReport.DataSources.Add(rdsDepartamento);
                rv.LocalReport.DataSources.Add(rdsTotales);

                var TipoInmueble = listaCuota.Count > 0 ? listaCuota[0].Departamento.TipoInmueble : null;

                rv.LocalReport.SetParameters(new ReportParameter("Edificio", "EDIFICIO " + listaCuota.FirstOrDefault().Departamento.Edificio.Nombre));

                rv.LocalReport.SetParameters(new ReportParameter("TipoInmueble", TipoInmueble.Acronimo));
                rv.LocalReport.SetParameters(new ReportParameter("TipoInmuebleNombreCompleto", TipoInmueble.Nombre));

                rv.LocalReport.SetParameters(new ReportParameter("FlagLecturaAnterior", FlagLecturaAnterior));
                rv.LocalReport.SetParameters(new ReportParameter("FlagLecturaActual", FlagLecturaActual));
                rv.LocalReport.SetParameters(new ReportParameter("FlagConsumoMes", FlagConsumoDelMes));
                rv.LocalReport.SetParameters(new ReportParameter("FlagAreaComun", FlagAreaComun));
                rv.LocalReport.SetParameters(new ReportParameter("FlagAlcantarillado", FlagAlcantarillado));
                rv.LocalReport.SetParameters(new ReportParameter("FlagCargoFijo", FlagCargoFijo));
                rv.LocalReport.SetParameters(new ReportParameter("FlagIGV", FlagIGV));
                rv.LocalReport.SetParameters(new ReportParameter("FlagConsumoAgua", FlagConsumoAgua));
                rv.LocalReport.SetParameters(new ReportParameter("FlagConsumo", FlagConsumoSoles));
                rv.LocalReport.SetParameters(new ReportParameter("FlagCuota", FlagCuota));
                rv.LocalReport.SetParameters(new ReportParameter("FlagExtraordinaria", FlagExtraordinaria));
                rv.LocalReport.SetParameters(new ReportParameter("FlagOtros", FlagOtros));
                rv.LocalReport.SetParameters(new ReportParameter("ColumnaProInq", ColumnaProInq));
                

                Warning[] warnings;
                string[] streamids;
                string mimeType;
                string encoding;
                string filenameExtension;
                String name = "REPORTE-" + UnidadTiempo + ".pdf";
                String fileName = Server.MapPath("~/Resources") + "//" + name;
                lstNombrePDF.Add(name);

                byte[] bytes = rv.LocalReport.Render(
                    "PDF", null, out mimeType, out encoding, out filenameExtension,
                    out streamids, out warnings);
                byte[] bytesExcel = rv.LocalReport.Render(
                     "Excel", null, out mimeType, out encoding, out filenameExtension,
                     out streamids, out warnings);

                lstMemoryStreamPDF.Add(bytes);
                ExcelArchivo = bytesExcel;
                lstMemoryStream.Add(new MemoryStream(bytesExcel));
                return fileName;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public String GetReportTableAPI(List<Cuota> listaCuota, String UnidadTiempo)
        {
            try
            {
                rv.Clear();
                rv.LocalReport.DataSources.Clear();
                DSInfoReporteEdificio ds = new DSInfoReporteEdificio();

                DataRow titulo = ds.Tables["DTInfo"].NewRow();
                titulo["UnidadTiempo"] = " " + UnidadTiempo;
                //" " + UnidadTiempo + "\n\r EDIFICIO " + listaCuota.FirstOrDefault().Departamento.Edificio.Nombre;
                ds.Tables["DTInfo"].Rows.Add(titulo);
                Boolean EsDiferenteCuotatTotal = false;
                foreach (var cuota in listaCuota)
                {
                    DataRow rowDepartamento = ds.Tables["DTDepartamento"].NewRow();
                    String nombrePropietario = "SIN PROPIETARIO";
                    if (cuota.Departamento.Propietario.Count > 0)
                    {
                        Propietario p = ConstantHelpers.getTitularDepartamento(cuota.Departamento);
                        nombrePropietario = p.Nombres + " " + p.ApellidoPaterno + " " + p.ApellidoMaterno;
                    }
                    rowDepartamento["NroDepartamento"] = cuota.Departamento.Numero;
                    rowDepartamento["Propietario"] = nombrePropietario;
                    rowDepartamento["LecturaAnterior"] = cuota.LecturaAgua - cuota.ConsumoAgua;
                    rowDepartamento["LecturaActual"] = cuota.LecturaAgua;
                    rowDepartamento["ConsumoDelMes"] = cuota.ConsumoMes.ToString("#,##0.00");
                    rowDepartamento["AreaComun"] = cuota.AreaComun.ToString("#,##0.00");
                    rowDepartamento["Alcantarillado"] = cuota.Alcantarillado.ToString("#,##0.00");
                    rowDepartamento["CargoFijo"] = cuota.CargoFijo.ToString("#,##0.00");
                    rowDepartamento["IGV"] = cuota.IGV.ToString("#,##0.00");
                    rowDepartamento["ConsumoAgua"] = cuota.ConsumoAguaTotal.ToString("#,##0.00");
                    rowDepartamento["ConsumoSoles"] = cuota.ConsumoSoles.ToString("#,##0.00");
                    rowDepartamento["Cuota"] = cuota.Monto.ToString("#,##0.00");
                    //rowDepartamento["Extraordinaria"] = (cuota.CuotaExtraordinaria ?? 0).ToString("#,##0.00");
                    rowDepartamento["Extraordinaria"] = (cuota.CuotaExtraordinaria ?? 0).ToString("#,##0.00");
                    //rowDepartamento["CuotaExtraordinaria"] = (cuota.CuotaExtraordinaria ?? 0).ToString("#,##0.00");89
                    rowDepartamento["Otros"] = cuota.ConsumoIndividual.Where(x => x.Estado == ConstantHelpers.EstadoActivo).Sum(x => x.Monto).ToString("#,##0.00");
                    rowDepartamento["Total"] = (cuota.Total).ToString("#,##0.00");

                    if (cuota.Monto != cuota.Total)
                    {
                        EsDiferenteCuotatTotal = true;
                    }

                    ds.Tables["DTDepartamento"].Rows.Add(rowDepartamento);
                }
                Int32 ContColumnNoVisible = 0;
                String FlagLecturaAnterior = "T";
                String FlagLecturaActual = "T";
                String FlagConsumoDelMes = "T";
                String FlagAreaComun = "T";
                String FlagAlcantarillado = "T";
                String FlagCargoFijo = "T";
                String FlagIGV = "T";
                String FlagConsumoAgua = "T";
                String FlagConsumoSoles = "T";
                String FlagCuota = "T";
                String FlagExtraordinaria = "T";
                String FlagOtros = "T";
                String ColumnaProInq = "Propietario";
                if (listaCuota.FirstOrDefault().Departamento.Edificio.UsarInquilinoCCPD == true)
                {
                    ColumnaProInq = "Propietario o Inquilino";
                }
                DataRow rowDepartamentoTotal = ds.Tables["DTTotalesDepartamento"].NewRow();
                rowDepartamentoTotal["NroDepartamento"] = "";
                rowDepartamentoTotal["Propietario"] = "";
                rowDepartamentoTotal["LecturaAnterior"] = "";
                var OtrosSum = listaCuota.Sum(X => X.ConsumoIndividual.Where(y => y.Estado == ConstantHelpers.EstadoActivo).Sum(y => y.Monto));
                rowDepartamentoTotal["Otros"] = OtrosSum.ToString("#,##0.00");

                if (OtrosSum == 0)
                {
                    FlagOtros = "F";
                    ContColumnNoVisible++;
                }
                if (listaCuota.Sum(X => X.LecturaAgua) - listaCuota.Sum(X => X.ConsumoAgua) == 0)
                {
                    FlagLecturaAnterior = "F";
                    ContColumnNoVisible++;
                }
                rowDepartamentoTotal["LecturaActual"] = "";
                if (listaCuota.Sum(X => X.LecturaAgua) == 0)
                {
                    FlagLecturaActual = "F";
                    ContColumnNoVisible++;
                }
                rowDepartamentoTotal["ConsumoDelMes"] = listaCuota.Sum(X => X.ConsumoMes).ToString("#,##0.00");
                if (listaCuota.Sum(X => X.ConsumoMes) == 0)
                {
                    FlagConsumoDelMes = "F";
                    ContColumnNoVisible++;
                }
                rowDepartamentoTotal["AreaComun"] = listaCuota.Sum(X => X.AreaComun).ToString("#,##0.00");
                if (listaCuota.Sum(X => X.AreaComun) == 0)
                {
                    FlagAreaComun = "F";
                    ContColumnNoVisible++;
                }
                rowDepartamentoTotal["Alcantarillado"] = listaCuota.Sum(X => X.Alcantarillado).ToString("#,##0.00");
                if (listaCuota.Sum(X => X.Alcantarillado) == 0)
                {
                    FlagAlcantarillado = "F";
                    ContColumnNoVisible++;
                }
                rowDepartamentoTotal["CargoFijo"] = listaCuota.Sum(X => X.CargoFijo).ToString("#,##0.00");
                if (listaCuota.Sum(X => X.CargoFijo) == 0)
                {
                    FlagCargoFijo = "F";
                    ContColumnNoVisible++;
                }
                rowDepartamentoTotal["IGV"] = listaCuota.Sum(X => X.IGV).ToString("#,##0.00");
                if (listaCuota.Sum(X => X.IGV) == 0)
                {
                    FlagIGV = "F";
                    ContColumnNoVisible++;
                }
                rowDepartamentoTotal["ConsumoAgua"] = listaCuota.Sum(X => X.ConsumoAguaTotal).ToString("#,##0.00");
                if (listaCuota.Sum(X => X.ConsumoAguaTotal) == 0)
                {
                    FlagConsumoAgua = "F";
                    ContColumnNoVisible++;
                }
                rowDepartamentoTotal["ConsumoSoles"] = listaCuota.Sum(X => X.ConsumoSoles).ToString("#,##0.00");
                if (listaCuota.Sum(X => X.ConsumoSoles) == 0)
                {
                    FlagConsumoSoles = "F";
                    ContColumnNoVisible++;
                }
                rowDepartamentoTotal["Cuota"] = listaCuota.Sum(X => X.Monto).ToString("#,##0.00");
                if (listaCuota.Sum(X => X.Monto) == 0)
                {
                    FlagCuota = "F";
                }
                var totalExtraordinarias = (listaCuota.Sum(X => X.CuotaExtraordinaria) ?? 0);
                rowDepartamentoTotal["Extraordinaria"] = totalExtraordinarias.ToString("#,##0.00");
                if (totalExtraordinarias == 0)
                {
                    FlagExtraordinaria = "F";
                }

                if (EsDiferenteCuotatTotal == false)
                {
                    FlagCuota = "F";
                }
                rowDepartamentoTotal["Total"] = (listaCuota.Sum(X => X.Total)).ToString("#,##0.00");
                ds.Tables["DTTotalesDepartamento"].Rows.Add(rowDepartamentoTotal);

                ReportDataSource rdsInfo = new ReportDataSource("DSInfo", ds.Tables["DTInfo"].DefaultView);
                ReportDataSource rdsDepartamento = new ReportDataSource("DSInfoReporteEdificio", ds.Tables["DTDepartamento"].DefaultView);
                ReportDataSource rdsTotales = new ReportDataSource("DataSetTotales", ds.Tables["DTTotalesDepartamento"].DefaultView);
                rv.ProcessingMode = ProcessingMode.Local;
                rv.LocalReport.EnableExternalImages = true;

                if (ContColumnNoVisible >= 8)
                {
                    if (ContColumnNoVisible == 1)
                    {
                        rv.LocalReport.ReportEmbeddedResource = "VEH.Intranet.Report.ReporteEdificio3.rdlc";
                    }
                    else
                    {
                        rv.LocalReport.ReportEmbeddedResource = "VEH.Intranet.Report.ReporteEdificio2.rdlc";
                    }
                }
                else
                {
                    rv.LocalReport.ReportEmbeddedResource = "VEH.Intranet.Report.ReporteEdificio.rdlc";
                }

                rv.LocalReport.DataSources.Add(rdsInfo);
                rv.LocalReport.DataSources.Add(rdsDepartamento);
                rv.LocalReport.DataSources.Add(rdsTotales);

                var TipoInmueble = listaCuota.Count > 0 ? listaCuota[0].Departamento.TipoInmueble : null;
                rv.LocalReport.SetParameters(new ReportParameter("Edificio", "EDIFICIO " + listaCuota.FirstOrDefault().Departamento.Edificio.Nombre));

                rv.LocalReport.SetParameters(new ReportParameter("TipoInmueble", TipoInmueble.Acronimo));
                rv.LocalReport.SetParameters(new ReportParameter("TipoInmuebleNombreCompleto", TipoInmueble.Nombre));

                rv.LocalReport.SetParameters(new ReportParameter("FlagLecturaAnterior", FlagLecturaAnterior));
                rv.LocalReport.SetParameters(new ReportParameter("FlagLecturaActual", FlagLecturaActual));
                rv.LocalReport.SetParameters(new ReportParameter("FlagConsumoMes", FlagConsumoDelMes));
                rv.LocalReport.SetParameters(new ReportParameter("FlagAreaComun", FlagAreaComun));
                rv.LocalReport.SetParameters(new ReportParameter("FlagAlcantarillado", FlagAlcantarillado));
                rv.LocalReport.SetParameters(new ReportParameter("FlagCargoFijo", FlagCargoFijo));
                rv.LocalReport.SetParameters(new ReportParameter("FlagIGV", FlagIGV));
                rv.LocalReport.SetParameters(new ReportParameter("FlagConsumoAgua", FlagConsumoAgua));
                rv.LocalReport.SetParameters(new ReportParameter("FlagConsumo", FlagConsumoSoles));
                rv.LocalReport.SetParameters(new ReportParameter("FlagCuota", FlagCuota));
                rv.LocalReport.SetParameters(new ReportParameter("FlagExtraordinaria", FlagExtraordinaria));
                rv.LocalReport.SetParameters(new ReportParameter("FlagOtros", FlagOtros));
                rv.LocalReport.SetParameters(new ReportParameter("ColumnaProInq", ColumnaProInq));

                Warning[] warnings;
                string[] streamids;
                string mimeType;
                string encoding;
                string filenameExtension;
                String name = "REPORTE-" + UnidadTiempo + ".pdf";
                //String fileName = Server.MapPath("~/Resources") + "//" + name;
                lstNombrePDF.Add(name);

                byte[] bytes = rv.LocalReport.Render(
                    "PDF", null, out mimeType, out encoding, out filenameExtension,
                    out streamids, out warnings);
                byte[] bytesExcel = rv.LocalReport.Render(
                     "Excel", null, out mimeType, out encoding, out filenameExtension,
                     out streamids, out warnings);

                lstMemoryStreamPDF.Add(bytes);
                ExcelArchivo = bytesExcel;
                lstMemoryStream.Add(new MemoryStream(bytesExcel));
                return name;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public byte[] GetReportWord()
        {
            byte[] bytes = null;

            using (DocX document = DocX.Create(@"Test.docx"))
            {
                // Insert a new Paragraphs.
                Xceed.Words.NET.Paragraph p = document.InsertParagraph();

                p.Append("I am ").Append("bold").Bold()
                .Append(" and I am ")
                .Append("italic").Italic().Append(".")
                .AppendLine("I am ")
                .Append("Arial Black")
                .Font(new Xceed.Words.NET.Font("Arial Black"))
                .Append(" and I am not.")
                .AppendLine("I am ")
                .Append("BLUE").Color(Color.Blue)
                .Append(" and I am")
                .Append("Red").Color(Color.Red).Append(".");

                var ms = new MemoryStream();
                document.SaveAs(ms);
                ms.Position = 0;

                //var file = new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
                //{
                //    FileDownloadName = string.Format("test_{0}.docx", DateTime.Now.ToString("ddMMyyyyHHmmss"))
                //};

                bytes = ms.ToArray();

            }

            return bytes;
        }
        public void GetReport2()
        {
            try
            {
                Document doc = new Document(iTextSharp.text.PageSize.LETTER, 10, 10, 42, 35);

                byte[] pdfBytes;
                using (var mem = new MemoryStream())
                {
                    //using (
                    PdfWriter wri = PdfWriter.GetInstance(doc, mem);//)
                    //{
                        doc.Open();//Open Document to write
                        iTextSharp.text.Paragraph paragraph = new iTextSharp.text.Paragraph("This is my first line using Paragraph.");
                        Phrase pharse = new Phrase("This is my second line using Pharse.");
                        Chunk chunk = new Chunk(" This is my third line using Chunk.");

                        doc.Add(paragraph);

                        doc.Add(pharse);

                        doc.Add(chunk);
                        pdfBytes = mem.ToArray();
                    //}
                }

                byte[] bytes = GetReportWord();

                lstMemoryStream.Add(new MemoryStream(bytes));
                lstMemoryStreamPDF.Add(pdfBytes);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public String GetReport(Cuota cuota, DateTime fEmision, DateTime fVencimiento, Decimal PresupuestoMes, Decimal TotalM2, UnidadTiempo UnidadTiempoActualGeneral = null, List<Cuota> CuotasDelEdificio = null, UnidadTiempo lastUnidad = null, long? NumeroRecibo = null, bool EsSeparado = false)
        {
            try
            {
                rv.Clear();
                rv.LocalReport.DataSources.Clear();
                DSInfoReporte ds = new DSInfoReporte();
                DataRow rowInfo = fillInfo(ds, cuota, fEmision, fVencimiento, PresupuestoMes, TotalM2, CuotasDelEdificio, lastUnidad, UnidadTiempoActualGeneral, NumeroRecibo, EsSeparado);
                var UnidadTiempoActual = UnidadTiempoActualGeneral;
                if (UnidadTiempoActual == null)
                    UnidadTiempoActual = context.UnidadTiempo.FirstOrDefault(X => X.EsActivo);
                List<Cuota> LstDeuda;
                if (CuotasDelEdificio != null)
                    LstDeuda = CuotasDelEdificio.Where(x => x.DepartamentoId == cuota.DepartamentoId && !x.Pagado && x.CuotaId != cuota.CuotaId && x.UnidadTiempoId != UnidadTiempoActual.UnidadTiempoId && x.UnidadTiempo.Estado == ConstantHelpers.EstadoActivo).ToList();
                else
                    LstDeuda = context.Cuota.Where(x => x.DepartamentoId == cuota.DepartamentoId && !x.Pagado && x.CuotaId != cuota.CuotaId && x.UnidadTiempoId != UnidadTiempoActual.UnidadTiempoId && x.UnidadTiempo.Estado == ConstantHelpers.EstadoActivo).ToList();

                bool first = true;
                DataRow rowDeuda = ds.Tables["DTDeuda"].NewRow();

                rowDeuda["Mes"] = " ";
                rowDeuda["Anio"] = " ";
                rowDeuda["Monto"] = " ";
                //var act = context.UnidadTiempo.FirstOrDefault(x => x.EsActivo);
                LstDeuda = LstDeuda.Where(X => X.UnidadTiempoId < cuota.UnidadTiempo.UnidadTiempoId && X.UnidadTiempo.Estado == ConstantHelpers.EstadoActivo).OrderBy(X => X.UnidadTiempo.Orden).ToList();

                if (EsSeparado == false)
                foreach (Cuota c in LstDeuda)
                {
                    if (first && c.Total != 0)
                    {
                        rowDeuda["Mes"] = EsSeparado ? String.Empty : Meses[c.UnidadTiempo.Mes];
                        rowDeuda["Anio"] = EsSeparado ? String.Empty : c.UnidadTiempo.Anio.ToString();
                        rowDeuda["Monto"] = EsSeparado ? String.Empty : ("S/" + c.Total.ToString("#,##0.00"));
                        first = false;
                    }
                    else
                    {
                        if (c.Total != 0)
                        {
                            rowDeuda["Mes"] += "\n" + (EsSeparado ? String.Empty : Meses[c.UnidadTiempo.Mes]);
                            rowDeuda["Anio"] += "\n" + (EsSeparado ? String.Empty : c.UnidadTiempo.Anio.ToString());
                            rowDeuda["Monto"] += "\n" + (EsSeparado ? String.Empty : ("S/" + c.Total.ToString("#,##0.00")));
                        }
                    }
                }
                ds.Tables["DTDeuda"].Rows.Add(rowDeuda);


                ds.Tables["DTInfoReporte"].Rows.Add(rowInfo);

                ReportDataSource rdsInfo = new ReportDataSource("DTInfoReporte", ds.Tables["DTInfoReporte"].DefaultView);
                ReportDataSource rdsDeuda = new ReportDataSource("DTDeuda", ds.Tables["DTDeuda"].DefaultView);
                rv.ProcessingMode = ProcessingMode.Local;
                rv.LocalReport.EnableExternalImages = true;
                rv.LocalReport.ReportEmbeddedResource = "VEH.Intranet.Report.ReportRecibo.rdlc";
                rv.LocalReport.DataSources.Clear();
                rv.LocalReport.DataSources.Add(rdsInfo);
                rv.LocalReport.DataSources.Add(rdsDeuda);

                if (CuotasDelEdificio != null)
                {
                    var MensajeMora = (CuotasDelEdificio.Count > 0 ? CuotasDelEdificio[0].Departamento.Edificio.MensajeMora : " ") ?? " ";
                    var TipoInmueble = (CuotasDelEdificio.Count > 0 ? CuotasDelEdificio[0].Departamento.TipoInmueble.Acronimo : " ") ?? " ";
                    rv.LocalReport.SetParameters(new ReportParameter("MensajeMora", MensajeMora));
                    rv.LocalReport.SetParameters(new ReportParameter("TipoInmueble", TipoInmueble));
                }
                else
                {
                    if (cuota != null)
                    {
                        var MensajeMora = (cuota.Departamento.Edificio.MensajeMora ?? String.Empty) ?? " ";
                        var TipoInmueble = (cuota.Departamento.TipoInmueble.Acronimo ?? String.Empty ?? " ");
                        rv.LocalReport.SetParameters(new ReportParameter("MensajeMora", MensajeMora));
                        rv.LocalReport.SetParameters(new ReportParameter("TipoInmueble", TipoInmueble));
                    }
                    else
                    {
                        rv.LocalReport.SetParameters(new ReportParameter("MensajeMora", " "));
                        rv.LocalReport.SetParameters(new ReportParameter("TipoInmueble", " "));
                    }


                }
                rv.LocalReport.SetParameters(new ReportParameter("NombreRecibo", cuota.Departamento.NombreRecibo ?? "-"));

                Warning[] warnings;
                string[] streamids;
                string mimeType;
                string encoding;
                string filenameExtension;
                string valorCE = EsSeparado ? "CE-" : String.Empty;
                String nameDOC = valorCE + cuota.Departamento.TipoInmueble.Acronimo + "-" + cuota.Departamento.Numero + "-" + cuota.UnidadTiempo.Descripcion + ".doc";
                lstNombreDOC.Add(nameDOC);

                String namePDF = valorCE + cuota.Departamento.TipoInmueble.Acronimo + "-" + cuota.Departamento.Numero + "-" + cuota.UnidadTiempo.Descripcion + ".pdf";
                lstNombrePDF.Add(namePDF);

                String fileName = Server.MapPath("~/Resources") + "//" + nameDOC;

                byte[] bytes = rv.LocalReport.Render(
                    "WORDOPENXML", null, out mimeType, out encoding, out filenameExtension,
                    out streamids, out warnings);

                Warning[] warnings2;
                string[] streamids2;
                string mimeType2;
                string encoding2;
                string filenameExtension2;

                byte[] bytesPDF = rv.LocalReport.Render(
                    "PDF", null, out mimeType2, out encoding2, out filenameExtension2,
                    out streamids2, out warnings2);

                lstMemoryStream.Add(new MemoryStream(bytes));

                lstMemoryStreamPDF.Add(bytesPDF);

                /*using (FileStream fs = new FileStream(fileName, FileMode.Create))
                {
                    fs.Write(bytes, 0, bytes.Length);
                }*/


                return fileName;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public String GetReportAPI(Cuota cuota, DateTime fEmision, DateTime fVencimiento, Decimal PresupuestoMes, Decimal TotalM2, UnidadTiempo UnidadTiempoActualGeneral = null, List<Cuota> CuotasDelEdificio = null, UnidadTiempo lastUnidad = null, long? NumeroRecibo = null, bool EsSeparado = false)
        {
            try
            {
                rv.Clear();
                rv.LocalReport.DataSources.Clear();
                DSInfoReporte ds = new DSInfoReporte();
                DataRow rowInfo = fillInfo(ds, cuota, fEmision, fVencimiento, PresupuestoMes, TotalM2, CuotasDelEdificio, lastUnidad, UnidadTiempoActualGeneral, NumeroRecibo, EsSeparado);
                var UnidadTiempoActual = UnidadTiempoActualGeneral;
                if (UnidadTiempoActual == null)
                    UnidadTiempoActual = context.UnidadTiempo.FirstOrDefault(X => X.EsActivo);
                List<Cuota> LstDeuda;
                if (CuotasDelEdificio != null)
                    LstDeuda = CuotasDelEdificio.Where(x => x.DepartamentoId == cuota.DepartamentoId && !x.Pagado && x.CuotaId != cuota.CuotaId && x.UnidadTiempoId != UnidadTiempoActual.UnidadTiempoId && x.UnidadTiempo.Estado == ConstantHelpers.EstadoActivo).ToList();
                else
                    LstDeuda = context.Cuota.Where(x => x.DepartamentoId == cuota.DepartamentoId && !x.Pagado && x.CuotaId != cuota.CuotaId && x.UnidadTiempoId != UnidadTiempoActual.UnidadTiempoId && x.UnidadTiempo.Estado == ConstantHelpers.EstadoActivo).ToList();

                bool first = true;
                DataRow rowDeuda = ds.Tables["DTDeuda"].NewRow();

                rowDeuda["Mes"] = " ";
                rowDeuda["Anio"] = " ";
                rowDeuda["Monto"] = " ";
                var act = context.UnidadTiempo.FirstOrDefault(x => x.EsActivo);
                LstDeuda = LstDeuda.Where(X => X.UnidadTiempoId < cuota.UnidadTiempo.UnidadTiempoId && X.UnidadTiempo.Estado == ConstantHelpers.EstadoActivo).OrderBy(X => X.UnidadTiempo.Orden).ToList();
                foreach (Cuota c in LstDeuda)
                {
                    if (first && c.Total != 0)
                    {
                        rowDeuda["Mes"] = EsSeparado ? String.Empty : Meses[c.UnidadTiempo.Mes];
                        rowDeuda["Anio"] = EsSeparado ? String.Empty : c.UnidadTiempo.Anio.ToString();
                        rowDeuda["Monto"] = EsSeparado ? String.Empty : ("S/" + c.Total.ToString("#,##0.00"));
                        first = false;
                    }
                    else
                    {
                        if (c.Total != 0)
                        {
                            rowDeuda["Mes"] += "\n" + (EsSeparado ? String.Empty : Meses[c.UnidadTiempo.Mes]);
                            rowDeuda["Anio"] += "\n" + (EsSeparado ? String.Empty : c.UnidadTiempo.Anio.ToString());
                            rowDeuda["Monto"] += "\n" + (EsSeparado ? String.Empty : ("S/" + c.Total.ToString("#,##0.00")));
                        }
                    }
                }
                ds.Tables["DTDeuda"].Rows.Add(rowDeuda);


                ds.Tables["DTInfoReporte"].Rows.Add(rowInfo);

                ReportDataSource rdsInfo = new ReportDataSource("DTInfoReporte", ds.Tables["DTInfoReporte"].DefaultView);
                ReportDataSource rdsDeuda = new ReportDataSource("DTDeuda", ds.Tables["DTDeuda"].DefaultView);
                rv.ProcessingMode = ProcessingMode.Local;
                rv.LocalReport.EnableExternalImages = true;
                rv.LocalReport.ReportEmbeddedResource = "VEH.Intranet.Report.ReportRecibo.rdlc";
                rv.LocalReport.DataSources.Clear();
                rv.LocalReport.DataSources.Add(rdsInfo);
                rv.LocalReport.DataSources.Add(rdsDeuda);

                if (CuotasDelEdificio != null)
                {
                    var MensajeMora = (CuotasDelEdificio.Count > 0 ? CuotasDelEdificio[0].Departamento.Edificio.MensajeMora : " ") ?? " ";
                    var TipoInmueble = (CuotasDelEdificio.Count > 0 ? CuotasDelEdificio[0].Departamento.TipoInmueble.Acronimo : " ") ?? " ";
                    rv.LocalReport.SetParameters(new ReportParameter("MensajeMora", MensajeMora));
                    rv.LocalReport.SetParameters(new ReportParameter("TipoInmueble", TipoInmueble));
                }
                else
                {
                    if (cuota != null)
                    {
                        var MensajeMora = (cuota.Departamento.Edificio.MensajeMora ?? String.Empty) ?? " ";
                        var TipoInmueble = (cuota.Departamento.TipoInmueble.Acronimo ?? String.Empty ?? " ");
                        rv.LocalReport.SetParameters(new ReportParameter("MensajeMora", MensajeMora));
                        rv.LocalReport.SetParameters(new ReportParameter("TipoInmueble", TipoInmueble));
                    }
                    else
                    {
                        rv.LocalReport.SetParameters(new ReportParameter("MensajeMora", " "));
                        rv.LocalReport.SetParameters(new ReportParameter("TipoInmueble", " "));
                    }


                }
                rv.LocalReport.SetParameters(new ReportParameter("NombreRecibo", cuota.Departamento.NombreRecibo ?? "-"));

                Warning[] warnings;
                string[] streamids;
                string mimeType;
                string encoding;
                string filenameExtension;
                string valorCE = EsSeparado ? "CE-" : String.Empty;
                String nameDOC = valorCE + cuota.Departamento.TipoInmueble.Acronimo + "-" + cuota.Departamento.Numero + "-" + cuota.UnidadTiempo.Descripcion + ".doc";
                lstNombreDOC.Add(nameDOC);

                String namePDF = valorCE + cuota.Departamento.TipoInmueble.Acronimo + "-" + cuota.Departamento.Numero + "-" + cuota.UnidadTiempo.Descripcion + ".pdf";
                lstNombrePDF.Add(namePDF);

                //String fileName = Server.MapPath("~/Resources") + "//" + nameDOC;

                byte[] bytes = rv.LocalReport.Render(
                    "Word", null, out mimeType, out encoding, out filenameExtension,
                    out streamids, out warnings);

                Warning[] warnings2;
                string[] streamids2;
                string mimeType2;
                string encoding2;
                string filenameExtension2;

                byte[] bytesPDF = rv.LocalReport.Render(
                    "PDF", null, out mimeType2, out encoding2, out filenameExtension2,
                    out streamids2, out warnings2);

                lstMemoryStream.Add(new MemoryStream(bytes));

                lstMemoryStreamPDF.Add(bytesPDF);

                /*using (FileStream fs = new FileStream(fileName, FileMode.Create))
                {
                    fs.Write(bytes, 0, bytes.Length);
                }*/
                return namePDF;


            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private DataRow fillInfo(DSInfoReporte ds, Cuota cuota, DateTime fEmision, DateTime fVencimiento, Decimal presupuestoMes, Decimal TotalM2, List<Cuota> cuotasDelEdificio = null, UnidadTiempo lastUnidad = null, UnidadTiempo UnidadVista = null, long? NumeroRecibo = null, bool EsSeparado = false)
        {
            try
            {
                DataRow rowInfo = ds.Tables["DTInfoReporte"].NewRow();
                List<Propietario> lstPropietario = cuota.Departamento.Propietario.ToList();
                String NombrePropietario = "SIN PROPIETARIOS";
                String RUC = String.Empty;
                Boolean TieneRazonSocial = false;
                Boolean MostrarRUC = false;
                if (lstPropietario.Count > 0)
                {
                    Propietario objPropietario = ConstantHelpers.getTitularDepartamento(cuota.Departamento);
                    if (cuota.Departamento.NombrePropietario)
                    {
                        NombrePropietario = String.IsNullOrEmpty(objPropietario.RazonSocial) ?
    objPropietario.Nombres + " " + objPropietario.ApellidoPaterno + " " + objPropietario.ApellidoMaterno
    : objPropietario.RazonSocial;

                        if (!String.IsNullOrEmpty(objPropietario.RazonSocial))
                            TieneRazonSocial = true;
                        MostrarRUC = objPropietario.MostrarRUC.Value;
                        RUC = objPropietario.RUC;
                    }
                    else
                    {
                        
                        var inquilino = objPropietario.Inquilino.FirstOrDefault(x => x.Estado == ConstantHelpers.EstadoActivo);//context.Inquilino.FirstOrDefault(x => x.PropietarioId == objPropietario.PropietarioId && x.Estado == ConstantHelpers.EstadoActivo);
                        if (inquilino != null)
                        {
                            NombrePropietario = inquilino.Nombres;


                            if (!String.IsNullOrEmpty(inquilino.RazonSocial))
                                TieneRazonSocial = true;
                            MostrarRUC = inquilino.MostrarRUC.Value;
                            RUC = inquilino.RUC;

                        }
                        else
                        {
                            NombrePropietario = String.IsNullOrEmpty(objPropietario.RazonSocial) ?
    objPropietario.Nombres + " " + objPropietario.ApellidoPaterno + " " + objPropietario.ApellidoMaterno
    : objPropietario.RazonSocial;
                        }
                    }

                    //if (!String.IsNullOrEmpty(objPropietario.RazonSocial))
                    //    TieneRazonSocial = true;
                    //MostrarRUC = objPropietario.MostrarRUC.Value;
                    //RUC = objPropietario.RUC;
                }
                Decimal parcialM2 = cuota.Departamento.DepositoM2 ?? 0 + cuota.Departamento.DepartamentoM2 ?? 0 + cuota.Departamento.EstacionamientoM2 ?? 0;
                Edificio edificio = cuota.Departamento.Edificio;
                var pDistribucion = parcialM2 / Math.Max(TotalM2, 1);
                //UnidadTiempo lastUnidad = AnteriorUnidadTiempo(cuota.UnidadTiempo);
                if (lastUnidad == null)
                    lastUnidad = context.UnidadTiempo.FirstOrDefault(x => x.Orden == cuota.UnidadTiempo.Orden - 1);
                var lecturaAnterior = "-";
                var DeudaAnterior = "";
                var TotalDeuda = "";


                if (lastUnidad != null)
                {
                    if (cuotasDelEdificio == null)
                        cuotasDelEdificio = context.Cuota.Where(X => X.Departamento.EdificioId == cuota.Departamento.EdificioId && X.UnidadTiempo.Estado == ConstantHelpers.EstadoActivo).ToList();

                    lecturaAnterior = (Math.Truncate(cuota.LecturaAgua * 100) / 100).ToString();

                }
                //var UnidadTiempoActual = context.UnidadTiempo.FirstOrDefault(X => X.EsActivo == true);
                //var LstDeuda = context.Cuota.Where(x => x.DepartamentoId == cuota.DepartamentoId && !x.Pagado
                //&& x.CuotaId != cuota.CuotaId && x.UnidadTiempoId != UnidadTiempoActualId
                //&& x.UnidadTiempo.Estado == ConstantHelpers.EstadoActivo
                //&& x.UnidadTiempoId <= UnidadTiempoActualId && x.UnidadTiempoId != cuota.UnidadTiempoId).ToList();

                var LstDeuda = cuotasDelEdificio.Where(x => x.DepartamentoId == cuota.DepartamentoId && !x.Pagado
                && x.CuotaId != cuota.CuotaId && x.UnidadTiempoId != UnidadTiempoActualId
                && x.UnidadTiempo.Estado == ConstantHelpers.EstadoActivo
                && x.UnidadTiempoId <= UnidadTiempoActualId && x.UnidadTiempoId != cuota.UnidadTiempoId).ToList();

                TotalDeuda = DeudaAnterior = LstDeuda.Sum(X => X.Total).ToString("#,##0.00");
                if (TotalDeuda == "0.00")
                    TotalDeuda = "";

                if (this.CantidadReporte == 0)
                    this.CantidadReporte = edificio.CantidadReporte;
                String valorAuxiliarCE = EsSeparado ? "CE" : String.Empty;
                rowInfo["CodigoDeposito"] = valorAuxiliarCE + cuota.Departamento.Numero;
                rowInfo["ReciboMantenimiento"] = valorAuxiliarCE + AddCerosToLeft(edificio.Identificador, 3) + "-" + NumeroRecibo.ToString().PadLeft(6, '0');//AddCerosToLeft(this.CantidadReporte + (edificio.DesfaseRecibos.HasValue ? edificio.DesfaseRecibos.Value : 0), 6);
                rowInfo["Periodo"] = cuota.UnidadTiempo.Descripcion;
                rowInfo["CodigoPago"] = valorAuxiliarCE + cuota.Departamento.Numero;
                rowInfo["FechaEmision"] = fEmision.ToShortDateString();
                rowInfo["FechaVencimiento"] = fVencimiento.ToShortDateString();
                rowInfo["CuotaMes"] = EsSeparado ? "0.00" : cuota.Monto.ToString("#,###.00");
                try
                {
                    //if (cuota.FechaVencimiento.HasValue && cuota.FechaPagado.HasValue
                    //    && cuota.FechaVencimiento.Value.Month == cuota.FechaPagado.Value.Month)
                    //{
                    rowInfo["CuotaExtraordinaria"] = (cuota.CuotaExtraordinaria ?? 0);//.ToString("#,###.00");
                    //}
                }
                catch (Exception ex)
                {

                }

                var lstconsumos = cuota.ConsumoIndividual.Where(x => x.Estado == ConstantHelpers.EstadoActivo).ToList();
                //Check
                //rowInfo["ConsumosIndividuales"] = (EsSeparado ? 0 : cuota.ConsumoAguaTotal); //+ (cuota.ConsumoIndividual.Where( ).Sum( x => x.Monto));
                //

                rowInfo["TotalMesActual"] = (EsSeparado ? cuota.CuotaExtraordinaria.Value.ToString("#,###.00") : cuota.Total.ToString("#,###.00"));
                rowInfo["DeudaAnterior"] = EsSeparado ? "" : DeudaAnterior;
                rowInfo["TotalAcumulado"] = EsSeparado ? cuota.CuotaExtraordinaria.Value.ToString("#,###.00") : ((cuota.Total + Convert.ToDecimal("0" + DeudaAnterior)).ToString("#,###.00"));
                rowInfo["TotalDeuda"] = TotalDeuda;
                rowInfo["TotalConsumoIndividual"] = EsSeparado ? 0 : (cuota.ConsumoAguaTotal + lstconsumos.Sum(x => x.Monto));
                var areatotalcalculada = ((cuota.Departamento.DepartamentoM2 ?? 0) + (cuota.Departamento.EstacionamientoM2 ?? 0) + (cuota.Departamento.DepositoM2 ?? 0));
                rowInfo["TotalM2"] = EsSeparado ? "" : (cuota.Departamento.TotalM2.HasValue ? cuota.Departamento.TotalM2.Value.ToString() : areatotalcalculada == 0 ? " " : areatotalcalculada.ToString());
                rowInfo["PresupuestoMes"] = EsSeparado ? "" : (cuota.Departamento.Edificio.PresupuestoMensual.HasValue ? cuota.Departamento.Edificio.PresupuestoMensual.Value.ToString("#,##0.00") : presupuestoMes.ToString("#,##0.00"));
                rowInfo["PDistribucion"] = EsSeparado ? "" : (cuota.Departamento.PDistribucion.HasValue ? cuota.Departamento.PDistribucion.Value.ToString("###0.00") + " %" : "");
                rowInfo["CuotaMes2"] = EsSeparado ? cuota.CuotaExtraordinaria.Value.ToString("#,###.00") : cuota.Total.ToString("#,###.00");
                rowInfo["NombreTitulo"] = "JUNTA DE PROPIETARIOS DEL EDIFICIO " + cuota.Departamento.Edificio.Nombre;
                rowInfo["Direccion"] = cuota.Departamento.Edificio.Direccion;
                rowInfo["NombreLateral"] = cuota.Departamento.Edificio.Nombre;
                rowInfo["DetalleDescripcion"] = EsSeparado ? (("CUOTA EXTRAORDINARIA-") + cuota.UnidadTiempo.Descripcion) : (("CUOTA DE MANTENIMIENTO-") + cuota.UnidadTiempo.Descripcion);
                rowInfo["MontoDetalle"] = EsSeparado ? cuota.CuotaExtraordinaria.Value.ToString("#,###.00") : cuota.Total.ToString("#,###.00");
                rowInfo["TotalDetalle"] = EsSeparado ? cuota.CuotaExtraordinaria.Value.ToString("#,###.00") : cuota.Total.ToString("#,###.00");
                rowInfo["TotalLetraDetalle"] = EsSeparado ? TextNumber((double)cuota.CuotaExtraordinaria) : TextNumber((double)cuota.Total);

                var conceptoString = EsSeparado ? "" : "AGUA";
                var lecturaAnteriorString = EsSeparado ? "0" : (cuota.LecturaAgua - cuota.ConsumoAgua).ToString();
                var lecturaActualString = EsSeparado ? "0" : (cuota.LecturaAgua).ToString();
                var consumoMesString = EsSeparado ? "0" : (cuota.ConsumoAgua).ToString();
                var consumosIndividualesString = ((EsSeparado ? 0 : cuota.ConsumoAguaTotal)).ToString("#,###.00");
                if(EsSeparado == false)
                    foreach (var item in lstconsumos)
                    {
                        conceptoString += "\n" + item.Detalle.ToUpper();
                        lecturaAnteriorString += "\n";
                        lecturaActualString += "\n";
                        consumoMesString += "\n";
                        consumosIndividualesString += "\nS/" + item.Monto.ToString("#,###.00");
                    }
                rowInfo["ConsumosIndividuales"] = consumosIndividualesString;
                rowInfo["Concepto"] = conceptoString;//EsSeparado ? "" : "AGUA";
                rowInfo["LecturaAnterior"] = lecturaAnteriorString;//EsSeparado ? "" : (cuota.LecturaAgua - cuota.ConsumoAgua).ToString();
                rowInfo["LecturaActual"] = lecturaActualString;//EsSeparado ? "" : (cuota.LecturaAgua).ToString();
                rowInfo["ConsumoMes"] = consumoMesString;//EsSeparado ? "" : (cuota.ConsumoAgua).ToString();

                rowInfo["Total"] = EsSeparado ? 0 : cuota.Total;

                rowInfo["NroUnidad"] = EsSeparado ? "" : ((cuota.Departamento.Numero.ToString()) + (!(String.IsNullOrEmpty(cuota.Departamento.Estacionamiento)) ? "\n" + cuota.Departamento.Estacionamiento : "") + (!(String.IsNullOrEmpty(cuota.Departamento.Deposito)) ? "\n" + cuota.Departamento.Deposito.ToString() : ""));

                var tipoInmueble = cuota.Departamento.TipoInmueble;// context.TipoInmueble.FirstOrDefault(x => x.TipoInmuebleId == cuota.Departamento.TipoInmuebleId);
                rowInfo["Propietario"] = (TieneRazonSocial ? "RAZON SOCIAL: " : "NOMBRE: ") + NombrePropietario + "\n"
                    + tipoInmueble.Nombre + ": " + cuota.Departamento.Numero + "\n" + (MostrarRUC ? "RUC: " + RUC : String.Empty);
                rowInfo["PagoVentanilla"] = "INDICAR AL CAJERO QUE PAGARA SU CUOTA DE " + cuota.Departamento.Edificio.NombrePago + "\r\nINDIQUE SU CODIGO DE DEPOSITO";
                rowInfo["PagoInternet"] = "INGRESE A BCP - PAGO DE SERVICIOS - EMPRESAS DIVERSAS - " + cuota.Departamento.Edificio.NombrePago + "-" + cuota.Departamento.Edificio.Representante + " - CODIGO DE DEPOSITO - PAGAR. CONSULTAS: A nuestro correo informes@afari.pe o a nuestro teléfono 2246251 .";
                rowInfo["M2Unidad"] = EsSeparado ? "" : noZero(cuota.Departamento.DepartamentoM2) +
                    (!(String.IsNullOrEmpty(cuota.Departamento.Estacionamiento)) ? "\n" + cuota.Departamento.EstacionamientoM2.ToString() : "") +
                    (!(String.IsNullOrEmpty(cuota.Departamento.Deposito)) ? "\n" + cuota.Departamento.DepositoM2.ToString() : "") +
                    "\n" + rowInfo["TotalM2"];
                rowInfo["TipoUnidad"] = EsSeparado ? "" : tipoInmueble.Acronimo +
                  (!(String.IsNullOrEmpty(cuota.Departamento.Estacionamiento)) ? "\nESTAC." : "") +
                  (!(String.IsNullOrEmpty(cuota.Departamento.Deposito)) ? "\nDEPO." : "") +
                "\nTOTAL.";
                rowInfo["UnidadEstacionamiento"] = "Estacionamiento";
                rowInfo["NroEstacionamiento"] = EsSeparado ? "" : cuota.Departamento.Estacionamiento;
                rowInfo["UnidadDeposito"] = "Depósito";
                rowInfo["NroDeposito"] = EsSeparado ? "" : cuota.Departamento.Deposito;


                if (edificio.UnidadTiempo != null && (edificio.UnidadTiempo.Orden < UnidadVista.Orden))
                {
                    edificio.CantidadReporte += 1;
                    edificio.ReciboUnidadTiempoId = cuota.UnidadTiempoId;
                    context.Entry(edificio).State = System.Data.Entity.EntityState.Modified;
                }
                else if (edificio.UnidadTiempo == null)
                {
                    edificio.CantidadReporte += 1;
                    edificio.ReciboUnidadTiempoId = cuota.UnidadTiempoId;
                    context.Entry(edificio).State = System.Data.Entity.EntityState.Modified;
                }
                this.CantidadReporte++;
                //Innecesario a cada momento
                //  context.SaveChanges();
                return rowInfo;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public String noZero(Decimal a)
        {
            if (a == 0)

                return "";
            return a.ToString();
        }
        public String noZero(Decimal? a)
        {
            if (!a.HasValue)
                return "";
            if (a.Value == 0)
                return "";
            return a.Value.ToString();
        }
        public Decimal GetSaldoHasta(CargarDatosContext datacontext, UnidadTiempo t, Int32 EdificioId)
        {
            var ctx = datacontext.context;
            Decimal Total = 0;
            Int32 Orden = t.Orden ?? 0;
            Total += ctx.DetalleGasto.Where(X => X.Gasto.EdificioId == EdificioId && X.Gasto.UnidadTiempo.Orden < Orden && X.Pagado && X.Estado == ConstantHelpers.EstadoActivo).ToList().Sum(X => X.Monto);
            Total += ctx.DetalleIngreso.Where(X => X.Ingreso.EdificioId == EdificioId && X.Ingreso.UnidadTiempo.Orden < Orden && X.Pagado && X.Estado == ConstantHelpers.EstadoActivo).ToList().Sum(X => X.Monto);
            Total += ctx.Cuota.Where(X => X.Departamento.EdificioId == EdificioId && X.UnidadTiempo.Orden < Orden && X.Pagado && X.Estado == ConstantHelpers.EstadoActivo).ToList().Sum(X => X.Total);
            return Total;
        }
        private DataRow fillDeuda2(DSInfoReporte ds, Cuota cuota)
        {
            try
            {
                DataRow rowDeuda = ds.Tables["DTDeuda"].NewRow();
                rowDeuda["Mes"] = "DICIEMBRE";
                rowDeuda["Anio"] = "2014";
                rowDeuda["Monto"] = "S/ 300.00";
                return rowDeuda;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private void DeleteFile(String filePath)
        {
            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private String TextNumber(Double num)
        {
            String NumeroTexto = String.Empty;
            Int32 centimos = Convert.ToInt32(num * 100) % 100;
            ConvertNumberToText(Convert.ToInt32(Math.Floor(num)), out NumeroTexto);


            return "SON: " + NumeroTexto.ToUpper() + " CON " + centimos.ToString("0#") + "/100 SOLES";
        }
        private bool HelperConvertNumberToText(int num, out string buf)
        {

            string[] strones = {

            "Uno", "Dos", "Tres", "Cuatro", "Cinco", "Seis", "Siete", "Ocho",

            "Nueve", "Diez", "Once", "Doce", "Trece", "Catorce",

            "Quince", "Dieciseis", "Diecisiete", "Dieciocho", "Diecinueve", "Veinte",

            "Veintiuno", "Veintidos", "Veintitres", "Veinticuatro", "Veinticinco", "Veinticeis",

            "Veintisiete", "Veintiocho", "Veintinueve"

          };



            string[] strtens = {

              "Diez", "Veinte", "Treinta", "Cuarenta", "Cincuenta", "Sesenta",

              "Setenta", "Ochenta", "Noventa", "Cien"

          };

            string[] strhunds = {

              "Cien", "Doscientos", "Trescientos", "Cuatrocientos", "Quinientos", "Seiscientos",

              "Setecientos", "Ochocientos", "Novecientos"

          };


            int numAux = num;

            string result = "";

            buf = "";

            int single, tens, hundreds;



            if (num > 1000)

                return false;



            hundreds = num / 100;

            num = num - hundreds * 100;

            if (num < 30)
            {

                tens = 0; // special case

                single = num;

            }

            else
            {

                tens = num / 10;

                num = num - tens * 10;

                single = num;

            }



            result = "";



            if (hundreds > 0)
            {

                result += strhunds[hundreds - 1];
                if (numAux < 199)
                {
                    if (numAux == 100)
                        result += " ";
                    else if (numAux > 100)
                        result += "to ";
                }

                else
                    result += " ";

            }

            if (tens > 0)
            {

                result += strtens[tens - 1];

                if (num % 10 == 0)
                    result += " ";
                else
                    result += " y ";

            }

            if (single > 0)
            {

                result += strones[single - 1];

                result += " ";

            }



            buf = result;

            return true;

        }
        private String ConvertNumberToText(int num, out string result)
        {
            int numAux = num;

            string tempString = "";

            int thousands;

            int temp;

            result = "";

            if (num < 0 || num > 100000)
            {

                return num + " No Soportado";
            }



            if (num == 0)
                return num + " \tZero";


            if (num < 1000)
            {
                HelperConvertNumberToText(num, out tempString);
                result += tempString;
            }

            else
            {
                thousands = num / 1000;
                temp = num - thousands * 1000;
                HelperConvertNumberToText(thousands, out tempString);
                if (numAux < 2000)
                {
                    result += "Mil ";
                }
                else
                {
                    result += tempString;
                    result += "Mil ";
                }


                HelperConvertNumberToText(temp, out tempString);
                result += tempString;
            }

            return result.ToUpper();
        }

        private String AddCerosToLeft(int id, int ceros)
        {
            String res = "";
            for (int i = 0; i < ceros; ++i) res += "0";
            res += id;
            return res.Substring(res.Length - ceros, ceros);
        }

        private UnidadTiempo AnteriorUnidadTiempo(UnidadTiempo unidad)
        {
            try
            {
                int mes = unidad.Mes - 1;
                int anio = unidad.Anio;
                if (mes == 0)
                {
                    mes = 12;
                    anio--;
                }
                UnidadTiempo objUnidad = context.UnidadTiempo.FirstOrDefault(x => x.Mes == mes && x.Anio == anio && x.Estado.Equals(ConstantHelpers.EstadoActivo));
                return objUnidad;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private MemoryStream getReportAtPos(int pos)
        {
            if (lstMemoryStreamPDF.Count <= pos) return null;
            return new MemoryStream(lstMemoryStreamPDF[pos]);
        }

        public MemoryStream getFirstReport()
        {
            return getReportAtPos(0);
        }

        public MemoryStream getLastReport()
        {
            return getReportAtPos(lstMemoryStreamPDF.Count - 1);
        }
        public MemoryStream GetReportRelacionPropietario(Int32 EdificioId, String NombreEdificio)
        {
            // byte[] bytes = (byte)0;
            try
            {
                rv.Clear();
                rv.LocalReport.DataSources.Clear();
                DSInfoReporte ds = new DSInfoReporte();

                var LstPropietario = context.Propietario
                    .Include(x => x.Departamento)
                    .Include(x => x.Departamento.Edificio)
                    .Where(x => x.Departamento.EdificioId == EdificioId && x.Estado == ConstantHelpers.EstadoActivo)
                    .OrderBy(x => x.DepartamentoId).ToList();

                var dpto = String.Empty;
                foreach (var item in LstPropietario)
                {
                    DataRow rowPropietarios = ds.Tables["DTPropietario"].NewRow();
                    //if(dpto != item.Departamento.Numero)
                    rowPropietarios["DepartamentoId"] = item.DepartamentoId;
                    rowPropietarios["Departamento"] = item.Departamento.TipoInmueble.Acronimo + " " + item.Departamento.Numero;
                    //else
                    //rowPropietarios["Departamento"] = "1";
                    //= IIF(First(Fields!Departamento.Value, "DSInfoReporte") = "1", "White", "LightGrey")
                    rowPropietarios["Nombres"] = item.Nombres;
                    //rowPropietarios["NroDocumento"] = item.NroDocumento;
                    rowPropietarios["Celular"] = item.Celular + "\n" + item.Telefono;
                    rowPropietarios["Email"] = item.Email;
                    rowPropietarios["Contacto"] = item.Contacto;
                    rowPropietarios["Estacionamiento"] = item.Departamento.Estacionamiento;
                    rowPropietarios["Deposito"] = item.Departamento.Deposito;
                    var inq = item.Inquilino.FirstOrDefault( x => x.Estado == ConstantHelpers.EstadoActivo);
                    rowPropietarios["InquilinoNombres"] = inq != null ? inq.Nombres : String.Empty;
                    rowPropietarios["InquilinoNroDocumento"] = inq != null ? inq.Dni : String.Empty;
                    rowPropietarios["InquilinoCelular"] = inq != null ? inq.Celular + "\n" + inq.Telefono : String.Empty;
                    rowPropietarios["InquilinoEmail"] = inq != null ? inq.Email : String.Empty;
                    rowPropietarios["InquilinoContacto"] = inq != null ? inq.Contacto : String.Empty;

                    ds.Tables["DTPropietario"].Rows.Add(rowPropietarios);
                    dpto = item.Departamento.Numero;
                }

                ReportDataSource rdsPropietario = new ReportDataSource("DSInfoReporte", ds.Tables["DTPropietario"].DefaultView);

                rv.ProcessingMode = ProcessingMode.Local;
                rv.LocalReport.EnableExternalImages = true;
                rv.LocalReport.ReportEmbeddedResource = "VEH.Intranet.Report.ReportePropietarioInquilino.rdlc";
                rv.LocalReport.DataSources.Add(rdsPropietario);
                rv.LocalReport.SetParameters(new ReportParameter("Edificio", NombreEdificio));
                rv.LocalReport.SetParameters(new ReportParameter("Fecha", DateTime.Now.ToShortDateString()));

                Warning[] warnings;
                string[] streamids;
                string mimeType;
                string encoding;
                string filenameExtension;

                byte[] bytes = rv.LocalReport.Render(
                    "PDF", null, out mimeType, out encoding, out filenameExtension,
                    out streamids, out warnings);

                Warning[] warnings_excel;
                string[] streamids_excel;
                string mimeType_excel;
                string encoding_excel;
                string filenameExtension_excel;

                byte[] bytes_excel = rv.LocalReport.Render(
                    "Excel", null, out mimeType_excel, out encoding_excel, out filenameExtension_excel,
                    out streamids_excel, out warnings_excel);


                String fileName = Server.MapPath("~/Resources") + "\\Relacion_Propietarios.zip";
                MemoryStream outputMemStream = new MemoryStream();
                ZipOutputStream zipStream = new ZipOutputStream(outputMemStream);

                zipStream.SetLevel(3); //0-9, 9 being the highest level of compression

                ZipEntry entry_pdf = new ZipEntry("RelacionPropietarios.pdf");
                entry_pdf.DateTime = DateTime.Now;
                zipStream.PutNextEntry(entry_pdf);
                StreamUtils.Copy(new MemoryStream(bytes), zipStream, new byte[4096]);
                zipStream.CloseEntry();

                ZipEntry entry_excel = new ZipEntry("RelacionPropietarios.xls");
                entry_excel.DateTime = DateTime.Now;
                zipStream.PutNextEntry(entry_excel);
                StreamUtils.Copy(new MemoryStream(bytes_excel), zipStream, new byte[4096]);
                zipStream.CloseEntry();

                zipStream.IsStreamOwner = false;
                zipStream.Close();

                outputMemStream.Position = 0;

                return outputMemStream;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}