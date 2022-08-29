﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WATickets.Models
{
    public class Filtros
    {
        public string Texto { get; set; }
        public string CardName { get; set; }
        public string CardCode { get; set; }
        public int Codigo1 { get; set; }
        public int Codigo2 { get; set; }
        public int Codigo3 { get; set; }
        public string ListPrice { get; set; }
        public string ItemCode { get; set; }
        public string Categoria { get; set; }
        public DateTime FechaInicial { get; set; }
        public DateTime FechaFinal { get; set; }

    }
}