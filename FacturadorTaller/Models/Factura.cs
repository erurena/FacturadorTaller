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

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime FechaFac { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime FechaVen { get; set; }

        public string Ncf { get; set; }

        public string PagoStatus { get; set; }

        public string Consolidado { get; set; }

        public string FechaNcf { get; set; }

        [Required]
        public string OrdenCompraNu { get; set; }

        public string ImgOrdenCompra { get; set; }

        public virtual Cotizacion Cotizacion { get; set; }
    }
}
