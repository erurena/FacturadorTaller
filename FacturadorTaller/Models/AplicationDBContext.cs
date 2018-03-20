namespace FacturadorTaller.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using Microsoft.AspNet.Identity.EntityFramework;
    public class ApplicationDBContext : IdentityDbContext<ApplicationUser>
    {
        
        public virtual DbSet<Clientes> Clientes { get; set; }
        public virtual DbSet<Cotizacion> Cotizacion { get; set; }
        public virtual DbSet<DetalleCot> DetalleCot { get; set; }
        public virtual DbSet<Factura> Factura { get; set; }
        public virtual DbSet<Pago> Pago { get; set; }
        public virtual DbSet<FlujoCaja> FlujoCaja { get; set; }
        public virtual DbSet<Ncf> Ncf { get; set; }
        public virtual DbSet<Producto> Producto { get; set; }
        public ApplicationDBContext()
                : base("name=AplicationDBContext")
        {
        }
        public static ApplicationDBContext Create()
        {
            return new ApplicationDBContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Clientes>()
                .HasMany(e => e.Cotizacion)
                .WithRequired(e => e.Clientes)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Cotizacion>()
                .Property(e => e.TotalFactura)
                .HasPrecision(18, 0);

            modelBuilder.Entity<Cotizacion>()
                .Property(e => e.Itbis)
                .HasPrecision(18, 0);

            modelBuilder.Entity<Cotizacion>()
                .HasMany(e => e.DetalleCot)
                .WithRequired(e => e.Cotizacion)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Cotizacion>()
                .HasMany(e => e.Factura)
                .WithRequired(e => e.Cotizacion)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DetalleCot>()
                .Property(e => e.Valor)
                .HasPrecision(18, 0);

            modelBuilder.Entity<Producto>()
                .Property(e => e.Precio)
                .HasPrecision(18, 0);

            modelBuilder.Entity<Pago>()
                .Property(e => e.MontoPago)
                .HasPrecision(18, 0);

            modelBuilder.Entity<Producto>()
                .HasMany(e => e.DetalleCot)
                .WithRequired(e => e.Producto)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<IdentityUser>().ToTable("Users", "cotDor");
            modelBuilder.Entity<IdentityUserRole>().ToTable("UserRoles", "cotDor").HasKey<string>(l => l.UserId);
            modelBuilder.Entity<IdentityUserLogin>().ToTable("ExternalLogins", "cotDor").HasKey<string>(l => l.UserId);
            modelBuilder.Entity<IdentityUserClaim>().ToTable("UserClaims", "cotDor");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles", "cotDor");
        }
    }
}
