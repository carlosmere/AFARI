using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.Filters;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.External;

namespace VEH.Intranet.Controllers
{
    public class ExternalController : BaseController
    {
        // GET: External
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult CambiarContrasena()
        {
            var model = new CambiarContrasenaViewModel();
            model.Fill(CargarDatosContext(),Session.GetUsuarioId());
            return View(model);
        }
        [HttpPost]
        public ActionResult CambiarContrasena(CambiarContrasenaViewModel model)
        {
            try
            {
                var usuario = context.Usuario.FirstOrDefault( x => x.UsuarioId == model.UsuarioId);
                usuario.Password = model.ContrasenaNueva;
                context.SaveChanges();

                PostMessage(MessageType.Success);
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error);
            }
            return RedirectToAction("Index","Home");
        }
        public ActionResult ElementosExternos(Int32 EdificioId)
        {
            ElementosExternosViewModel model = new ElementosExternosViewModel();
            model.fill(CargarDatosContext(), EdificioId);
            return View(model);
        }
        public ActionResult AddEditElementoExterno(Int32 EdificioId, Int32? ElementoExternoId)
        {
            AddEditElementoExternoViewModel model = new AddEditElementoExternoViewModel();
            model.Fill(CargarDatosContext(), EdificioId, ElementoExternoId);
            return View(model);
        }
        [HttpPost]
        public ActionResult AddEditElementoExterno(AddEditElementoExternoViewModel model)
        {
            MenuPropietarioEdificio elemento = null;
            if (model.ElementoExternoId.HasValue)
                elemento = context.MenuPropietarioEdificio.FirstOrDefault(X => X.MenuPropietarioEdificioId == model.ElementoExternoId.Value);
            if (elemento == null)
            {
                elemento = new MenuPropietarioEdificio();
                context.MenuPropietarioEdificio.Add(elemento);
            }

            elemento.EdificioId = model.EdificioId;
            elemento.Estado = "ACT";
            elemento.Icono = model.Icono;
            elemento.Nombre = model.Nombre;


            if (model.Documento != null && model.Documento.ContentLength != 0)
            {
                string _rutaArchivodir = Server.MapPath("~") + "/Resources/Files/";
                string _nombreArc = Guid.NewGuid().ToString().Substring(0, 4) + model.Documento.FileName;
                elemento.Documento = _nombreArc;
                model.Documento.SaveAs(_rutaArchivodir+ _nombreArc);
            }


            context.SaveChanges();
            return RedirectToAction("ElementosExternos", "External", new { EdificioId = elemento.EdificioId });
        }
        public ActionResult DeleteElementoExterno(Int32 ElementoExternoId, Int32 EdificioId)
        {
            var elemento = context.MenuPropietarioEdificio.FirstOrDefault(X => X.MenuPropietarioEdificioId == ElementoExternoId);
            if (elemento != null)
            {
                elemento.Estado = "INA";
                context.SaveChanges();
            }

            return RedirectToAction("ElementosExternos", "External", new { EdificioId = EdificioId });
        }

