using System;
using Dsw2026Tpi.Application.Dtos;

namespace Dsw2026Tpi.Application.Interfaces
{
    public interface IHolidayService
    {
        IEnumerable<HolidayModel> GetHolidays();
        bool IsHoliday(DateTime date);
    }
}
