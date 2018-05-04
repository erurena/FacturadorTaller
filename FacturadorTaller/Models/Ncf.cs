namespace FacturadorTaller.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("cotDor.Ncf")]
    public partial class Ncf
    {
        public int NcfId { get; set; }

        public string Inicio { get; set; }

        public int Contador { get; set; }

        public int NumInicio { get; set; }

        public int NumFin { get; set; }

        public int NumActual { get; set; }

        public string Estatus { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? NcfFecha { get; set; }
    }
}
