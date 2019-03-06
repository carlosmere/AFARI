using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.Controllers;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.Fee
{
    public class AddEditLeyendaViewModel : BaseViewModel
    {
        [Display(Name = "Descripción")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public String Descripcion { get; set; }

        [Display(Name = "Numero")]
        [Required(ErrorMessageResourceName = "CampoRequerido", ErrorMessageResourceType = typeof(i18n.ValidationStrings))]
        public Int32 Numero { get; set; }

        public Int32 EdificioId {get; set;}
        public Int32 UnidadTiempoId { get; set; }
        public Int32 BalanceEdificioUnidadTiempoId { get; set; }
        public Int32? LeyendaId { get; set; }
        public Int32? DepartamentoId { get; set; }
        public List<SelectListItem> LstComboUnidadTiempo { get; set; } = new List<SelectListItem>();
        public void Fill(CargarDatosContext datacontext,Int32? LeyendaId)
        {
            baseFill(datacontext);
            Leyenda ley;
            List<UnidadTiempo> lstunidadtiempo = datacontext.context.UnidadTiempo.OrderBy(x => -x.Orden).Where(x => x.Estado == ConstantHelpers.EstadoActivo).ToList();
            foreach (var item in lstunidadtiempo)
                LstComboUnidadTiempo.Add(new SelectListItem { Value = item.UnidadTiempoId.ToString(), Text = item.Descripcion.ToUpper() });
            if (LeyendaId.HasValue)
            {
                ley = datacontext.context.Leyenda.FirstOrDefault(X => X.LeyendaId == LeyendaId.Value);
                Descripcion = ley.Descripcion;
                Numero = ley.Numero;
                this.LeyendaId = LeyendaId;
            }
        }
    }
}