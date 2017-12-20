namespace FacturadorTaller.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("cotDor.FlujoCaja")]
    public partial class FlujoCaja
    {
        public int FlujoCajaId { get; set; }

        public string Categoria { get; set; }

        public string Descripcion { get; set; }

        public int? Valor { get; set; }

        public DateTime Fecha { get; set; }
    }
}
