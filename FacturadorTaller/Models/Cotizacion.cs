namespace FacturadorTaller.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("cotDor.Cotizacion")]
    public partial class Cotizacion
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Cotizacion()
        {
            DetalleCot = new HashSet<DetalleCot>();
            Factura = new HashSet<Factura>();
        }

        public int CotizacionId { get; set; }

        public DateTime Fecha { get; set; }

        public int ClienteId { get; set; }

        public decimal TotalFactura { get; set; }

        public decimal Itbis { get; set; }

        public virtual Clientes Clientes { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DetalleCot> DetalleCot { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Factura> Factura { get; set; }
    }
}
