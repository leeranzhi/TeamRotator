namespace TeamRotator.Core.Interfaces;

public interface ITimeProvider
{
    DateOnly GetCurrentDate();
} 