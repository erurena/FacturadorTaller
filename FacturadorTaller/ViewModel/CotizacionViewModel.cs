using FacturadorTaller.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FacturadorTaller.ViewModel
{
    public class CotizacionViewModel
    {
        [ValidDate]
        public string Fecha { get; set; }

        [EmailAddress(ErrorMessage ="Correo Invalido !!!")]
        public string Email { get; set; }
       
        public Clientes Clientes { get; set; }
        public Cotizacion Cotizacion { get; set; }
        public IEnumerable<DetalleCot> DetalleCot { get; set; }

        public DateTime GetDate()
        {
            return DateTime.Parse(string.Format(Fecha));
        }
    }
}