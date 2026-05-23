namespace AirWatch.Domain.Entities;

public class Alert
{
    public Guid Id          { get; set; }
    public Guid DeviceId    { get; set; }
    public Device Device    { get; set; } = null!;
    public Guid UserId      { get; set; }
    public User User        { get; set; } = null!;
    public DateTime TriggeredAt { get; set; }
    public bool EmailSent   { get; set; }
}
