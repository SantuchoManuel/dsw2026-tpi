using Dsw2026Tpi.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Dsw2026Tpi.Domain.Entities;

public class Doctor: DeleteEntityBase
{
    public string Name { get; set; }
    public string LicenseNumber { get; set; }
    public bool IsActive { get; private set; }
    public Guid? SpecialityId { get; set; }
    public Speciality? Speciality { get; private set; }

    public List<Disponibilidad> Disponibilidades { get; private set; } = new();

    #region Constructor for EF
#pragma warning disable CS8618
    private Doctor()
    {
    }
#pragma warning restore CS8618
    #endregion

    public Doctor(string name, string licenseNumber, Speciality speciality, Guid? id = null) : base(id)
    {
        Name = name;
        LicenseNumber = licenseNumber;
        Speciality = speciality;
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public Disponibilidad CreteAvalibities(int month,int year,DayOfWeek dayOfWeek,TimeOnly startTime,TimeOnly endTime,DateTime fromDate)
    {

        var nuevaDisponibilidad = new Disponibilidad(
            month, year, dayOfWeek, startTime, endTime, null);


        Disponibilidades.Add(nuevaDisponibilidad);

        return nuevaDisponibilidad;
    }

    public bool CheckAvailable(int month, int year, DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime)
    {
        return Disponibilidades.Any(d =>
            d.Mes == month &&
            d.Año == year &&
            d.DiaDeLaSemana == dayOfWeek &&
            d.HoraDeEntrada < endTime &&
            d.HoraDeSalida > startTime);
    }
}
