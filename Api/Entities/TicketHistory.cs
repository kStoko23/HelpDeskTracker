namespace Api.Entities;

public class TicketHistory
{
    public long Id { get; set; }
    public long TicketId { get; set; }
    public Ticket Ticket { get; set; }
    public long ChangedById { get; set; }
    public User ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
    public string Field { get; set; }
    public string OldValue { get; set; }
    public string NewValue { get; set; }
}