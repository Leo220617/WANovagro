namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Cajas
    {
        public int id { get; set; }

        [StringLength(3)]
        public string CodSuc { get; set; }

        [StringLength(20)]
        public string Nombre { get; set; }

        public decimal MontoAperturaColones { get; set; }

        public decimal MontoAperturaDolares { get; set; }

        public int idUsuario { get; set; }
    }
}
