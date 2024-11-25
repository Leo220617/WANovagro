namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Productos
    {
        public int id { get; set; }

        [StringLength(50)]
        public string Codigo { get; set; }

        public int idBodega { get; set; }

        public int idImpuesto { get; set; }

        public int idListaPrecios { get; set; }

        public int idCategoria { get; set; }

        [StringLength(500)]
        public string Nombre { get; set; }

        public string Moneda { get; set; }

        [Column(TypeName = "money")]
        public decimal PrecioUnitario { get; set; }

        [StringLength(5)]
        public string UnidadMedida { get; set; }

        [StringLength(13)]
        public string Cabys { get; set; }

        [StringLength(100)]
        public string TipoCod { get; set; }

        [StringLength(50)]
        public string CodBarras { get; set; }

        [Column(TypeName = "money")]
        public decimal Costo { get; set; }

        [Column(TypeName = "money")]
        public decimal Stock { get; set; }

        public bool Activo { get; set; }

        public bool ProcesadoSAP { get; set; }

        public DateTime FechaActualizacion { get; set; }

        public bool MAG { get; set; }

        public bool Editable { get; set; }

        public bool Serie { get; set; }

        public string NormaReparto { get; set; }

        public int Dimension { get; set; }

        public DateTime FechaConteo { get; set; }

        public string Localizacion { get; set; }

    }
}
