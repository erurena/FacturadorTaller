using System;
using System.Collections.Generic;
using FacturadorTaller.Models;

namespace FacturadorTaller.ViewModel
{
    public class DetalleCotViewModel
    {
        public int Cantidad { get; set; }
        public string FichaVehiculo { get; set; }
        public decimal Valor { get; set; }
        public string Comentario { get; set; }

        public Clientes Clientes { get; set; }
        public Cotizacion Cotizacion { get; set; }
        public IEnumerable<DetalleCot> DetalleCot { get; set; }
    }
}