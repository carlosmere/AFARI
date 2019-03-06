using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;

namespace VEH.Intranet.Helpers
{
    public enum AppRol
    {
        Administrador,
        Propietario
    }

    public enum SessionKey
    {
        Usuario,
        UsuarioId,
        NombreCompleto,
        Rol,
        RolCompleto,
        EdificioId,
        DepartamentoId,
        Correo,
        NombreRemitente,
        EsAdmin
    }

    public static class SessionHelpers
    {
        #region TieneRol
        public static Boolean TieneRol(this HttpSessionState Session, AppRol Rol)
        {
            return Session.GetRol() == Rol;
        }

        public static Boolean TieneRol(this HttpSessionStateBase Session, AppRol Rol)
        {
            return Session.GetRol() == Rol;
        }
        #endregion

        #region GetUsuarioId
        public static Int32 GetUsuarioId(this HttpSessionState Session)
        {
            return Get(Session, SessionKey.UsuarioId).ToInteger();
        }

        public static Int32 GetUsuarioId(this HttpSessionStateBase Session)
        {
            return Get(Session, SessionKey.UsuarioId).ToInteger();
        }
        #endregion

        #region GetNombreCompleto
        public static String GetNombreCompleto(this HttpSessionState Session)
        {
            return Get(Session, SessionKey.NombreCompleto).ToString();
        }

        public static String GetNombreCompleto(this HttpSessionStateBase Session)
        {
            return Get(Session, SessionKey.NombreCompleto).ToString();
        }
        #endregion
        #region GetNombreRemitente
        public static String GetNombreRemitente(this HttpSessionState Session)
        {
            return Get(Session, SessionKey.NombreRemitente).ToString();
        }

        public static String GetNombreRemitente(this HttpSessionStateBase Session)
        {
            return Get(Session, SessionKey.NombreRemitente).ToString();
        }
        #endregion

        #region GetCorreo
        public static String GetCorreo(this HttpSessionState Session)
        {
            return Get(Session, SessionKey.Correo).ToString();
        }

        public static String GetCorreo(this HttpSessionStateBase Session)
        {
            return Get(Session, SessionKey.Correo).ToString();
        }
        #endregion

        #region GetRol
        public static AppRol? GetRol(this HttpSessionState Session)
        {
            return (AppRol?)Get(Session, SessionKey.Rol);
        }

        public static AppRol? GetRol(this HttpSessionStateBase Session)
        {
            return (AppRol?)Get(Session, SessionKey.Rol);
        }
        #endregion
        
        #region GetRolCompleto
        public static String GetRolCompleto(this HttpSessionState Session)
        {
            return (String)Get(Session, SessionKey.RolCompleto);
        }

        public static String GetRolCompleto(this HttpSessionStateBase Session)
        {
            return (String)Get(Session, SessionKey.RolCompleto);
        }
        #endregion

        #region GetEdificioId
        public static Int32 GetEdificioId(this HttpSessionState Session)
        {
            return Get(Session, SessionKey.EdificioId).ToInteger();
        }

        public static Int32 GetEdificioId(this HttpSessionStateBase Session)
        {
            return Get(Session, SessionKey.EdificioId).ToInteger();
        }
        #endregion
        #region GetEsAdmin
        public static bool GetEsAdmin(this HttpSessionState Session)
        {
            return Get(Session, SessionKey.EsAdmin).ToBoolean();
        }

        public static bool GetEsAdmin(this HttpSessionStateBase Session)
        {
            return Get(Session, SessionKey.EsAdmin).ToBoolean();
        }
        #endregion

        #region GetDepartamentoId
        public static Int32 GetDepartamentoId(this HttpSessionState Session)
        {
            return Get(Session, SessionKey.DepartamentoId).ToInteger();
        }

        public static Int32 GetDepartamentoId(this HttpSessionStateBase Session)
        {
            return Get(Session, SessionKey.DepartamentoId).ToInteger();
        }
        #endregion

        #region Private

        private static object Get(HttpSessionState Session, String Key)
        {
            return Session[Key];
        }

        private static void Set(HttpSessionState Session, String Key, object Value)
        {
            Session[Key] = Value;
        }

        private static bool Exists(HttpSessionState Session, String Key)
        {
            return Session[Key] != null;
        }

        private static object Get(HttpSessionStateBase Session, String Key)
        {
            return Session[Key];
        }

        private static void Set(HttpSessionStateBase Session, String Key, object Value)
        {
            Session[Key] = Value;
        }

        private static bool Exists(HttpSessionStateBase Session, String Key)
        {
            return Session[Key] != null;
        }

        #endregion

        #region Getters setters GlobalKey
        //HttpSessionState
        public static object Get(this HttpSessionState Session, SessionKey Key)
        {
            return Get(Session, Key.ToString());
        }

        public static void Set(this HttpSessionState Session, SessionKey Key, object Value)
        {
            Set(Session, Key.ToString(), Value);
        }

        public static bool Exists(this HttpSessionState Session, SessionKey Key)
        {
            return Exists(Session, Key.ToString());
        }

        //HttpSessionStateBase
        public static object Get(this HttpSessionStateBase Session, SessionKey Key)
        {
            return Get(Session, Key.ToString());
        }

        public static void Set(this HttpSessionStateBase Session, SessionKey Key, object Value)
        {
            Set(Session, Key.ToString(), Value);
        }

        public static bool Exists(this HttpSessionStateBase Session, SessionKey Key)
        {
            return Exists(Session, Key.ToString());
        }
        #endregion
    }
}