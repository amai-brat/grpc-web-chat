namespace Chat.Service.Models;

public class Message
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string SenderName { get; set; } = null!;
    public string Text { get; set; } = null!;

    public MessageResponse ToResponse() => new()
    {
        SenderName = SenderName,
        Text = Text
    };

    public static Message From(MessageRequest request, string senderName)
    {
        return new Message
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            SenderName = senderName,
            Text = request.Text
        };
    }
}