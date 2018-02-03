using FacturadorTaller.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace FacturadorTaller.ViewModel
{
    public class PagoViewModel
    {
        public Factura Factura { get; set; }
        [ValidDate]
        public string Fecha { get; set; }

        [Display(Name = "Total Pago")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal SumPago { get; set; }

        [Display(Name = "Balance Pendiente")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal PagoFac { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal Balance { get; set; }

        public Pago Pago { get; set; }

        public DateTime GetDate()
        {
            return DateTime.Parse(string.Format(Fecha));
        }

    }
}