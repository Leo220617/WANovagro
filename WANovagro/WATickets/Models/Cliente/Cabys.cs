namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Cabys
    {
        public int id { get; set; }

        [StringLength(500)]
        public string Descripcion { get; set; }

        [StringLength(13)]
        public string CodCabys { get; set; }
    }
}
