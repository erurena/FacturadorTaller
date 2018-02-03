using FacturadorTaller.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FacturadorTaller.ViewModel
{
    public class FacturaViewModel
    {
        public Factura Factura { get; set; }
        public Clientes Clientes { get; set; }
        public IEnumerable<DetalleCot> DetalleCot { get; set; }

    }
}