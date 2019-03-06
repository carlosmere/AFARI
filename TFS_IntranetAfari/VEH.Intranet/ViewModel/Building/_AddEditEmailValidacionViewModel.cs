using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Building
{
    public class _AddEditEmailValidacionViewModel : BaseViewModel
    {
        public Int32? EmailValidacionId { get; set; }
        public String Destinatarios { get; set; }
        public String CopiaCarbon { get; set; }
        public String Asunto { get; set; }
        public String Mensaje { get; set; }
        public Int32 UsuarioId { get; set; }
        public String CopiaOculta { get; set; }
        public List<Usuario> LstUsuario { get; set; }
        public void Fill(CargarDatosContext c, Int32? emailValidacionId, Int32 edificioid)
        {
            baseFill(c);
            this.EdificioId = edificioid;
            this.EmailValidacionId = emailValidacionId;
            if (this.EmailValidacionId.HasValue)
            {
                var email = c.context.EmailValidacion.FirstOrDefault( x => x.EmailValidacionId == this.EmailValidacionId);
                Destinatarios = email.Destinatarios;
                CopiaCarbon = email.CopiaCarbon;
                Asunto = email.Asunto;
                UsuarioId = email.UsuarioId;
                Mensaje = email.Mensaje;
            }
            LstUsuario = c.context.Usuario.Where(x => x.Estado == ConstantHelpers.EstadoActivo
            && x.Rol == "ADM").ToList();
        }
    }
}