namespace AirWatch.Domain.Entities;

public class Calibration
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public Device Device { get; set; } = null!;

    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public CalibrationStatus Status { get; set; }
    public string Location { get; set; } = string.Empty;

    public double? R0Mq3 { get; set; }
    public double? R0Mq5 { get; set; }
    public double? R0Mq135 { get; set; }

    public int SampleCount { get; set; }
    public int DuracaoSegundos { get; set; }

    public bool IsActive { get; set; }
}

public enum CalibrationStatus
{
    InProgress,
    Completed,
    Cancelled,
    Failed
}
