using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;

namespace FacturadorTaller.Models
{
    [Table("cotDor.Pago")]
    public class Pago
    {
        public int PagoId { get; set; }

        public int FacturaId { get; set; }

        public string CajeroId { get; set; }

        public DateTime FechaPago { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = true)]
        public Decimal MontoPago { get; set; }

        public string Categoria { get; set; }

        public virtual Factura Factura { get; set; }

    }
}