namespace WATickets.Models.Cliente
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Productos
    {
        [Key]
        [StringLength(50)]
        public string Codigo { get; set; }

        public int idBodega { get; set; }

        public int idImpuesto { get; set; }

        public int idListaPrecios { get; set; }

        [StringLength(50)]
        public string Nombre { get; set; }

        [Column(TypeName = "money")]
        public decimal PrecioUnitario { get; set; }

        public int UnidadMedida { get; set; }

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
    }
}
