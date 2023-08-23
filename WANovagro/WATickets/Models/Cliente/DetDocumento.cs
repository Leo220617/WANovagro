
﻿
namespace WATickets.Models.Cliente
{

    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    [Table("DetDocumento")]

    public partial class DetDocumento
    {
        public int id { get; set; }
        public int idEncabezado { get; set; }
        public int idProducto { get; set; }
        public int NumLinea { get; set; }
        public decimal Cantidad { get; set; }
        public decimal TotalImpuesto { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal PorDescto { get; set; }
        public decimal Descuento { get; set; }
        public decimal TotalLinea { get; set; }
        public string Cabys { get; set; }
        public int idExoneracion { get; set; }
       
    }
}