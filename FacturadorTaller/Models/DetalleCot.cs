namespace FacturadorTaller.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("cotDor.DetalleCot")]
    public partial class DetalleCot
    {
        public int DetalleCotId { get; set; }

        public int CotizacionId { get; set; }

        public int ProductoId { get; set; }

        public int Cantidad { get; set; }

        public string FichaVehiculo { get; set; }

        public decimal Valor { get; set; }

        public string Comentario { get; set; }

        public virtual Cotizacion Cotizacion { get; set; }

        public virtual Producto Producto { get; set; }
    }
}
