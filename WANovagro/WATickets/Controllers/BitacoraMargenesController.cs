﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WATickets.Models;
using WATickets.Models.Cliente;

namespace WATickets.Controllers
{
    [Authorize]
    public class BitacoraMargenesController : ApiController
    {
        ModelCliente db = new ModelCliente();

        public async Task<HttpResponseMessage> Get([FromUri] Filtros filtro)
        {
            try
            {
                var time = new DateTime(); // La hora inicial 01/01/0001 00:00:00
                if (filtro.FechaFinal != time)
                {
                    filtro.FechaFinal = filtro.FechaFinal.AddDays(1);
                }

                var Bitacora = db.BitacoraMargenes.Where(a => (filtro.FechaInicial != time ? a.Fecha >= filtro.FechaInicial : true) && (filtro.FechaFinal != time ? a.Fecha <= filtro.FechaFinal : true)
    
                ).ToList();
                

                return Request.CreateResponse(HttpStatusCode.OK, Bitacora);

            }
            catch (Exception ex)
            {
                BitacoraErrores be = new BitacoraErrores();
                be.Descripcion = ex.Message;
                be.StrackTrace = ex.StackTrace;
                be.Fecha = DateTime.Now;
                be.JSON = JsonConvert.SerializeObject(ex);
                db.BitacoraErrores.Add(be);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}