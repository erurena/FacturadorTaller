namespace FacturadorTaller.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("cotDor.Clientes")]
    public partial class Clientes
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Clientes()
        {
            Cotizacion = new HashSet<Cotizacion>();
        }

        [Key]
        public int ClienteId { get; set; }

        public string NombreCliente { get; set; }

        public string RncCliente { get; set; }

        public string DireccionCliente { get; set; }

        public string ContactoCliente { get; set; }

        public string TelefonoCliente { get; set; }

        public string CorreoCliente { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Cotizacion> Cotizacion { get; set; }
    }
}
