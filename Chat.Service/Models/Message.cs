namespace Chat.Service.Models;

public class Message
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string SenderName { get; set; } = null!;
    public string Text { get; set; } = null!;
}