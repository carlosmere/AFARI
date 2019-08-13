using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.Filters;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.CorretajeInmobiliario;

namespace VEH.Intranet.Controllers
{
    [AppAuthorize(AppRol.Administrador)]
    public class CorretajeInmobiliarioController : BaseController
    {
        // GET: CorretajeInmobiliario
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult LstVisitas(Int32? np, String Fecha, String NombreCliente)
        {
            var model = new LstVisitasViewModel();
            model.Fill(CargarDatosContext(), np, Fecha, NombreCliente);
            return View(model);
        }
        public ActionResult AddEditVisita(Int32? VisitaCorretajeId)
        {
            var model = new AddEditVisitaViewModel();
            model.Fill(CargarDatosContext(), VisitaCorretajeId);
            return View(model);
        }
        [HttpPost]
        public ActionResult AddEditVisita(AddEditVisitaViewModel model)
        {
            try
            {
                VisitaCorretaje visita = null;
                if (model.VisitaCorretajeId.HasValue)
                {
                    visita = context.VisitaCorretaje.FirstOrDefault(x => x.VisitaCorretajeId == model.VisitaCorretajeId);
                }
                else
                {
                    visita = new VisitaCorretaje();
                    visita.Estado = ConstantHelpers.EstadoActivo;
                    context.VisitaCorretaje.Add(visita);
                }

                visita.Cliente = model.Cliente;
                visita.Direccion = model.Direccion;
                visita.Tipo = model.Tipo;
                visita.Precio = model.Precio;
                visita.Moneda = model.Moneda;
                visita.NombreCliente = model.NombreCliente;
                visita.Fecha = model.Fecha.ToDateTime();
                var arrHora = model.Hora.Split(':');
                visita.Hora = new TimeSpan(arrHora[0].ToInteger(), arrHora[1].ToInteger(), 0);
                visita.Firma = model.Firma;

                context.SaveChanges();
                PostMessage(MessageType.Success);
                return RedirectToAction("LstVisitas");
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error);

                model.Fill(CargarDatosContext(), model.VisitaCorretajeId);
                TryUpdateModel(model);
                return View(model);
            }
        }
        public ActionResult DeleteVisita(Int32? VisitaCorretajeId)
        {
            try
            {
                var visita = context.VisitaCorretaje.FirstOrDefault(x => x.VisitaCorretajeId == VisitaCorretajeId);
                visita.Estado = ConstantHelpers.EstadoInactivo;
                context.SaveChanges();

                PostMessage(MessageType.Success);
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error);
            }

