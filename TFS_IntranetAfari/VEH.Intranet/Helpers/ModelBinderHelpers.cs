using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace VEH.Intranet.Helpers
{
    public class PeruDateTimeModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var displayFormat = bindingContext.ModelMetadata.DisplayFormatString;
            //var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            var modelName = bindingContext.ModelName;
            var value = bindingContext.ValueProvider.GetValue(modelName);
            var attemptedValue = value != null ? value.AttemptedValue : null;

            if (!string.IsNullOrEmpty(displayFormat))
            {
                DateTime date;
                displayFormat = displayFormat.Replace("{0:", string.Empty).Replace("}", string.Empty);
                // use the format specified in the DisplayFormat attribute to parse the date
                if (DateTime.TryParseExact(attemptedValue, displayFormat, CultureInfo.GetCultureInfo("es-PE"), DateTimeStyles.None, out date))
                {
                    return date;
                }
                else
                {
                    if (bindingContext.ModelMetadata.IsNullableValueType && string.IsNullOrWhiteSpace(attemptedValue))
                        return null;
                    else
                        bindingContext.ModelState.AddModelError(bindingContext.ModelName, string.Format("{0} no es una fecha correcta.", attemptedValue));
                }
            }
            else
            {
                DateTime date;
                if (DateTime.TryParse(attemptedValue, CultureInfo.GetCultureInfo("es-PE"), DateTimeStyles.None, out date))
                {
                    return date;
                }
                else
                {
                    if (bindingContext.ModelMetadata.IsNullableValueType && string.IsNullOrWhiteSpace(attemptedValue))
                        return null;
                    else
                        bindingContext.ModelState.AddModelError(bindingContext.ModelName, string.Format("{0} no es una fecha correcta.", attemptedValue));
                }
            }

            return base.BindModel(controllerContext, bindingContext);
        }
    }

    public class DecimalModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext,
                                         ModelBindingContext bindingContext)
        {
            object result = null;

            // Don't do this here!
            // It might do bindingContext.ModelState.AddModelError
            // and there is no RemoveModelError!
            // 
            // result = base.BindModel(controllerContext, bindingContext);

            string modelName = bindingContext.ModelName;
            string attemptedValue =bindingContext.ValueProvider.GetValue(modelName).AttemptedValue;

            // Depending on CultureInfo, the NumberDecimalSeparator can be "," or "."
            // Both "." and "," should be accepted, but aren't.
            string wantedSeperator = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
            string alternateSeperator = (wantedSeperator == "," ? "." : ",");

            if (attemptedValue.IndexOf(wantedSeperator) == -1
                && attemptedValue.IndexOf(alternateSeperator) != -1)
            {
                attemptedValue =
                    attemptedValue.Replace(alternateSeperator, wantedSeperator);
            }

            try
            {
                if (bindingContext.ModelMetadata.IsNullableValueType && string.IsNullOrWhiteSpace(attemptedValue)) {
                    return null;
                }

                result = decimal.Parse(attemptedValue, NumberStyles.Any);
            }
            catch (FormatException e)
            {
                bindingContext.ModelState.AddModelError(modelName, e);
            }

            return result;
        }
    }
}
