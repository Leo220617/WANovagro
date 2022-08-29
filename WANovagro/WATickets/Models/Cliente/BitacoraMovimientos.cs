namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class BitacoraMovimientos
    {
        public int id { get; set; }

        public int? idUsuario { get; set; }

        [StringLength(500)]
        public string Descripcion { get; set; }

        public DateTime? Fecha { get; set; }

        [StringLength(100)]
        public string Metodo { get; set; }
    }
}
