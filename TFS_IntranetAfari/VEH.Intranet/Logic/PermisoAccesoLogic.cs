using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using VEH.Intranet.Models;
using VEH.Intranet.Helpers;

namespace VEH.Intranet.Logic
{
    public class PermisoAccesoLogic
    {
        public SIVEHEntities context = new SIVEHEntities();

        public enum TipoPermiso
        {
            Restringido = 1,
            Lectura = 2,
            Escritura = 3
        }

        #region Functiones comunes
        public TipoPermiso GetMaxPermiso(TipoPermiso permisoA, TipoPermiso permisoB)
        {
            return (TipoPermiso)Math.Max((Int32)permisoA, (Int32)permisoB);
        }

        public TipoPermiso GetMinPermiso(TipoPermiso permisoA, TipoPermiso permisoB)
        {
            return (TipoPermiso)Math.Min((Int32)permisoA, (Int32)permisoB);
        }

        public Boolean TienePermisoSuficiente(TipoPermiso permisoObtenido, TipoPermiso permisoNecesario)
        {
            return (Int32)permisoObtenido >= (Int32)permisoNecesario;
        }
               
        #endregion

        #region Templates

        /*public TipoPermiso GetPermisoDocumentoVisita(Int32 usuarioId, Int32 documentoVisitaId)
        {
            try
            {
                var documentoVisita = context.DocumentoVisita.First(x => x.DocumentoVisitaId == documentoVisitaId);
                var usuario = context.Usuario.First(x => x.UsuarioId == usuarioId);

                if (usuario.Rol == ConstantHelpers.ROL_ADMINISTRADOR)
                    return TipoPermiso.Escritura;

                if (documentoVisita.EmpresaId == usuario.EmpresaId)
                    return TipoPermiso.Lectura;
            }
            catch (Exception ex)
            {
            }

            return TipoPermiso.Restringido;
        }*/
        
        #endregion
    }
}