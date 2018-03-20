namespace FacturadorTaller.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("cotDor.Producto")]
    public partial class Producto
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Producto()
        {
            DetalleCot = new HashSet<DetalleCot>();
        }

        public int ProductoId { get; set; }

        public string NombreProducto { get; set; }

        public string Categoria { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = true)]
        public decimal Precio { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DetalleCot> DetalleCot { get; set; }
    }
}
