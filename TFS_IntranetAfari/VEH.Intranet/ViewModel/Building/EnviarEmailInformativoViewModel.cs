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
    public class EnviarEmailInformativoViewModel : BaseViewModel
    {
        public Int32 EdificioId { get; set; }
        public String Asunto { get; set; }
        public String Mensaje { get; set; }
        public List<Destinatario> lstDestinatario { get; set; }
        public List<Boolean> check { get; set; }
        public IEnumerable<HttpPostedFileBase> Archivos { get; set; }
        public String NombreEdificio { get; set; }
        public String CopiaCarbon { get; set; }
        public void Fill(CargarDatosContext datacontext,Int32 edificioId)
        {
            baseFill(datacontext);
            EdificioId = edificioId;
            var edificio = datacontext.context.Edificio.FirstOrDefault(X => X.EdificioId == edificioId);
            NombreEdificio = edificio.Nombre;
            //var lstPropietarios = edificio.Departamento.Where( x => x.Estado == ConstantHelpers.EstadoActivo).SelectMany(x => x.Propietario).Distinct().ToList();
            var query = datacontext.context.Propietario.Where(x => x.Estado == ConstantHelpers.EstadoActivo && x.Departamento.EdificioId == edificioId).Distinct().ToList();

            //List<Propietario> lstPropietarios = new List<Propietario>();
            //List<String> Lstnombre = new List<String>();
            //foreach (var item in query)
            //{
            //    if (!Lstnombre.Contains(item.Nombres))
            //    {
            //        Lstnombre.Add(item.Nombres);
            //        lstPropietarios.Add(item);
            //    }
            //}
            lstDestinatario = new List<Destinatario>();
            lstDestinatario.AddRange(query.Select(X => new Destinatario { dptoId = X.DepartamentoId, dpto = X.Departamento.Numero, nombre = X.Nombres + X.ApellidoPaterno, email = X.Email ,id=X.PropietarioId.ToString()}).ToList());
            lstDestinatario.AddRange(query.SelectMany(X=>X.Inquilino.Select(Y=> new Destinatario{ dptoId = Y.Propietario.DepartamentoId , dpto=Y.Propietario.Departamento.Numero,nombre = Y.Nombres ,email=Y.Email , id="i"+Y.InquilinoId.ToString() , EsInquilino = true}).ToList()));
            lstDestinatario = lstDestinatario.OrderBy(x => x.dptoId).ToList();

            check = new List<bool>();
            
        }
        public class Destinatario
        {
            public Int32 dptoId { get; set; }
            public String nombre { get; set; } = String.Empty;
            public String dpto { get; set; } = String.Empty;
            public String email { get; set; } = String.Empty;
            public String id { get; set; } = String.Empty;
            public Boolean EsInquilino { get; set; } = false;
        }

    }
}