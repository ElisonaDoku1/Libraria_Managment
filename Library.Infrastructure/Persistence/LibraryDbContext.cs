using System;
using System.Collections.Generic;
using System.Text;
using Library.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Persistence
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }

        public DbSet<Book> Books => Set<Book>();
        public DbSet<BookType> BookTypes => Set<BookType>();
        public DbSet<BookBookTypeBridge> BookBookTypeBridges => Set<BookBookTypeBridge>();

        public DbSet<Client> Clients => Set<Client>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<ClientOrderBridge> ClientOrderBridges => Set<ClientOrderBridge>();

        public DbSet<User> Users => Set<User>();
        public DbSet<UserType> UserTypes => Set<UserType>();
        public DbSet<UserRoles> UserRoles => Set<UserRoles>();
        public DbSet<UserRoleBridge> UserRoleBridges => Set<UserRoleBridge>();

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("db_accessadmin");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(LibraryDbContext).Assembly);

            
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
                foreach (var property in entity.GetProperties())
                {
                    if (property.Name == "Id") continue;

                    property.SetColumnName(char.ToLowerInvariant(property.Name[0]) + property.Name[1..]);

                }
        }
    }
}
