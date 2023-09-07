using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WATickets.Models.Cliente
{
    public class PreCierres
    {
        public int id { get; set; }
        public int idUsuario { get; set; }


        public int idCaja { get; set; }


        public DateTime FechaCaja { get; set; }

        public DateTime FecUltAct { get; set; }


        public string IP { get; set; }


        public decimal EfectivoColones { get; set; }


        public decimal ChequesColones { get; set; }

        public decimal TarjetasColones { get; set; }


        public decimal OtrosMediosColones { get; set; }


        public decimal TotalVendidoColones { get; set; }


        public decimal TotalRegistradoColones { get; set; }


        public decimal TotalAperturaColones { get; set; }


        public decimal EfectivoFC { get; set; }


        public decimal ChequesFC { get; set; }


        public decimal TarjetasFC { get; set; }


        public decimal OtrosMediosFC { get; set; }


        public decimal TotalVendidoFC { get; set; }


        public decimal TotalRegistradoFC { get; set; }


        public decimal TotalAperturaFC { get; set; }

        public bool Activo { get; set; }

        public DateTime HoraCierre { get; set; }

        public decimal TotalizadoMonedas { get; set; }


        public decimal TransferenciasColones { get; set; }


        public decimal TransferenciasDolares { get; set; }


        public decimal EfectivoColonesC { get; set; }


        public decimal ChequesColonesC { get; set; }


        public decimal TarjetasColonesC { get; set; }

        public decimal OtrosMediosColonesC { get; set; }






        public decimal EfectivoFCC { get; set; }


        public decimal ChequesFCC { get; set; }


        public decimal TarjetasFCC { get; set; }


        public decimal OtrosMediosFCC { get; set; }







        public decimal TransferenciasColonesC { get; set; }


        public decimal TransferenciasDolaresC { get; set; }
    }
}