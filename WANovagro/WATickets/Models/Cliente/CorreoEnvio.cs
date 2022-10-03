namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("CorreoEnvio")]
    public partial class CorreoEnvio
    {
        public int id { get; set; }

        [StringLength(500)]
        public string RecepcionHostName { get; set; }

        public int EnvioPort { get; set; }

        public bool RecepcionUseSSL { get; set; }

        [StringLength(500)]
        public string RecepcionEmail { get; set; }

        [StringLength(500)]
        public string RecepcionPassword { get; set; }

        [StringLength(3)]
        public string CodSuc { get; set; }
    }
}
