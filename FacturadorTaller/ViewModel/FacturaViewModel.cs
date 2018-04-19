using FacturadorTaller.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FacturadorTaller.ViewModel
{
    public class FacturaViewModel
    {
        [EmailAddress(ErrorMessage = "Correo Invalido !!!")]
        public string Email { get; set; }
        public string Nota { get; set; }
        public int cont { get; set; }
        public decimal TotalFacb { get; set; }
        public decimal TotalFac { get; set; }
        public decimal TotalItbis { get; set; }
        public Factura Factura { get; set; }
        public Clientes Clientes { get; set; }
        public IEnumerable<DetalleCot> DetalleCot { get; set; }

    }
}