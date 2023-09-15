using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WATickets.Models.APIS
{
    public class SucursalesViewModel
    {
      
        [StringLength(3)]
        public string CodSuc { get; set; }

        [StringLength(100)]
        public string Nombre { get; set; }

        public string Imagen { get; set; }

        [StringLength(2)]
        public string TipoCedula { get; set; }

        [StringLength(12)]
        public string Cedula { get; set; }

        [StringLength(1)]
        public string Provincia { get; set; }

        [StringLength(2)]
        public string Canton { get; set; }

        [StringLength(2)]
        public string Distrito { get; set; }

        [StringLength(2)]
        public string Barrio { get; set; }

        [StringLength(250)]
        public string Sennas { get; set; }

        [StringLength(20)]
        public string Telefono { get; set; }

        [StringLength(100)]
        public string Correo { get; set; }

        public int SerieFECO { get; set; }

        public int SerieFECR { get; set; }

        public int SerieNC { get; set; }
        public int SeriePago { get; set; }

        public int SerieDeposito { get; set; }
        public int idListaPrecios { get; set; }

        public string NormaReparto { get; set; }

        public int Dimension { get; set; }
    }
}