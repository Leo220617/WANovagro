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

            modelBuilder.Entity<Impuestos>()
                .Property(e => e.Tarifa)
                .HasPrecision(4, 2);

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
