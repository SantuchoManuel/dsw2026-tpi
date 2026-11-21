using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{
    public class Availability : DeleteEntityBase
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public Guid? DoctorId { get; set; }
        public Doctor Doctor { get; set; }
        public List<AvailabilitySlot> AvailabilitySlots { get; set; } = new();

        private Availability() 
        {
            
        }

        public Availability(int month, int year, DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime, Guid? doctorId, Guid? id = null) : base(id)
        {
            Month = month;
            Year = year;
            DayOfWeek = dayOfWeek;
            StartTime = startTime;
            EndTime = endTime;
            DoctorId = doctorId;
        }
    }
}
