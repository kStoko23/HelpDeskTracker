using Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.Configurations;

public class TicketHistoryConfiguration : IEntityTypeConfiguration<TicketHistory>
{
    public void Configure(EntityTypeBuilder<TicketHistory> builder)
    {
        builder.ToTable("TicketHistories");
        
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();
        builder.Property(x => x.TicketId)
            .IsRequired();
        builder.Property(x=>x.ChangedById)
            .IsRequired();
        builder.Property(x=>x.ChangedAt)
            .IsRequired();
        builder.Property(x=>x.Field)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(x=>x.OldValue)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(x=>x.NewValue)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasOne(x => x.Ticket)
            .WithMany(x => x.TicketHistories)
            .HasForeignKey(x => x.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ChangedBy)
            .WithMany(x => x.TicketHistories)
            .HasForeignKey(x => x.ChangedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.TicketId);
        builder.HasIndex(x => x.ChangedById);
        builder.HasIndex(x => x.ChangedAt);
    }
}