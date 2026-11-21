using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{

    public class AvailabilitySlot : DeleteEntityBase
    {
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public SlotStatus Status { get; set; }
        public Guid? AvailabilityId { get; set; }
        public Availability Availability { get; set; }
        private AvailabilitySlot()
        {

        }
        public AvailabilitySlot(DateOnly date, TimeOnly startTime, TimeOnly endTime, Guid? availabilityId, Guid? id = null) : base(id)
        {
            Date = date;
            StartTime = startTime;
            EndTime = endTime;
            AvailabilityId = availabilityId;
            Status = 0;
        }
    }
}

