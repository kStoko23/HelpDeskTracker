namespace Api.Entities;

public class Attachment
{
    public long Id { get; set; }
    public string FileName { get; set; } = null!;
    public string StoredPath { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long SizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
    public long? TicketId { get; set; }
    public Ticket? Ticket { get; set; }
    public long? CommentId { get; set; }
    public Comment? Comment { get; set; }
}