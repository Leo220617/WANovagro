namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CierreCajas
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int idUsuario { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int idCaja { get; set; }

        [Key]
        [Column(Order = 2)]
        public DateTime FechaCaja { get; set; }

        public DateTime FecUltAct { get; set; }

        [StringLength(20)]
        public string IP { get; set; }

        [Column(TypeName = "money")]
        public decimal EfectivoColones { get; set; }

        [Column(TypeName = "money")]
        public decimal ChequesColones { get; set; }

        [Column(TypeName = "money")]
        public decimal TarjetasColones { get; set; }

        [Column(TypeName = "money")]
        public decimal OtrosMediosColones { get; set; }

        [Column(TypeName = "money")]
        public decimal TotalVendidoColones { get; set; }

        [Column(TypeName = "money")]
        public decimal TotalRegistradoColones { get; set; }

        [Column(TypeName = "money")]
        public decimal TotalAperturaColones { get; set; }

        [Column(TypeName = "money")]
        public decimal EfectivoFC { get; set; }

        [Column(TypeName = "money")]
        public decimal ChequesFC { get; set; }

        [Column(TypeName = "money")]
        public decimal TarjetasFC { get; set; }

        [Column(TypeName = "money")]
        public decimal OtrosMediosFC { get; set; }

        [Column(TypeName = "money")]
        public decimal TotalVendidoFC { get; set; }

        [Column(TypeName = "money")]
        public decimal TotalRegistradoFC { get; set; }

        [Column(TypeName = "money")]
        public decimal TotalAperturaFC { get; set; }

        [Column(TypeName = "money")]
        public decimal TransferenciasColones { get; set; }

        [Column(TypeName = "money")]
        public decimal TransferenciasDolares { get; set; }

        public bool Activo { get; set; }

        public DateTime HoraCierre { get; set; }

        public decimal TotalizadoMonedas { get; set; }

        [Column(TypeName = "money")]
        public decimal EfectivoColonesC { get; set; }

        [Column(TypeName = "money")]
        public decimal ChequesColonesC { get; set; }

        [Column(TypeName = "money")]
        public decimal TarjetasColonesC { get; set; }

        [Column(TypeName = "money")]
        public decimal OtrosMediosColonesC { get; set; }

     

    

        [Column(TypeName = "money")]
        public decimal EfectivoFCC { get; set; }

        [Column(TypeName = "money")]
        public decimal ChequesFCC { get; set; }

        [Column(TypeName = "money")]
        public decimal TarjetasFCC { get; set; }

        [Column(TypeName = "money")]
        public decimal OtrosMediosFCC { get; set; }

    



        [Column(TypeName = "money")]
        public decimal TransferenciasColonesC { get; set; }

        [Column(TypeName = "money")]
        public decimal TransferenciasDolaresC { get; set; }

        [Column(TypeName = "money")]
        public decimal NotasCreditoColones { get; set; }

        [Column(TypeName = "money")]
        public decimal NotasCreditoFC { get; set; }
    }
}
