using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Building
{
    public class EnviarEmailValidacionesViewModel : BaseViewModel
    {
        public String Destinatarios { get; set; }
        public String CopiaCarbon { get; set; }
        public String Mensaje { get; set; }
        public String Asunto { get; set; }
        public Int32 UsuarioId { get; set; }
        public String CorreoRemitente { get; set; }
        public String CopiaOculta { get; set; }
        public Int32? EmailValidacionId { get; set; }
        public String NombreEdificio { get; set; }
        public HttpPostedFileBase Archivo { get; set; }
        public List<HttpPostedFileBase> LstAdjuntos { get; set; }
        public List<HttpPostedFileBase> LstAdjuntosD { get; set; }
        public void Fill(CargarDatosContext c, Int32 edificioId, String directorioEliminar)
        {
            baseFill(c);
            this.EdificioId = edificioId;
            NombreEdificio = c.context.Edificio.FirstOrDefault( x => x.EdificioId == this.EdificioId).Nombre;
            var validacion = c.context.EmailValidacion.FirstOrDefault( x => x.EdificioId == this.EdificioId);
            if (validacion != null)
            {
                Destinatarios = validacion.Destinatarios;
                CopiaCarbon = validacion.CopiaCarbon;
                Asunto = validacion.Asunto;
                UsuarioId = validacion.UsuarioId;
                CorreoRemitente = validacion.Usuario.Email;
                EmailValidacionId = validacion.EmailValidacionId;
                Mensaje = validacion.Mensaje;
                CopiaOculta = validacion.CopiaOculta;
            }

            try
            {
                DirectoryInfo myDirInfo = new DirectoryInfo(directorioEliminar);

                foreach (FileInfo file in myDirInfo.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in myDirInfo.GetDirectories())
                {
                    dir.Delete(true);
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}