namespace WATickets.Models.Cliente
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class ModelCliente : DbContext
    {
        public ModelCliente()
            : base("name=ModelCliente")
        {
        }

        public virtual DbSet<Barrios> Barrios { get; set; }
        public virtual DbSet<BitacoraErrores> BitacoraErrores { get; set; }
        public virtual DbSet<BitacoraMovimientos> BitacoraMovimientos { get; set; }
        public virtual DbSet<Bodegas> Bodegas { get; set; }
        public virtual DbSet<Cabys> Cabys { get; set; }
        public virtual DbSet<Cajas> Cajas { get; set; }
        public virtual DbSet<Cantones> Cantones { get; set; }
        public virtual DbSet<CierreCajas> CierreCajas { get; set; }
        public virtual DbSet<Clientes> Clientes { get; set; }
        public virtual DbSet<ConexionSAP> ConexionSAP { get; set; }
        public virtual DbSet<CorreoEnvio> CorreoEnvio { get; set; }
        public virtual DbSet<Distritos> Distritos { get; set; }
        public virtual DbSet<Impuestos> Impuestos { get; set; }
        public virtual DbSet<ListaPrecios> ListaPrecios { get; set; }
        public virtual DbSet<Productos> Productos { get; set; }
        public virtual DbSet<Roles> Roles { get; set; }
        public virtual DbSet<SeguridadModulos> SeguridadModulos { get; set; }
        public virtual DbSet<SeguridadRolesModulos> SeguridadRolesModulos { get; set; }
        public virtual DbSet<Sucursales> Sucursales { get; set; }
        public virtual DbSet<Usuarios> Usuarios { get; set; }
        public virtual DbSet<UsuariosSucursales> UsuariosSucursales { get; set; }
        public virtual DbSet<Parametros> Parametros { get; set; }
        public virtual DbSet<EncOferta> EncOferta { get; set; }
        public virtual DbSet<DetOferta> DetOferta { get; set; }
        public virtual DbSet<CondicionesPagos> CondicionesPagos { get; set; }
        public virtual DbSet<Exoneraciones> Exoneraciones { get; set; }
        public virtual DbSet<DetExoneraciones> DetExoneraciones { get; set; }
        public virtual DbSet<GruposClientes> GruposClientes { get; set; }
        public virtual DbSet<TipoCambios> TipoCambios { get; set; }
        public virtual DbSet<EncDocumento> EncDocumento { get; set; }
        public virtual DbSet<DetDocumento> DetDocumento { get; set; }
        public virtual DbSet<MetodosPagos> MetodosPagos { get; set; }
        public virtual DbSet<MetodosPagosCuentas> MetodosPagosCuentas { get; set; }
        public virtual DbSet<MetodosPagosAbonos> MetodosPagosAbonos { get; set; }
        public virtual DbSet<CuentasBancarias> CuentasBancarias { get; set; }
        public virtual DbSet<Vendedores> Vendedores { get; set; }
        public virtual DbSet<EncDocumentoCredito> EncDocumentoCredito { get; set; }
        public virtual DbSet<DetDocumentoCredito> DetDocumentoCredito { get; set; }
        public virtual DbSet<EncPagos> EncPagos { get; set; }
        public virtual DbSet<DetPagos> DetPagos { get; set; }
        public virtual DbSet<FECXDia> FECXDia { get; set; }
        public virtual DbSet<Depositos> Depositos { get; set; }
        public virtual DbSet<PagoCuentas> PagoCuentas { get; set; }
        public virtual DbSet<Lotes> Lotes { get; set; }
        public virtual DbSet<PreCierres> PreCierres { get; set; }
        public virtual DbSet<Asientos> Asientos { get; set; }
        public virtual DbSet<PrecioXLista> PrecioXLista { get; set; }
        public virtual DbSet<Categorias> Categorias { get; set; }
        public virtual DbSet<Promociones> Promociones { get; set; }
        public virtual DbSet<EncPromociones> EncPromociones { get; set; }
        public virtual DbSet<EncMargenes> EncMargenes { get; set; }
        public virtual DbSet<DetMargenes> DetMargenes { get; set; }
        public virtual DbSet<AprobacionesCreditos> AprobacionesCreditos { get; set; }
        public virtual DbSet<BitacoraMargenes> BitacoraMargenes { get; set; }
        public virtual DbSet<EncArqueos> EncArqueos { get; set; }
        public virtual DbSet<DetArqueos> DetArqueos { get; set; }
        public virtual DbSet<PalabrasClaves> PalabrasClaves { get; set; }
        public virtual DbSet<ClientesPromociones> ClientesPromociones { get; set; }
        public virtual DbSet<SubCategorias> SubCategorias { get; set; }
        public virtual DbSet<LogsProductosAprov> LogsProductosAprov { get; set; }
        public virtual DbSet<EncAprovisionamiento> EncAprovisionamiento { get; set; }
        public virtual DbSet<DetAprovisionamiento> DetAprovisionamiento { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Barrios>()
                .Property(e => e.NomBarrio)
                .IsUnicode(false);

            modelBuilder.Entity<BitacoraErrores>()
                .Property(e => e.Descripcion)
                .IsUnicode(false);

            modelBuilder.Entity<BitacoraErrores>()
                .Property(e => e.StrackTrace)
                .IsUnicode(false);

            modelBuilder.Entity<BitacoraErrores>()
                .Property(e => e.JSON)
                .IsUnicode(false);

            modelBuilder.Entity<BitacoraMovimientos>()
                .Property(e => e.Descripcion)
                .IsUnicode(false);

            modelBuilder.Entity<BitacoraMovimientos>()
                .Property(e => e.Metodo)
                .IsUnicode(false);

            modelBuilder.Entity<Bodegas>()
                .Property(e => e.CodSuc)
                .IsUnicode(false);

            modelBuilder.Entity<Bodegas>()
                .Property(e => e.CodSAP)
                .IsUnicode(false);

            modelBuilder.Entity<Bodegas>()
                .Property(e => e.Nombre)
                .IsUnicode(false);

            modelBuilder.Entity<Cabys>()
                .Property(e => e.Descripcion)
                .IsUnicode(false);

            modelBuilder.Entity<Cabys>()
                .Property(e => e.CodCabys)
                .IsUnicode(false);

            modelBuilder.Entity<Cajas>()
                .Property(e => e.CodSuc)
                .IsUnicode(false);

            modelBuilder.Entity<Cajas>()
                .Property(e => e.Nombre)
                .IsUnicode(false);

            modelBuilder.Entity<Cantones>()
                .Property(e => e.NomCanton)
                .IsUnicode(false);

            modelBuilder.Entity<CierreCajas>()
                .Property(e => e.IP)
                .IsUnicode(false);

            modelBuilder.Entity<CierreCajas>()
                .Property(e => e.EfectivoColones)
                .HasPrecision(19, 4);

            modelBuilder.Entity<CierreCajas>()
                .Property(e => e.ChequesColones)
                .HasPrecision(19, 4);

            modelBuilder.Entity<CierreCajas>()
                .Property(e => e.TarjetasColones)
                .HasPrecision(19, 4);

            modelBuilder.Entity<CierreCajas>()
                .Property(e => e.OtrosMediosColones)
                .HasPrecision(19, 4);

            modelBuilder.Entity<CierreCajas>()
                .Property(e => e.TotalVendidoColones)
                .HasPrecision(19, 4);

            modelBuilder.Entity<CierreCajas>()
                .Property(e => e.TotalRegistradoColones)
                .HasPrecision(19, 4);

            modelBuilder.Entity<CierreCajas>()
                .Property(e => e.TotalAperturaColones)
                .HasPrecision(19, 4);

            modelBuilder.Entity<CierreCajas>()
                .Property(e => e.EfectivoFC)
                .HasPrecision(19, 4);

            modelBuilder.Entity<CierreCajas>()
                .Property(e => e.ChequesFC)
                .HasPrecision(19, 4);

            modelBuilder.Entity<CierreCajas>()
                .Property(e => e.TarjetasFC)
                .HasPrecision(19, 4);

            modelBuilder.Entity<CierreCajas>()
                .Property(e => e.OtrosMediosFC)
                .HasPrecision(19, 4);

            modelBuilder.Entity<CierreCajas>()
                .Property(e => e.TotalVendidoFC)
                .HasPrecision(19, 4);

            modelBuilder.Entity<CierreCajas>()
                .Property(e => e.TotalRegistradoFC)
                .HasPrecision(19, 4);

            modelBuilder.Entity<CierreCajas>()
                .Property(e => e.TotalAperturaFC)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Clientes>()
                .Property(e => e.Codigo)
                .IsUnicode(false);

            modelBuilder.Entity<Clientes>()
                .Property(e => e.Nombre)
                .IsUnicode(false);

            modelBuilder.Entity<Clientes>()
                .Property(e => e.TipoCedula)
                .IsUnicode(false);

            modelBuilder.Entity<Clientes>()
                .Property(e => e.Cedula)
                .IsUnicode(false);

            modelBuilder.Entity<Clientes>()
                .Property(e => e.Email)
                .IsUnicode(false);

            modelBuilder.Entity<Clientes>()
                .Property(e => e.CodPais)
                .IsUnicode(false);

            modelBuilder.Entity<Clientes>()
                .Property(e => e.Telefono)
                .IsUnicode(false);

            modelBuilder.Entity<Clientes>()
                .Property(e => e.Canton)
                .IsUnicode(false);

            modelBuilder.Entity<Clientes>()
                .Property(e => e.Distrito)
                .IsUnicode(false);

            modelBuilder.Entity<Clientes>()
                .Property(e => e.Barrio)
                .IsUnicode(false);

            modelBuilder.Entity<Clientes>()
                .Property(e => e.Sennas)
                .IsUnicode(false);

            modelBuilder.Entity<Clientes>()
                .Property(e => e.Saldo)
                .HasPrecision(19, 4);

            modelBuilder.Entity<ConexionSAP>()
                .Property(e => e.SAPUser)
                .IsUnicode(false);

            modelBuilder.Entity<ConexionSAP>()
                .Property(e => e.SAPPass)
                .IsUnicode(false);

            modelBuilder.Entity<ConexionSAP>()
                .Property(e => e.SQLUser)
                .IsUnicode(false);

            modelBuilder.Entity<ConexionSAP>()
                .Property(e => e.ServerSQL)
                .IsUnicode(false);

            modelBuilder.Entity<ConexionSAP>()
                .Property(e => e.ServerLicense)
                .IsUnicode(false);

            modelBuilder.Entity<ConexionSAP>()
                .Property(e => e.SQLPass)
                .IsUnicode(false);

            modelBuilder.Entity<ConexionSAP>()
                .Property(e => e.SQLType)
                .IsUnicode(false);

            modelBuilder.Entity<ConexionSAP>()
                .Property(e => e.SQLBD)
                .IsUnicode(false);

            modelBuilder.Entity<CorreoEnvio>()
                .Property(e => e.RecepcionHostName)
                .IsUnicode(false);

            modelBuilder.Entity<CorreoEnvio>()
                .Property(e => e.RecepcionEmail)
                .IsUnicode(false);

            modelBuilder.Entity<CorreoEnvio>()
                .Property(e => e.RecepcionPassword)
                .IsUnicode(false);

            modelBuilder.Entity<Distritos>()
                .Property(e => e.NomDistrito)
                .IsUnicode(false);

            modelBuilder.Entity<Impuestos>()
                .Property(e => e.Codigo)
                .IsUnicode(false);

         

            modelBuilder.Entity<ListaPrecios>()
                .Property(e => e.CodSAP)
                .IsUnicode(false);

            modelBuilder.Entity<ListaPrecios>()
                .Property(e => e.Nombre)
                .IsUnicode(false);

            modelBuilder.Entity<Productos>()
                .Property(e => e.Codigo)
                .IsUnicode(false);

            modelBuilder.Entity<Productos>()
                .Property(e => e.Nombre)
                .IsUnicode(false);

            modelBuilder.Entity<Productos>()
                .Property(e => e.PrecioUnitario)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Productos>()
                .Property(e => e.Cabys)
                .IsUnicode(false);

            modelBuilder.Entity<Productos>()
                .Property(e => e.TipoCod)
                .IsUnicode(false);

            modelBuilder.Entity<Productos>()
                .Property(e => e.CodBarras)
                .IsUnicode(false);

            modelBuilder.Entity<Productos>()
                .Property(e => e.Costo)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Productos>()
                .Property(e => e.Stock)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Roles>()
                .Property(e => e.NombreRol)
                .IsUnicode(false);

            modelBuilder.Entity<SeguridadModulos>()
                .Property(e => e.Descripcion)
                .IsUnicode(false);

            modelBuilder.Entity<Sucursales>()
                .Property(e => e.CodSuc)
                .IsUnicode(false);

            modelBuilder.Entity<Sucursales>()
                .Property(e => e.Nombre)
                .IsUnicode(false);

            modelBuilder.Entity<Sucursales>()
                .Property(e => e.TipoCedula)
                .IsUnicode(false);

            modelBuilder.Entity<Sucursales>()
                .Property(e => e.Cedula)
                .IsUnicode(false);

            modelBuilder.Entity<Sucursales>()
                .Property(e => e.Provincia)
                .IsUnicode(false);

            modelBuilder.Entity<Sucursales>()
                .Property(e => e.Canton)
                .IsUnicode(false);

            modelBuilder.Entity<Sucursales>()
                .Property(e => e.Distrito)
                .IsUnicode(false);

            modelBuilder.Entity<Sucursales>()
                .Property(e => e.Barrio)
                .IsUnicode(false);

            modelBuilder.Entity<Sucursales>()
                .Property(e => e.Sennas)
                .IsUnicode(false);

            modelBuilder.Entity<Sucursales>()
                .Property(e => e.Telefono)
                .IsUnicode(false);

            modelBuilder.Entity<Sucursales>()
                .Property(e => e.Correo)
                .IsUnicode(false);

            modelBuilder.Entity<Usuarios>()
                .Property(e => e.Nombre)
                .IsUnicode(false);

            modelBuilder.Entity<Usuarios>()
                .Property(e => e.NombreUsuario)
                .IsUnicode(false);

            modelBuilder.Entity<Usuarios>()
                .Property(e => e.Clave)
                .IsUnicode(false);

            modelBuilder.Entity<Usuarios>()
                .Property(e => e.ClaveSupervision)
                .IsUnicode(false);

            modelBuilder.Entity<UsuariosSucursales>()
                .Property(e => e.CodSuc)
                .IsUnicode(false);

           
        }
    }
}
