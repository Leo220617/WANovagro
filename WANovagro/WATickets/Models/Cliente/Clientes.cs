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

        public int idCondicionPago { get; set; }

      

        public string CorreoPublicitario { get; set; }

        public int idGrupo { get; set; }

        public DateTime FechaActualizacion { get; set; }

        public bool MAG { get; set; }

        public bool INT { get; set; }

        public decimal LimiteCredito { get; set; }

        public decimal Descuento { get; set; }

       
        public string CorreoEC { get; set; }

        public bool CxC { get; set; }

        public bool Transitorio { get; set; }


        public string DV { get; set; }
        public decimal DiasGracia { get; set; }

        public decimal MontoExtra { get; set; }

    }
       
}
