using Library.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Infrastructure.Persistence.Configurations;

public class BookTypeConfiguration : IEntityTypeConfiguration<BookType>
{
    public void Configure(EntityTypeBuilder<BookType> builder)
    {
        builder.ToTable("BookTypes");
        builder.HasKey(t => t.Id);
    }
}

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books");
        builder.HasKey(b => b.Id);
    }
}

public class BookBookTypeBridgeConfiguration : IEntityTypeConfiguration<BookBookTypeBridge>
{
    public void Configure(EntityTypeBuilder<BookBookTypeBridge> builder)
    {
        builder.ToTable("BookBookTypeBridge");   

        builder.HasKey(b => b.Id);

        builder.HasOne(b => b.Book)
            .WithOne(b => b!.BookBookType)
            .HasForeignKey<BookBookTypeBridge>(b => b.BookId);

        builder.HasOne(b => b.BookType)
            .WithMany(t => t.BookBookTypeBridges)
            .HasForeignKey(b => b.BookTypeId);
    }
}