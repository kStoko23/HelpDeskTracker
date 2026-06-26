using Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();
        builder.Property(x => x.Email)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(x => x.Username)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(x => x.PasswordHash)
            .IsRequired();
        builder.Property(x => x.Role)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(x=>x.IsActive)
            .HasDefaultValue(true)
            .IsRequired();
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.Email).IsUnique();
    }
}