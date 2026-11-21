using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dsw2026Tpi.Application.Interfaces
{
    public interface IAppointmentService
    {
        Task<AppointmentModel.Response> CreateAppointmentAsync(AppointmentModel.Request request); Task<List<AppointmentModel.Response>> GetPatientAppointmentsAsync(int dni);
        Task CancelAppointmentAsync(Guid appointmentId);
        Task<IEnumerable<AppointmentModel.ByDateResponse>> GetAppointmentsByDateAsync(DateTime date);
        Task<Pagination<AppointmentModel.SearchResponse>> SearchAppointmentsAsync(Guid? specialtyId, Guid? doctorId, int? dni, DateTime? date, int pageIndex, int pageSize);
    }
}