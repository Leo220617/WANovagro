namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Clientes
    {
        [Key]
        public int id { get; set; }

        [StringLength(50)]
        public string Codigo { get; set; }

        public int idListaPrecios { get; set; }

        [StringLength(200)]
        public string Nombre { get; set; }

        [StringLength(2)]
        public string TipoCedula { get; set; }

        [StringLength(12)]
        public string Cedula { get; set; }

        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(50)]
        public string CodPais { get; set; }

        [StringLength(50)]
        public string Telefono { get; set; }

        public int Provincia { get; set; }

        [StringLength(2)]
        public string Canton { get; set; }

        [StringLength(2)]
        public string Distrito { get; set; }

        [StringLength(2)]
        public string Barrio { get; set; }

        [StringLength(250)]
        public string Sennas { get; set; }

        [Column(TypeName = "money")]
        public decimal Saldo { get; set; }

        public bool Activo { get; set; }

        public bool ProcesadoSAP { get; set; }
    }
}
