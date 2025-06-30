namespace TeamRotator.Core.Entities;

public class SystemConfig
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
    public string? ModifiedBy { get; set; }
} 