using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;
using VEH.Intranet.Controllers;
using VEH.Intranet.Helpers;
using VEH.Intranet.Models;
using PagedList;
using VEH.Intranet.ViewModel.Shared;

namespace VEH.Intranet.ViewModel.News
{
    public class LstNoticiaViewModel : BaseViewModel
    {
        public Int32? np { get; set; }
        public IPagedList<Noticia> LstNoticia { get; set; }
        public Int32 EdificioId { get; set; }
        public Edificio Edificio { get; set; }

        public LstNoticiaViewModel() { }

        public void Fill(CargarDatosContext datacontext, Int32? _np)
        {
            baseFill(datacontext);
            Edificio = datacontext.context.Edificio.FirstOrDefault(x => x.EdificioId == EdificioId);

            np = _np ?? 1;
            var lstnoticia = datacontext.context.Noticia
                .OrderByDescending(x => x.Fecha)
                .Include(x => x.Edificio)
                .Where(x => x.Estado == ConstantHelpers.EstadoActivo && x.EdificioId == EdificioId).ToList();
            LstNoticia = lstnoticia.ToPagedList(np.Value, ConstantHelpers.DEFAULT_PAGE_SIZE);
        }
    }
}