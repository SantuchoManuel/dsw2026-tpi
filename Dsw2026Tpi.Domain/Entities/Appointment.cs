using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{
    public class Appointment : EntityBase
    {
        public DateTime AppointmentDate { get; set; }
        public DateTime CancellationDate { get; set; }
        public AppointmentStatus Status { get; set; }
        public string Reason { get; set; }
        public Guid? PatientId { get; set; }
        public Patient Patient { get; set; }
        public Guid? AvailabilitySlotId { get; set; }
        public AvailabilitySlot AvailabilitySlot { get; set; }
        private Appointment()
        {

        }
        public Appointment(DateTime appointmentDate, DateTime cancellationDate, Guid? id) : base(id)
        {
            AppointmentDate = appointmentDate;
            CancellationDate = cancellationDate;
        }
    }
}
