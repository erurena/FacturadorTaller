using FacturadorTaller.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FacturadorTaller.ViewModel
{
    public class NcfViewModel
    {
        [ValidDate]
        public string NcfFecha { get; set; }

        public Ncf Ncf { get; set; }

        public DateTime GetDateF()
        {
            return DateTime.Parse(string.Format(NcfFecha));
        }
    }
}