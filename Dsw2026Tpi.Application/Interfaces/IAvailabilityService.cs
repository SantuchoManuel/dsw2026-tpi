using Dsw2026Tpi.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using static Dsw2026Tpi.Application.Dtos.AvailabilityModel;

namespace Dsw2026Tpi.Application.Interfaces
{
    public interface IAvailabilityService
    {
        Task<AvailabilityModel.Request> CreateAvailabilityAsync(AvailabilityModel.Request request);
        Task<AvailabilityModel.Request> UpdateAvailabilityAsync(AvailabilityModel.Request request);
    }
}
