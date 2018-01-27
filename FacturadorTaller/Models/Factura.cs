namespace FacturadorTaller.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("cotDor.Factura")]
    public partial class Factura
    {
        public int FacturaId { get; set; }

        public int CotizacionId { get; set; }

        public DateTime FechaFac { get; set; }

        public DateTime FechaVen { get; set; }

        public string Ncf { get; set; }

        public string PagoStatus { get; set; }

        [Required]
        public string OrdenCompraNu { get; set; }

        public string ImgOrdenCompra { get; set; }

        public virtual Cotizacion Cotizacion { get; set; }
    }
}
