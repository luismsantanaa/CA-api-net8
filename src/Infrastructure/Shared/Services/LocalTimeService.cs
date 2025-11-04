using Microsoft.Extensions.Configuration;
using Shared.Services.Contracts;

namespace Shared.Services
{
    public class LocalTimeService : ILocalTimeService
    {
        private string _timeZoneId = string.Empty;

        public LocalTimeService(IConfiguration configuration)
        {
            _timeZoneId = configuration["LocalTimeZoneId"]!;
        }

        public DateTime LocalTime => GetLocalTime();

        private DateTime GetLocalTime()
        {
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(_timeZoneId);
            return timeZoneInfo == null
                ? throw new ArgumentException("Not TimeZoneInfo was founded with the id " + _timeZoneId)
                : TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo);
        }
    }
}
