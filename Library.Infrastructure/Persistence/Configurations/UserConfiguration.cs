using Library.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Infrastructure.Persistence.Configurations;

public class UserTypeConfiguration : IEntityTypeConfiguration<UserType>
{
    public void Configure(EntityTypeBuilder<UserType> builder)
    {
        builder.ToTable("UserTypes");
        builder.HasKey(t => t.Id);
    }
}

public class UserRolesConfiguration : IEntityTypeConfiguration<UserRoles>
{
    public void Configure(EntityTypeBuilder<UserRoles> builder)
    {
        builder.ToTable("UserRoles");
        builder.HasKey(r => r.Id);
    }
}

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);

        builder.HasOne(u => u.UserType)
            .WithMany(t => t.Users)
            .HasForeignKey(u => u.UserTypeId);
    }
}

public class UserRoleBridgeConfiguration : IEntityTypeConfiguration<UserRoleBridge>
{
    public void Configure(EntityTypeBuilder<UserRoleBridge> builder)
    {
        builder.ToTable("UserRoleBridge");
        builder.HasKey(b => new { b.UserId, b.UserRoleId });

        builder.HasOne(b => b.User)
            .WithMany(u => u.UserRoleBridges)
            .HasForeignKey(b => b.UserId);

        builder.HasOne(b => b.UserRole)
            .WithMany(r => r.UserRoleBridges)
            .HasForeignKey(b => b.UserRoleId);
    }
}
