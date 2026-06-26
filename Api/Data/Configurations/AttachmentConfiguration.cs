using Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.Configurations;

public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable("Attachments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();
        builder.Property(x => x.FileName)
            .HasMaxLength(255)
            .IsRequired();
        builder.Property(x => x.StoredPath)
            .HasMaxLength(500)
            .IsRequired();
        builder.Property(x => x.ContentType)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(x => x.SizeBytes)
            .IsRequired();
        builder.Property(x => x.UploadedAt)
            .IsRequired();

        builder.Property(x => x.TicketId);

        builder.Property(x => x.CommentId);

        builder.HasOne(x => x.Ticket)
            .WithMany(x => x.Attachments)
            .HasForeignKey(x => x.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Comment)
            .WithMany(x => x.Attachments)
            .HasForeignKey(x => x.CommentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.TicketId);
        builder.HasIndex(x => x.CommentId);
        builder.HasIndex(x => x.UploadedAt);
    }
}