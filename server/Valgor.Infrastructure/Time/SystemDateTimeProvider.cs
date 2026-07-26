using Valgor.Application.Common.Interfaces;

namespace Valgor.Infrastructure.Time;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
