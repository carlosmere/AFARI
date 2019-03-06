using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Models;
using VEH.Intranet.Controllers;
using System.Web.Mvc;

namespace VEH.Intranet.Helpers
{
    public sealed class UbigeoHelper
    {
        SIVEHEntities datacontext = new SIVEHEntities();
        public List<SelectListItem> ListaComboDepartamentos()
        {
            List<SelectListItem> departamentos = new List<SelectListItem>();
            foreach (UDepartamento item in datacontext.UDepartamento.OrderBy(x => x.Nombre).ToList())
                departamentos.Add(new SelectListItem { Value = item.UDepartamentoId.ToString(), Text = item.Nombre });
            return departamentos;
        }

        public List<SelectListItem> ListarComboProvincias(Int32 UDepartamentoId)
        {
            List<SelectListItem> provincias = new List<SelectListItem>();
            foreach (var item in datacontext.UProvincia.OrderBy(x => x.Nombre).Where(x => x.UDepartamentoId == UDepartamentoId))
                provincias.Add(new SelectListItem { Value = item.UProvinciaId.ToString(), Text = item.Nombre });
            return provincias;
        }

        public List<SelectListItem> ListarComboDistritos(Int32 UProvinciaId)
        {
            List<SelectListItem> distritos = new List<SelectListItem>();
            foreach (var item in datacontext.UDistrito.OrderBy(x => x.Nombre).Where(x => x.UProvinciaId == UProvinciaId))
                distritos.Add(new SelectListItem { Value = item.UDistritoId.ToString(), Text = item.Nombre });
            return distritos;
        }
    }
}