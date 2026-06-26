using Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments");
        
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedOnAdd();
        builder.Property(c => c.Content)
            .HasMaxLength(2000)
            .IsRequired();
        builder.Property(c => c.CreatedAt)
            .IsRequired();
        builder.Property(c => c.TicketId)
            .IsRequired();
        builder.Property(c => c.AuthorId)
            .IsRequired();
        builder.Property(c => c.IsInternal)
            .HasDefaultValue(false)
            .IsRequired();

        builder.HasOne(x => x.Ticket)
            .WithMany(x => x.Comments)
            .HasForeignKey(x => x.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(x => x.Author)
            .WithMany(x => x.Comments)
            .HasForeignKey(x => x.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.TicketId);
        builder.HasIndex(x => x.AuthorId);
        builder.HasIndex(x => x.CreatedAt);
    }
}