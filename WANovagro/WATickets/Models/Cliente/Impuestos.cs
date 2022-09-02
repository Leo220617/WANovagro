namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Impuestos
    {
        public int id { get; set; }

        [StringLength(10)]
        public string Codigo { get; set; }

        [Column(TypeName = "money")]
        public decimal Tarifa { get; set; }
    }
}
