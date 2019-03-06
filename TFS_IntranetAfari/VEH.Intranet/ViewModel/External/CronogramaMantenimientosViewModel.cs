using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.Controllers;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.External
{
    public class CronogramaMantenimientosViewModel : BaseViewModel
    {
        public Int32 EdificioId { get; set; }
        public Boolean Editar { get; set; }
        public List<String> LstEquipos { get; set; } = new List<String>();
        public List<DatoEdificio> LstDatos { get; set; } = new List<DatoEdificio>();
        public Dictionary<String, Int32> mapa { get; set; } = new Dictionary<String, Int32>();
        public Dictionary<String, bool> DicValores { get; set; } = new Dictionary<String, bool>();
        public List<SelectListItem> LstAnios { get; set; } = new List<SelectListItem>();
        public Int32? Anio { get; set; }
        public void fill(CargarDatosContext datacontext,Boolean Editar, Int32 EdificioId,Int32? Anio)
        {
            baseFill(datacontext);
            this.EdificioId = EdificioId;
            this.Editar = Editar;

            var anioMinimo = datacontext.context.UnidadTiempo.Min(x => x.Anio);
            var anioActual = DateTime.Now.Year;

            this.Anio = Anio ?? anioActual;

            for (int i = 0; i <= anioActual - anioMinimo; i++)
            {
                var value = (anioActual - i).ToString();
                LstAnios.Add(new SelectListItem { Value = value, Text = value });
            }

            if (this.Anio.HasValue)
            {
                LstEquipos = datacontext.context.DatoEdificio.Where(X => X.EdificioId == EdificioId && X.Tipo.Contains("Equipo") && X.AplicaMantenimiento == true && X.UnidadTiempo.Anio == this.Anio).OrderBy(X => X.Orden).ToList().Select(X => X.Tipo).Distinct().ToList();
                LstDatos = datacontext.context.DatoEdificio.Where(X => X.EdificioId == EdificioId && X.Tipo.Contains("Crono")  && X.UnidadTiempo.Anio == this.Anio).OrderBy( X => X.Orden).ToList();
            }
             
            //LstEquipos = datacontext.context.DatoEdificio.Where(X => X.EdificioId == EdificioId && X.Tipo.Contains("Equipo") && X.AplicaMantenimiento == true).ToList().Select(X => X.Tipo).Distinct().ToList();

            mapa = new Dictionary<string,int>();
            for(int i=0;i<LstEquipos.Count;i++)
            {
                LstEquipos[i]= ConstantHelpers.TipoDato.getInner(LstEquipos[i]);
            }
            foreach (var dato in LstDatos)
            {
                mapa[ConstantHelpers.TipoDato.getInner(dato.Tipo) + dato.Dato.Split('|').First()] = dato.Dato.Split('|').Last().ToInteger();
            }
           
        }
    }
}