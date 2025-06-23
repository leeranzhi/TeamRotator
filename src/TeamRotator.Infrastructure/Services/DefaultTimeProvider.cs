using TeamRotator.Core.Interfaces;

namespace TeamRotator.Infrastructure.Services;

public class DefaultTimeProvider : ITimeProvider
{
    public DateOnly GetCurrentDate()
    {
        return DateOnly.FromDateTime(DateTime.Now);
    }
} 