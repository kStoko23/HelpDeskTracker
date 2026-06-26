namespace Api.Entities;

public class Comment
{
    public long Id { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public long TicketId { get; set; }
    public Ticket Ticket { get; set; }
    public long AuthorId { get; set; }
    public User Author { get; set; }
    public bool IsInternal { get; set; }
    public ICollection<Attachment> Attachments { get; set; } = [];
}