            return RedirectToAction("LstVisitas");
        }
        public ActionResult DescargarVisita(Int32? VisitaCorretajeId)
        {
            try
            {
                var visita = context.VisitaCorretaje.FirstOrDefault(x => x.VisitaCorretajeId == VisitaCorretajeId);

                var nombre = String.Empty;
                byte[] pdfBytes;
                Document doc = new Document(PageSize.A4, 100f, 100f, 100f, 100f);


                using (MemoryStream output = new MemoryStream())
                {
                    Font font = FontFactory.GetFont(FontFactory.HELVETICA, 12);
                    Font fontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);

                    PdfWriter wri = PdfWriter.GetInstance(doc, output);
                    doc.Open();

                    Image pic = Image.GetInstance(Server.MapPath(@"~\Content\img\Logo Afari Transparente.png"));
                    pic.Alignment = Element.ALIGN_LEFT;
                    pic.ScaleAbsolute(120, 35);

                    doc.Add(pic);

                    Paragraph paragraph = new Paragraph(" ", font) { Alignment = Element.ALIGN_LEFT };
                    doc.Add(paragraph);
                    paragraph = new Paragraph(" ", font) { Alignment = Element.ALIGN_LEFT };
                    doc.Add(paragraph);

                    Paragraph header = new Paragraph("CONSTANCIA DE VISITA", fontBold) { Alignment = Element.ALIGN_CENTER };
                    doc.Add(header);

                    paragraph = new Paragraph(" ", font) { Alignment = Element.ALIGN_LEFT };
                    doc.Add(paragraph);
                    paragraph = new Paragraph(" ", font) { Alignment = Element.ALIGN_LEFT };
                    doc.Add(paragraph);

                    paragraph = new Paragraph("CLIENTE: " + visita.Cliente.ToUpper(), font) { Alignment = Element.ALIGN_LEFT };
                    doc.Add(paragraph);

                    paragraph = new Paragraph(" ", font) { Alignment = Element.ALIGN_LEFT };
                    doc.Add(paragraph);
                    paragraph = new Paragraph(" ", font) { Alignment = Element.ALIGN_LEFT };
                    doc.Add(paragraph);

                    paragraph = new Paragraph("DATOS INMUEBLE", fontBold) { Alignment = Element.ALIGN_LEFT };
                    doc.Add(paragraph);

                    paragraph = new Paragraph("DIRECCIÓN: " + visita.Direccion.ToUpper(), font) { Alignment = Element.ALIGN_LEFT };
                    doc.Add(paragraph);

                    paragraph = new Paragraph("TIPO: " + visita.Tipo.ToUpper(), font) { Alignment = Element.ALIGN_LEFT };
                    doc.Add(paragraph);

                    paragraph = new Paragraph("PRECIO: " + ((visita.Moneda == "SOL" ? "S/ " : "$ ") + visita.Precio.ToString("#,##0.00")).ToUpper(), font) { Alignment = Element.ALIGN_LEFT };
                    doc.Add(paragraph);

                    paragraph = new Paragraph(" ", font) { Alignment = Element.ALIGN_LEFT };
                    doc.Add(paragraph);
                    paragraph = new Paragraph(" ", font) { Alignment = Element.ALIGN_LEFT };
                    doc.Add(paragraph);

                    paragraph = new Paragraph("DATOS CLIENTE", fontBold) { Alignment = Element.ALIGN_LEFT };
                    doc.Add(paragraph);

                    paragraph = new Paragraph("NOMBRE: " + visita.NombreCliente.ToUpper(), font) { Alignment = Element.ALIGN_LEFT };
                    doc.Add(paragraph);

                    paragraph = new Paragraph("FECHA VISITA: " + visita.Fecha.ToString("dd/MM/yyyy"), font) { Alignment = Element.ALIGN_LEFT };
                    doc.Add(paragraph);

                    paragraph = new Paragraph("HORA: " + visita.Hora.ToString(), font) { Alignment = Element.ALIGN_LEFT };
                    doc.Add(paragraph);

                    paragraph = new Paragraph(" ", font) { Alignment = Element.ALIGN_LEFT };
                    doc.Add(paragraph);
                    paragraph = new Paragraph(" ", font) { Alignment = Element.ALIGN_LEFT };
                    doc.Add(paragraph);

                    var arrFirma = visita.Firma.Split(',');
                    byte[] bytesFirma = System.Convert.FromBase64String(arrFirma[1]);
                    pic = Image.GetInstance(bytesFirma);
                    pic.Alignment = Element.ALIGN_CENTER;
                    pic.ScaleAbsolute(450, 150);
                    doc.Add(pic);

                    paragraph = new Paragraph("FIRMA", font) { Alignment = Element.ALIGN_CENTER };
                    doc.Add(paragraph);

                    doc.Close();
                    pdfBytes = output.ToArray();
                }

                var stream = new MemoryStream(pdfBytes);

                nombre = "Constancia_Visita_" + visita.Fecha.ToString("dd/MM/yyyy") + "_" + visita.NombreCliente + ".pdf";

                return File(stream, "text/plain", nombre);
            }
            catch (Exception ex)
            {
            }

            return null;
        }

        public ActionResult LstDatosClientes(Int32? np, String Direccion, String Distrito, String Cliente)
        {
            var model = new LstDatosClientesViewModel();
            model.Fill(CargarDatosContext(), np, Direccion, Distrito, Cliente);
            return View(model);
        }
        public ActionResult AddEditDatosClientes(Int32? ClienteCorretajeId, String FlagVer)
        {
            var model = new AddEditDatosClientesViewModel();
            model.Fill(CargarDatosContext(), ClienteCorretajeId, FlagVer);
            return View(model);
        }
        [HttpPost]
        public ActionResult AddEditDatosClientes(AddEditDatosClientesViewModel model)
        {
            try
            {
                ClienteCorretaje cliente = null;
                if (model.ClienteCorretajeId.HasValue)
                {
                    cliente = context.ClienteCorretaje.FirstOrDefault(x => x.ClienteCorretajeId == model.ClienteCorretajeId);
                }
                else
                {
                    cliente = new ClienteCorretaje();
                    cliente.Estado = ConstantHelpers.EstadoActivo;
                    context.ClienteCorretaje.Add(cliente);
                }

                cliente.TipoServicio = model.TipoServicio;
                cliente.TipoInmueble = model.TipoInmueble;
                cliente.Direccion = model.Direccion;
                cliente.Distrito = model.Distrito;
                cliente.Area = model.Area;
                cliente.Dormitorios = model.Dormitorios;
                cliente.Estacionamientos = model.Estacionamientos;
                cliente.Deposito = model.Deposito;
                cliente.Antiguedad = model.Antiguedad;
                cliente.CantidadPiso = model.CantidadPiso;
                cliente.Precio = model.Precio;
                cliente.CostoMantenimiento = model.CostoMantenimiento;
                cliente.Cliente = model.Cliente;
                cliente.Numero = model.Numero;
                cliente.Correo = model.Correo;

                context.SaveChanges();
                PostMessage(MessageType.Success);
                return RedirectToAction("LstDatosClientes");
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error);

                model.Fill(CargarDatosContext(), model.ClienteCorretajeId, model.FlagVer);
                TryUpdateModel(model);
                return View(model);
            }
        }
        public ActionResult DeleteDatosClientes(Int32 ClienteCorretajeId)
        {
            try
            {
                var visita = context.ClienteCorretaje.FirstOrDefault(x => x.ClienteCorretajeId == ClienteCorretajeId);
                visita.Estado = ConstantHelpers.EstadoInactivo;
                context.SaveChanges();

                PostMessage(MessageType.Success);
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error);
            }

            return RedirectToAction("LstDatosClientes");
        }
    }
}