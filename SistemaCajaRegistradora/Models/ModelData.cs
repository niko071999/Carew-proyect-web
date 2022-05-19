using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace SistemaCajaRegistradora.Models
{
    public partial class ModelData : DbContext
    {
        public ModelData()
            : base("name=ModelData")
        {
        }

        public virtual DbSet<Categoria> Categorias { get; set; }
        public virtual DbSet<MetodoPago> MetodoPagos { get; set; }
        public virtual DbSet<Prioridade> Prioridades { get; set; }
        public virtual DbSet<Producto> Productos { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Usuario> Usuarios { get; set; }
        public virtual DbSet<Venta> Ventas { get; set; }
        public virtual DbSet<venta_producto> venta_producto { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Categoria>()
                .Property(e => e.nombre)
                .IsFixedLength();

            modelBuilder.Entity<Categoria>()
                .Property(e => e.descripcion)
                .IsFixedLength();

            modelBuilder.Entity<Categoria>()
                .HasMany(e => e.Productos)
                .WithRequired(e => e.Categoria)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<MetodoPago>()
                .Property(e => e.metodo_pago)
                .IsFixedLength();

            modelBuilder.Entity<MetodoPago>()
                .HasMany(e => e.Ventas)
                .WithRequired(e => e.MetodoPago)
                .HasForeignKey(e => e.metodo_pagoid)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Prioridade>()
                .Property(e => e.prioridad)
                .IsFixedLength();

            modelBuilder.Entity<Prioridade>()
                .HasMany(e => e.Productos)
                .WithRequired(e => e.Prioridade)
                .HasForeignKey(e => e.prioridadid)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Producto>()
                .Property(e => e.codigo_barra)
                .IsFixedLength();

            modelBuilder.Entity<Producto>()
                .Property(e => e.nombre)
                .IsFixedLength();

            modelBuilder.Entity<Producto>()
                .Property(e => e.rutaImg)
                .IsFixedLength();

            modelBuilder.Entity<Role>()
                .Property(e => e.rol)
                .IsFixedLength();

            modelBuilder.Entity<Role>()
                .HasMany(e => e.Usuarios)
                .WithRequired(e => e.Role)
                .HasForeignKey(e => e.rolid)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Usuario>()
                .Property(e => e.nombreUsuario)
                .IsFixedLength();

            modelBuilder.Entity<Usuario>()
                .Property(e => e.clave)
                .IsFixedLength();

            modelBuilder.Entity<Usuario>()
                .Property(e => e.nombre)
                .IsFixedLength();

            modelBuilder.Entity<Usuario>()
                .Property(e => e.apellido)
                .IsFixedLength();

            modelBuilder.Entity<Usuario>()
                .Property(e => e.rutaImg)
                .IsFixedLength();

            modelBuilder.Entity<Usuario>()
                .HasMany(e => e.Ventas)
                .WithRequired(e => e.Usuario)
                .HasForeignKey(e => e.cajeroid)
                .WillCascadeOnDelete(false);
        }
    }
}
