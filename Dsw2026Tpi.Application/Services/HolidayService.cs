using System.Text.Json;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;

namespace Dsw2026Tpi.Application.Services
{
    public class HolidayService : IHolidayService
    {
        private readonly IEnumerable<HolidayModel> _holidays;

        public HolidayService()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sources", "holidays.json");

            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                _holidays = JsonSerializer.Deserialize<IEnumerable<HolidayModel>>(json) ?? new List<HolidayModel>();
            }
            else
            {
                _holidays = new List<HolidayModel>();
            }
        }

        public IEnumerable<HolidayModel> GetHolidays()
        {
            return _holidays;
        }
        public bool IsHoliday(DateTime date)
        {
            return _holidays.Any(f => f.Date.Date == date.Date);
        }
    }
}
