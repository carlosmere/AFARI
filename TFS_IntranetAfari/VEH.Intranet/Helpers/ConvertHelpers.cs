using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace VEH.Intranet.Helpers
{
    public static class ConvertHelpers
    {
        private static readonly DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        public static string[] RemoveDuplicates(string[] myList)
        {
            System.Collections.ArrayList newList = new System.Collections.ArrayList();

            foreach (string str in myList)
                if (!newList.Contains(str))
                    newList.Add(str);
            return (string[])newList.ToArray(typeof(string));
        }
        public static Int32 ToInteger(this object val)
        {
            return ConvertHelpers.ToInteger(val, 0);
        }

        public static Int32 ToInteger(this object val, Int32 def)
        {
            try
            {
                Int32 reval = 0;

                if (Int32.TryParse(val.ToString(), out reval))
                    return reval;
            }
            catch (Exception ex)
            {
            }

            return def;
        }

        public static Decimal ToDecimal(this object val)
        {
            return ConvertHelpers.ToDecimal(val, 0);
        }

        public static Decimal ToDecimal(this object val, Decimal def)
        {
            try
            {
                Decimal reval = 0;

                if (Decimal.TryParse(val.ToString(), out reval))
                    return reval;
            }
            catch (Exception ex)
            {
            }

            return def;
        }

        public static long ToJsonDateTime(this DateTime val)
        {
            return ((long)(val - unixEpoch).TotalSeconds) * 1000;
        }

        public static String ToFullCallendarDate(this DateTime val)
        {
            return val.ToString("yyyy-MM-dd hh:mm:ss");
        }

        public static DateTime ToDateTime(this object val)
        {
            return ConvertHelpers.ToDateTime(val, DateTime.MinValue);
        }

        /// <summary>
        /// Convierte una fecha UTC a una fecha "Local". Se toma la hora de Per� (GMT-5)
        /// </summary>
        /// <param name="val">Fecha, en UTC</param>
        /// <returns>Fecha, en hora de Per�</returns>
        public static DateTime ToLocalDate(this DateTime val)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(val, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"));
        }

        /// <summary>
        /// Convierte una fecha UTC a una string de una fecha "Local" con formato. Se toma la hora de Per� (GMT-5)
        /// </summary>
        /// <param name="val">Fecha, en UTC</param>
        /// <param name="format">Formato para el string</param>
        /// <returns>Fecha, en hora de Per�</returns>
        public static String ToLocalDateString(this DateTime val, String format)
        {
            return val.ToLocalDate().ToString(format);
        }

        public static DateTime ToDateTime(this object val, DateTime def)
        {
            try
            {
                DateTime reval = DateTime.MinValue;

                if (DateTime.TryParse(val.ToString(), out reval))
                    return reval;
            }
            catch (Exception ex)
            {
            }

            return def;
        }

        public static Boolean IsBetween(this DateTime val, DateTime Start, DateTime End)
        {
            return val >= Start && val <= End;
        }

        public static Boolean ToBoolean(this object val)
        {
            return ConvertHelpers.ToBoolean(val, false);
        }

        public static Boolean ToBoolean(this object val, Boolean def)
        {
            try
            {
                Boolean reval = false;

                if (Boolean.TryParse(val.ToString(), out reval))
                    return reval;
            }
            catch (Exception ex)
            {
            }

            return def;
        }

        public static MvcHtmlString ToHTMLJson(this object val)
        {
            var json = JsonConvert.SerializeObject(val, Formatting.Indented, new JsonSerializerSettings() { PreserveReferencesHandling = PreserveReferencesHandling.None, ReferenceLoopHandling = ReferenceLoopHandling.Ignore, ObjectCreationHandling = ObjectCreationHandling.Reuse });
            return MvcHtmlString.Create(json);
        }
    }
}