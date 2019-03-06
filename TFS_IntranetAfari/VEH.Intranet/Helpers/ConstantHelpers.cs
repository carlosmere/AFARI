using PagedList.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VEH.Intranet.Models;

namespace VEH.Intranet.Helpers
{
    public static class ConstantHelpers
    {
        public static readonly Int32 DEFAULT_PAGE_SIZE = 25;

        public const String ROL_ADMINISTRADOR = "ADM";
        public const String ROL_PROPIETARIO = "PRO";

        public const String COMISION_MENSUAL = "MEN";
        public const String COMISION_ANUAL = "ANU";

        public const String COMISION_MENSUAL_TEXT = "Comisión Mensual";
        public const String COMISION_ANUAL_TEXT = "Comisión Anual";

        public const String ESTADO_PENDIENTE = "PEN";

        public static class TipoDato
        {
            public static class ObligacionesLaborales
            {
                public const string AFP = "ObligacionesLaborales[AFP]";
                public const string PresentacionPlanillaTrabajadores = "ObligacionesLaborales[Presentacion planilla trabajadores]";
                public const string EssaludYOnp = "ObligacionesLaborales[Essalud y Onp]";
            }
            public static class Equipo
            {
                public static String nombre(String n)
                {
                    return "Equipo[" + n + "]";
                }
            }
            public static class Cronograma
            {
                public static String nombre(String n)
                {
                    return "Crono[" + n + "]";
                }
            }
            public static String getInner(String n)
            {
                if (!n.Contains("["))
                    return n;

                return n.Split('[').Last().Split(']').First().ToString();
            }
            public static String getOutter(String n)
            {
                return n.Split('[').First();
            }
        }
        public static Propietario getTitularDepartamento(Departamento d)
        {
            Propietario propietario = null;
            propietario = d.Propietario.FirstOrDefault(X => X.ParentescoTitular == "Titular" && X.Estado == "ACT");
            if (propietario == null)
            {
                propietario = d.Propietario.FirstOrDefault(X => X.Estado == "ACT");
            }
            return propietario;
        }
        public static class TipoArchivo
        {
            public static readonly String BalanceGeneral = "Balance General";
            public static readonly String ReporteGeneral = "Reporte General";
            public static readonly String Recibo = "Recibo";

        }
        public static class Layout
        {
            public static readonly String MODAL_LAYOUT_PATH = "~/Views/Shared/_ModalLayout.cshtml";
            public static readonly String MODAL_EMAIL_PATH = "~/Views/Shared/_MailLayout.cshtml";
        }

        public static PagedListRenderOptions Bootstrap3Pager
        {
            get
            {
                return new PagedListRenderOptions
                {
                    DisplayLinkToFirstPage = PagedListDisplayMode.IfNeeded,
                    DisplayLinkToLastPage = PagedListDisplayMode.IfNeeded,
                    DisplayLinkToPreviousPage = PagedListDisplayMode.IfNeeded,
                    DisplayLinkToNextPage = PagedListDisplayMode.IfNeeded,
                    DisplayLinkToIndividualPages = true,
                    DisplayPageCountAndCurrentLocation = false,
                    MaximumPageNumbersToDisplay = 10,
                    DisplayEllipsesWhenNotShowingAllPageNumbers = true,
                    EllipsesFormat = "&#8230;",
                    LinkToFirstPageFormat = "««",
                    LinkToPreviousPageFormat = "«",
                    LinkToIndividualPageFormat = "{0}",
                    LinkToNextPageFormat = "»",
                    LinkToLastPageFormat = "»»",
                    PageCountAndCurrentLocationFormat = "Page {0} of {1}.",
                    ItemSliceAndTotalFormat = "Showing items {0} through {1} of {2}.",
                    FunctionToDisplayEachPageNumber = null,
                    ClassToApplyToFirstListItemInPager = null,
                    ClassToApplyToLastListItemInPager = null,
                    ContainerDivClasses = new[] { "pagination-container" },
                    UlElementClasses = new[] { "pagination" },
                    LiElementClasses = Enumerable.Empty<string>(),
                };
            }
        }
        public const string TipoMoraPorcentual = "POR";

        public const string EstadoActivo = "ACT";
        public const string EstadoInactivo = "INA";
        public const string EstadoEliminado = "ELI";
        public const string EstadoPendiente = "PEN";
        //public const string EstadoPagado = "PAG";
        public const string EstadoCerrado = "CER";
        public const string EstadoMora = "MOR";
        public const string EstadoTemporal = "TEM";

        public const string ModalidadRegular = "REG";
        public const string ModalidadTurno = "TUR";

        public const string DeudasActivas = "POSEE CUOTAS PENDIENTES";
        public const string DeudasCerradas = "NO POSEE DEUDAS";

        public static List<SelectListItem> ObtenerComboTipoDocumento()
        {
            return new List<SelectListItem>(){ 
                new SelectListItem { Value = "DNI", Text = "DNI" }, 
                //new SelectListItem { Value = "EXT", Text = "Carnet Extranjero" }            
            };
        }

        public static List<SelectListItem> ObtenerComboRoles()
        {
            return new List<SelectListItem>(){ 
                new SelectListItem { Value = "PRO", Text = "Propietario" },
                new SelectListItem { Value = "ADM", Text = "Administrador" }
             };
        }

        public static List<SelectListItem> ObtenerComboMeses()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem{ Value="1", Text="Enero"},
                new SelectListItem{ Value="2", Text="Febrero"},
                new SelectListItem{ Value="3", Text="Marzo"},
                new SelectListItem{ Value="4", Text="Abril"},
                new SelectListItem{ Value="5", Text="Mayo"},
                new SelectListItem{ Value="6", Text="Junio"},
                new SelectListItem{ Value="7", Text="Julio"},
                new SelectListItem{ Value="8", Text="Agosto"},
                new SelectListItem{ Value="9", Text="Setiembre"},
                new SelectListItem{ Value="10", Text="Octubre"},
                new SelectListItem{ Value="11", Text="Noviembre"},
                new SelectListItem{ Value="12", Text="Diciembre"},
            };
        }

        public static string ObtenerMesPorValorId(string valor)
        {
            string resultado = string.Empty;
            switch (valor)
            {
                case "1": resultado = "Enero"; break;
                case "2": resultado = "Febrero"; break;
                case "3": resultado = "Marzo"; break;
                case "4": resultado = "Abril"; break;
                case "5": resultado = "Mayo"; break;
                case "6": resultado = "Junio"; break;
                case "7": resultado = "Julio"; break;
                case "8": resultado = "Agosto"; break;
                case "9": resultado = "Septiembre"; break;
                case "10": resultado = "Octubre"; break;
                case "11": resultado = "Noviembre"; break;
                case "12": resultado = "Diciembre"; break;
            }
            return resultado;
        }

        public static List<SelectListItem> ObtenerComboModalidad()
        {
            return new List<SelectListItem>(){ 
                new SelectListItem { Value = ModalidadRegular, Text = "Regular" }, 
                new SelectListItem { Value = ModalidadTurno, Text = "Turnos" }            
            };
        }
    }
}