using Microsoft.EntityFrameworkCore;
using Transacciones.Core.Entities;

namespace Transacciones.Infrastructure.Persistence;

public class TransaccionesDbContext : DbContext {
	public TransaccionesDbContext(DbContextOptions<TransaccionesDbContext> options)
		: base(options) {
	}

	public DbSet<Cuenta> Cuentas { get; set; }
	public DbSet<Transaccion> Transacciones { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder) {
		base.OnModelCreating(modelBuilder);

		// Configuración de Cuenta
		modelBuilder.Entity<Cuenta>(entity => {
			entity.HasKey(e => e.Id);
			entity.Property(e => e.NumeroCuenta)
				.IsRequired()
				.HasMaxLength(50);
			entity.Property(e => e.Titular)
				.IsRequired()
				.HasMaxLength(200);
			entity.Property(e => e.Saldo)
				.HasPrecision(18, 2);
			entity.Property(e => e.RowVersion)
				.IsRowVersion();

			// Índice único para NumeroCuenta
			entity.HasIndex(e => e.NumeroCuenta)
				.IsUnique();

			// Relación con Transacciones
			entity.HasMany(e => e.Transacciones)
				.WithOne(e => e.Cuenta)
				.HasForeignKey(e => e.CuentaId)
				.OnDelete(DeleteBehavior.Restrict);
		});

		// Configuración de Transaccion
		modelBuilder.Entity<Transaccion>(entity => {
			entity.HasKey(e => e.Id);
			entity.Property(e => e.TipoTransaccion)
				.IsRequired()
				.HasMaxLength(20);
			entity.Property(e => e.Monto)
				.HasPrecision(18, 2);
			entity.Property(e => e.SaldoAnterior)
				.HasPrecision(18, 2);
			entity.Property(e => e.SaldoNuevo)
				.HasPrecision(18, 2);
			entity.Property(e => e.Descripcion)
				.HasMaxLength(500);

			// Índice para búsquedas por cuenta
			entity.HasIndex(e => e.CuentaId);
			entity.HasIndex(e => e.FechaTransaccion);
		});
	}
}





