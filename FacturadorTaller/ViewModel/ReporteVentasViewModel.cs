using FacturadorTaller.Models;
using System;
using System.Collections.Generic;

namespace FacturadorTaller.ViewModel
{
    public class ReporteVentasViewModel
    {
        [ValidDate]
        public string FecIni { get; set; }
        [ValidDate]
        public string FecFin { get; set; }
        public decimal Total { get; set; }
        public IEnumerable<Pago> Pago { get; set; }
        public DateTime GetDatei()
        {
            return DateTime.Parse(string.Format(FecIni));
        }
        public DateTime GetDatef()
        {
            return DateTime.Parse(string.Format(FecFin));
        }

    }

}