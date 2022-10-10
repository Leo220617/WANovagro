
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WATickets.Models.Cliente;

namespace WATickets.Models.APIS
{
    public class Documentos
    {
        public int id { get; set; }
        public int idCliente { get; set; }
        public int idUsuarioCreador { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string Comentarios { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TotalImpuestos { get; set; }
        public decimal TotalDescuento { get; set; }
        public decimal TotalCompra { get; set; }
        public decimal PorDescto { get; set; }
        public string Status { get; set; }
        public string CodSuc { get; set; }
        public string Moneda { get; set; }
        public string TipoDocumento { get; set; }
        public List<MetodosPagos> MetodosPagos { get; set; }
        public List<DetDocumento> Detalle { get; set; }
    }
}