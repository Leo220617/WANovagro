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

        [StringLength(2)]
        public string Codigo { get; set; }

        public decimal? Tarifa { get; set; }
    }
}
