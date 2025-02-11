﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WATickets.Models.Cliente;

namespace WATickets.Models.APIS
{
    public class PagoCuentaViewModel
    {
        public int id { get; set; }
        public int idCliente { get; set; }
        public string CodSuc { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public DateTime FechaContabilizacion { get; set; }
        public string Comentarios { get; set; }
        public decimal Total { get; set; }
        public string Moneda { get; set; }
        public bool ProcesadaSAP { get; set; }
        public string DocEntry { get; set; }
        public int idCaja { get; set; }
        public int idUsuarioCreador { get; set; }
        public int idCuentaBancaria { get; set; }

        public List<MetodosPagosCuentas> MetodosPagosCuentas { get; set; }
    }
}