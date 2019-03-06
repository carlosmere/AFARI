using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.User
{
    public class LstUsuarioViewModel : BaseViewModel
    {
        public Int32 DepartamentoId { get; set; }
        public Int32 EdificioId { get; set; }
        public List<Usuario> LstUsuario { get; set; }

        public Departamento Departamento { get; set; }
        public Edificio Edificio { get; set; }

        public LstUsuarioViewModel() { }

        public void Fill(CargarDatosContext datacontext)
        {
            baseFill(datacontext);
            Departamento = datacontext.context.Departamento.FirstOrDefault(x=>x.DepartamentoId == DepartamentoId);
            Edificio = datacontext.context.Edificio.FirstOrDefault(x=>x.EdificioId == EdificioId);

            LstUsuario = new List<Usuario>();
            LstUsuario = datacontext.context.Usuario.
                OrderBy(x => x.Nombres).
                Where(x => (x.Estado == ConstantHelpers.EstadoActivo  || x.Estado == ConstantHelpers.EstadoInactivo || x.Estado == ConstantHelpers.EstadoPendiente || x.Estado == ConstantHelpers.EstadoTemporal) && x.DepartamentoId == DepartamentoId && x.Rol == ConstantHelpers.ROL_PROPIETARIO)
                .ToList();
        }

        public void FillAdm(CargarDatosContext datacontext)
        {
            LstUsuario = new List<Usuario>();
            LstUsuario = datacontext.context.Usuario.OrderBy(x => x.Nombres).Where(x => (x.Estado == ConstantHelpers.EstadoActivo || x.Estado == ConstantHelpers.EstadoPendiente || x.Estado == ConstantHelpers.EstadoTemporal) && x.Rol == ConstantHelpers.ROL_ADMINISTRADOR).ToList();
        }
    }
}