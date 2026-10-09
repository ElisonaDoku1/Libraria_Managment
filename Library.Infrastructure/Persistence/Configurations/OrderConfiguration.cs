using Library.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);
    }
}

public class ClientOrderBridgeConfiguration : IEntityTypeConfiguration<ClientOrderBridge>
{
    public void Configure(EntityTypeBuilder<ClientOrderBridge> builder)
    {
        builder.ToTable("ClientOrderBridge");
        builder.HasKey(b => b.Id);

        builder.HasOne(b => b.Book)
            .WithMany(b => b.ClientOrderBridges)
            .HasForeignKey(b => b.BookId);

        builder.HasOne(b => b.Client)
            .WithMany(c => c.ClientOrderBridges)
            .HasForeignKey(b => b.ClientId);

        builder.HasOne(b => b.Order)
            .WithMany(o => o.ClientOrderBridges)
            .HasForeignKey(b => b.OrderId);
    }
}
