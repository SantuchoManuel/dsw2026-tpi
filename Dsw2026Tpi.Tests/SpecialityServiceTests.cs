using System;
using System.Collections.Generic;
using System.Text;
using Azure;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using NSubstitute;
using NSubstitute.ReturnsExtensions;


namespace Dsw2026Tpi.Tests;

public class SpecialityServiceTests
{
    private readonly IPersistence _mockPersistence = Substitute.For<IPersistence>();

    [Theory]
    [InlineData("Ca", "Especialidad con nombre demasiado corto")]
    [InlineData("C", "Especialidad con nombre demasiado corto")]
    [InlineData("NombreDemasiadoLargoQueSuperaLosCienCaracteresPermitidosParaElCampoNombreDeLaEspecialidadXXXXXXXXXXXX", "Especialidad con nombre demasiado largo")]
    public async Task AddSpeciality_CuandoSeCreaUnaEspecialidadConNombreDemasiadoCortoOLargo_EntoncesSeLanzaUnaValidationExcepcion(string name, string description)
    {
        var speciality = new SpecialityService(_mockPersistence);
        var request = new SpecialityModel.Request(name, description);

        var exception = await Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await speciality.Add(request);
        });

        Assert.Equal(nameof(ErrorCodes.FIELD_LENGTH_INVALID), exception.Error.ErrorCode);
        Assert.Equal(string.Format(ErrorCodes.FIELD_LENGTH_INVALID,"name", 3, 100), exception.Error.Message);
        await _mockPersistence.DidNotReceive().Add(Arg.Any<Speciality>());
    }
}