        public ActionResult MenuExtra(Int32 menuId)
        {
            MenuExtraViewModel model = new MenuExtraViewModel();
            model.fill(CargarDatosContext(), menuId);
            return View(model);
        }
        public FileStreamResult DescargarDocumentoMenu(Int32 menuId)
        {

            var documento = context.MenuPropietarioEdificio.FirstOrDefault(X => X.MenuPropietarioEdificioId == menuId);
            if (documento == null)
                return null;


            var ruta = Path.Combine(Request.MapPath("~/Resources/Files"), documento.Documento);
            FileStream fs = new FileStream(ruta, FileMode.Open, FileAccess.Read);
            return File(fs, "application/pdf");

        }
        public FileStreamResult DescargarDocumentoGenerico(Int32 itemId)
        {

            var documento = context.DatoEdificio.FirstOrDefault(X => X.DatoEdificioId == itemId);
            if (documento == null)
                return null;


            var ruta = Path.Combine(Request.MapPath("~/Resources/Files"), documento.Dato);
            FileStream fs = new FileStream(ruta, FileMode.Open, FileAccess.Read);
            return File(fs, "application/pdf");

        }
        [ViewParameter("Edificio", "fa fa-cubes")]
        public ActionResult LstEquiposSinCertificado(Int32 EdificioId, Int32? Anio)
        {
            var model = new LstEquiposSinCertificadoViewModel();
            model.Fill(CargarDatosContext(), EdificioId, Anio);
            return View(model);
        }
        public ActionResult ItemsGenericos(Int32 EdificioId, String filtroNombre, String filtroDato, String filtroTipo, String vista, Int32? UnidadTiempoId, Int32? Anio)
        {
            ItemsGenericosViewModel model = new ItemsGenericosViewModel();
            model.Vista = vista;
            model.fill(CargarDatosContext(), filtroTipo, filtroNombre, filtroDato, UnidadTiempoId, EdificioId, Anio);
            return View(vista, model);
        }
        public ActionResult _MarcarCronogramaMantenimiento(Int32 EdificioId, Int32 Anio)
        {
            var model = new _MarcarCronogramaMantenimientoViewModel();
            model.Fill(CargarDatosContext(), EdificioId, Anio);
            return View(model);
        } 
        [HttpPost]
        public ActionResult _MarcarCronogramaMantenimiento(FormCollection frm)
        {
            var EdificioId = frm["EdificioId"].ToInteger();
            try
            {
                var Anio = frm["Anio"].ToInteger();
                var LstDatos = context.DatoEdificio.Where(X => X.EdificioId == EdificioId && X.Tipo.Contains("Crono") && X.UnidadTiempo.Anio == Anio).ToList();
                var LstElementos = frm.AllKeys.Where(X => X.StartsWith("check-")).ToList();
                foreach (var item in LstElementos)
                {
                    var DatoEdificioId = item.Replace("check-", String.Empty).ToInteger();
                    var dato = context.DatoEdificio.FirstOrDefault( x => x.DatoEdificioId == DatoEdificioId);
                    dato.EsRealizado = frm[item] == "on" ? true : false;
                    context.SaveChanges();
                    LstDatos.Remove(dato);
                }
                foreach (var item in LstDatos)
                {
                    item.EsRealizado = false;
                    context.SaveChanges();
                }
                PostMessage(MessageType.Success);
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error);
            }
            return RedirectToAction("CronogramaMantenimientos",new { EdificioId = EdificioId, Editar = "True" });
        }

        public ActionResult AddEditItemGenerico(Int32? itemId,String vista,Int32 EdificioId,String tipo, String TipoLista, Int32? MaxOrden)
        {
            AddEditItemGenericoViewModel model = new AddEditItemGenericoViewModel();
            model.fill(CargarDatosContext(), itemId, vista, EdificioId, tipo,MaxOrden);
            model.TipoLista = TipoLista;
            return View(model);
        }
        [HttpPost]
        public ActionResult AddEditItemGenerico(AddEditItemGenericoViewModel model)
        {
            var filtro = ConstantHelpers.TipoDato.getOutter(model.tipo);
            try
            {
                DatoEdificio dato = null;

                if (model.itemId.HasValue) dato = context.DatoEdificio.FirstOrDefault(X => X.DatoEdificioId == model.itemId.Value);
                if (dato == null) { dato = new DatoEdificio(); context.DatoEdificio.Add(dato); }

                dato.Nombre = model.nombre;
                dato.Tipo = model.tipo;
                dato.EdificioId = model.EdificioId;
                dato.UnidadTiempoId = model.UnidadTiempoId;
                dato.Orden = model.Orden;
                dato.AplicaMantenimiento = true;//model.AplicaMantenimiento;

                if (model.file != null && model.file.ContentLength != 0)
                {
                    string _rutaArchivodir = Server.MapPath("~") + "/Resources/Files";
                    string _nombreArc = Guid.NewGuid().ToString().Substring(0, 6) + Path.GetExtension(model.file.FileName);
                    dato.Dato = _nombreArc;
                    model.file.SaveAs(Path.Combine(_rutaArchivodir, _nombreArc));
                }
                else
                    dato.Dato = model.nombre;

                PostMessage(MessageType.Success);
                context.SaveChanges();
                if(String.IsNullOrEmpty(model.TipoLista))
                    return RedirectToAction("ItemsGenericos", new { vista = model.vista, EdificioId = model.EdificioId, filtroTipo = filtro });
                else
                    return RedirectToAction("LstEquiposSinCertificado","External", new { EdificioId = model.EdificioId});
            }
            catch (Exception ex)
            {
                PostMessage(MessageType.Error);
                if (String.IsNullOrEmpty(model.TipoLista))
                    return RedirectToAction("ItemsGenericos", new { vista = model.vista, EdificioId = model.EdificioId, filtroTipo = filtro });
                else
                    return RedirectToAction("LstEquiposSinCertificado", "External", new { EdificioId = model.EdificioId });
            }        
        }
        public ActionResult DeleteItemGenerico(Int32 EdificioId,String filtroNombre,String filtroDato,String filtroTipo,String vista,Int32? UnidadTiempoId,Int32 itemId)
        {
            var item = context.DatoEdificio.FirstOrDefault(X => X.DatoEdificioId == itemId);
            if (item != null)
            {
                context.DatoEdificio.Remove(item);
                context.SaveChanges();
            }
            UnidadTiempoId = null;
            PostMessage(MessageType.Success);
            return RedirectToAction("ItemsGenericos", new { EdificioId = EdificioId, filtroNombre = filtroNombre, filtroDato = filtroDato, filtroTipo = filtroTipo, vista = vista, UnidadTiempoId = UnidadTiempoId });

        }
        public ActionResult CronogramaMantenimientos(Boolean Editar, Int32 EdificioId,Int32? Anio)
        {
            CronogramaMantenimientosViewModel model = new CronogramaMantenimientosViewModel();
            model.fill(CargarDatosContext(), Editar, EdificioId,Anio);
            return View(model);
        }

        public ActionResult LimpiarGastosEIngresosAdicionales(Int32 orden)
        {

            var detallesABorrar = context.Gasto.Where(x => x.UnidadTiempo.Orden < orden).SelectMany(X => X.DetalleGasto).ToList();
            context.DetalleGasto.RemoveRange(detallesABorrar);
            var archivosABorrar = context.Gasto.Where(x => x.UnidadTiempo.Orden < orden).SelectMany(X => X.ArchivoGasto).ToList();
            context.ArchivoGasto.RemoveRange(archivosABorrar);

            var detallesBorrar = context.Ingreso.Where(x => x.UnidadTiempo.Orden < orden).SelectMany(X => X.DetalleIngreso).ToList();
            context.DetalleIngreso.RemoveRange(detallesBorrar);
            var archivosBorrar = context.Ingreso.Where(x => x.UnidadTiempo.Orden < orden).SelectMany(X => X.ArchivoIngreso).ToList();
            context.ArchivoIngreso.RemoveRange(archivosBorrar);

            context.Gasto.RemoveRange(context.Gasto.Where(X => X.UnidadTiempo.Orden < orden).ToList());
            context.Ingreso.RemoveRange(context.Ingreso.Where(X => X.UnidadTiempo.Orden < orden).ToList());
            context.SaveChanges();
            return RedirectToAction("LstEdificio", "Building");
        }


        [HttpPost]
        public ActionResult CronogramaMantenimientos(Boolean Editar, Int32 EdificioId, FormCollection formCollection, Int32 Anio)
        {

            List<DatoEdificio> datosAnteriores = context.DatoEdificio.Where(X => X.EdificioId == EdificioId && X.Tipo.Contains("Crono") && X.UnidadTiempo.Anio == Anio).ToList();
            context.DatoEdificio.RemoveRange(datosAnteriores);
            Dictionary<String, Int32> mapa = new Dictionary<string, int>();
            List<DatoEdificio> datosNuevos = new List<DatoEdificio>();
            foreach (var key in formCollection.AllKeys)
            {
                if (key.StartsWith("che|"))
                {
                    Int32 mes = (key.Split('|').ToList())[1].ToInteger();
                    String equipo = (key.Split('|').ToList()).Last();
                    mapa[equipo +"|"+ mes] = 0;

                }
                if (key.StartsWith("num|"))
                {
                    Int32 mes = (key.Split('|').ToList())[1].ToInteger();
                    String equipo = (key.Split('|').ToList()).Last();
                    Int32 num =(!String.IsNullOrEmpty(formCollection[key]))?formCollection[key].ToInteger():-1;
                    if (num != -1)
                        mapa[equipo + "|" + mes] = num;
                }
            }
            var unidadTiempo = context.UnidadTiempo.FirstOrDefault(x => x.Anio == Anio);
            var i = 0;
            foreach (var item in mapa)
            {
                String equipo = item.Key.Split('|').First();
                String mes = item.Key.Split('|').Last();
                DatoEdificio nuevoDato = new DatoEdificio();
                nuevoDato.EdificioId = EdificioId;
                nuevoDato.Tipo = ConstantHelpers.TipoDato.Cronograma.nombre(equipo);
                nuevoDato.Dato = mes + "|" + item.Value;
                nuevoDato.Nombre = mes;
                nuevoDato.EsRealizado = i >= datosAnteriores.Count ? false : datosAnteriores[i].EsRealizado;
                nuevoDato.UnidadTiempoId = unidadTiempo.UnidadTiempoId;
                datosNuevos.Add(nuevoDato);
                i++;
            }

            context.DatoEdificio.AddRange(datosNuevos);
            context.SaveChanges();

            return RedirectToAction("CronogramaMantenimientos", new { Editar = Editar, EdificioId = EdificioId ,Anio = Anio});
        }

        
    }
}