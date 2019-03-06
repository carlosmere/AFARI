using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Models;
using VEH.Intranet.Controllers;
using VEH.Intranet.Helpers;
using VEH.Intranet.ViewModel.Shared;
using System.Web.Mvc;

namespace VEH.Intranet.ViewModel.Home
{
    public class AdministradorIndexViewModel : BaseViewModel
    {
        public String DesUnidadTiempo { get; set; }
        public String NUsuariosAdmin { get; set; }
        public String NEdificios { get; set; }
        public List<SelectListItem> LstAnios { get; set; } = new List<SelectListItem>();
        public Int32? Anio { get; set; } = DateTime.Now.Year;
        public Dictionary<String, Int32> DicVisitaEdificioWeb { get; set; } = new Dictionary<string, int>();
        public Dictionary<String, Int32> DicVisitaEdificioApp { get; set; } = new Dictionary<string, int>();
        public AdministradorIndexViewModel()
        {
        }

        public void CargarDatos(CargarDatosContext dataContext, Int32? anio)
        {
            baseFill(dataContext);
            UnidadTiempo _UnidadTiempo = dataContext.context.UnidadTiempo.FirstOrDefault(x => x.EsActivo);
            DesUnidadTiempo = _UnidadTiempo == null ? String.Empty : _UnidadTiempo.Descripcion;

            this.Anio = anio ?? DateTime.Now.Year;
            NEdificios = dataContext.context.Edificio.Where(x => x.Estado.Equals("ACT")).Count().ToString();
            NUsuariosAdmin = dataContext.context.Usuario.Where(x => x.Rol.Equals("ADM")).Count().ToString();

            var anioActual = DateTime.Now.Year;
            for (int i = 2018; i <= anioActual; i++)
            {
                LstAnios.Add(new SelectListItem
                {
                    Value = i.ToString(),
                    Text = i.ToString()
                });
            }

            var lstEdificio = dataContext.context.Edificio.Where(x => x.Estado == ConstantHelpers.EstadoActivo).Select( x => x.EdificioId + "-" + x.Nombre).ToList();
            var lstVisitas = dataContext.context.Visita.Where( x => x.Fecha.Year == this.Anio && x.Tipo == "WEB").ToList();
            foreach (var edi in lstEdificio)
            {
                var auxSplit = edi.Split('-');
                var edificioId = auxSplit[0].ToInteger();
                var cant = lstVisitas.Count(x => x.EdificioId == edificioId);
                DicVisitaEdificioWeb.Add(edi, cant);
            }

            lstVisitas = dataContext.context.Visita.Where(x => x.Fecha.Year == this.Anio && x.Tipo == "APP").ToList();
            foreach (var edi in lstEdificio)
            {
                var auxSplit = edi.Split('-');
                var edificioId = auxSplit[0].ToInteger();
                var cant = lstVisitas.Count(x => x.EdificioId == edificioId);
                DicVisitaEdificioApp.Add(edi, cant);
            }

        }
    }
}