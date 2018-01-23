using System;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.Threading;


namespace FacturadorTaller.ViewModel
{
    public class ValidDate : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            DateTime dateTime;
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("es-MX");
            var isValid = DateTime.TryParseExact(Convert.ToString(value),
                "dd/MM/yyyy",
                CultureInfo.CurrentCulture,
                DateTimeStyles.None,
                out dateTime);

            return (isValid);
        }
    }
}