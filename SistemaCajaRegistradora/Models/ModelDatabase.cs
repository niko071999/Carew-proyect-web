using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace SistemaCajaRegistradora.Models
{
    public partial class ModelDatabase : DbContext
    {
        public ModelDatabase()
            : base("name=ModelDatabase")
        {
        }

        public virtual DbSet<Categoria> Categoria { get; set; }
        public virtual DbSet<MetodoPago> MetodoPago { get; set; }
        public virtual DbSet<Prioridad> Prioridad { get; set; }
        public virtual DbSet<Producto> Producto { get; set; }
        public virtual DbSet<Rol> Rol { get; set; }
        public virtual DbSet<Usuario> Usuario { get; set; }
        public virtual DbSet<Venta> Venta { get; set; }
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
                .HasMany(e => e.Producto)
                .WithRequired(e => e.Categoria)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<MetodoPago>()
                .Property(e => e.metodo_pago)
                .IsFixedLength();

            modelBuilder.Entity<MetodoPago>()
                .HasMany(e => e.Venta)
                .WithRequired(e => e.MetodoPago)
                .HasForeignKey(e => e.metodo_pagoid)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Prioridad>()
                .Property(e => e.prioridad1)
                .IsFixedLength();

            modelBuilder.Entity<Prioridad>()
                .HasMany(e => e.Producto)
                .WithRequired(e => e.Prioridad)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Producto>()
                .Property(e => e.nombre)
                .IsFixedLength();

            modelBuilder.Entity<Producto>()
                .Property(e => e.rutaImg)
                .IsFixedLength();

            modelBuilder.Entity<Producto>()
                .HasMany(e => e.venta_producto)
                .WithRequired(e => e.Producto)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Rol>()
                .Property(e => e.rol1)
                .IsFixedLength();

            modelBuilder.Entity<Rol>()
                .HasMany(e => e.Usuario)
                .WithRequired(e => e.Rol)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Usuario>()
                .Property(e => e.nombreUsuario)
                .IsFixedLength();

            modelBuilder.Entity<Usuario>()
                .Property(e => e.nombreCajero)
                .IsFixedLength();

            modelBuilder.Entity<Usuario>()
                .Property(e => e.apellidoCajero)
                .IsFixedLength();

            modelBuilder.Entity<Usuario>()
                .Property(e => e.rutaImg)
                .IsFixedLength();

            modelBuilder.Entity<Usuario>()
                .Property(e => e.contraseña)
                .IsFixedLength();

            modelBuilder.Entity<Usuario>()
                .HasMany(e => e.Venta)
                .WithRequired(e => e.Usuario)
                .HasForeignKey(e => e.cajeroid)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Venta>()
                .HasMany(e => e.venta_producto)
                .WithRequired(e => e.Venta)
                .WillCascadeOnDelete(false);
        }
    }
}
