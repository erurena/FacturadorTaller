using FacturadorTaller.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FacturadorTaller.ViewModel
{
    public class CreafacturaViewModel
    {
        [ValidDate]
        public string FechaFac { get; set; }

        [ValidDate]
        public string FechaVen { get; set; }

        public int CotId { get; set; }

        public String NcfInd { get; set; }

        public Factura Factura { get; set; }

        public DateTime GetDateF()
        {
            return DateTime.Parse(string.Format(FechaFac));
        }

        public DateTime GetDateFv()
        {
            return DateTime.Parse(string.Format(FechaVen));
        }
    }
}