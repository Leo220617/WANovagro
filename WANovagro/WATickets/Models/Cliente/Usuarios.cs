namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Usuarios
    {
        public int id { get; set; }

        public int idRol { get; set; }

        [StringLength(50)]
        public string Nombre { get; set; }

        [StringLength(50)]
        public string NombreUsuario { get; set; }

        public string Clave { get; set; }

        [StringLength(20)]
        public string ClaveSupervision { get; set; }

        public DateTime FecUltSup { get; set; }

        public bool Activo { get; set; }

        public bool novapos { get; set; }

        public int idVendedor { get; set; }

        public decimal Descuento { get; set; }
        public string PIN { get; set; }

    }
}
