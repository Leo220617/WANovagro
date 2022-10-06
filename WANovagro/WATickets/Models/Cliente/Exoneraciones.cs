 

namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    [Table("Exoneraciones")]
    public partial class Exoneraciones
    {
        public int id { get; set; }
        public string TipoDoc { get; set; }
        public string NumDoc { get; set; }
        public string NomInst { get; set; }
        public DateTime FechaEmision { get; set; }
        public int PorExon { get; set; }
        public int idCliente { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public byte[] Imagen { get; set; }
        public bool Activo { get; set; }
    }
}