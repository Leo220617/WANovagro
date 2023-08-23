

namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    [Table("Lotes")]

    public partial class Lotes
    {
        public int id { get; set; }
        public int idEncabezado { get; set; }
        public int idDetalle { get; set; }
        public string Tipo { get; set; }
        public string Serie { get; set; }
        public string ItemCode { get; set; }
        public int Cantidad { get; set; }
    }
}