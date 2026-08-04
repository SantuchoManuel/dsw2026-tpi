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

    [Fact]
    public async Task AddSpeciality_CuandoSeCreaUnaEspecialidadConNombreYDescripcion_EntoncesSeGuardaEnLaBaseDeDatos()
    {
        var speciality = new SpecialityService(_mockPersistence);
        var request = new SpecialityModel.Request("Cardiología", "Especialidad del corazón");

        var result = await speciality.Add(request);

        Assert.NotNull(result);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.Description, result.Description);

        await _mockPersistence.Received(1).Add(Arg.Is<Speciality>(s =>
            s.Name == request.Name &&
            s.Description == request.Description));
    }

    [Fact]
    public async Task AddSpeciality_CuandoSeCreaUnaEspecialidadSinDescripcion_EntoncesSeLanzaUnaValidationExcepcion()
    {
        var speciality = new SpecialityService(_mockPersistence);
        var request = new SpecialityModel.Request("Especialidad sin Descripción", "");

        var exception = await Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await speciality.Add(request);
        });

        Assert.Equal(nameof(ErrorCodes.FIELD_REQUIRED), exception.Error.ErrorCode);
        Assert.Equal(string.Format(ErrorCodes.FIELD_REQUIRED, "description"), exception.Error.Message);
        await _mockPersistence.DidNotReceive().Add(Arg.Any<Speciality>());
    }

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
    [Fact]
    public async Task AddSpeciality_CuandoSeCreaUnaEspecialidadSinNombre_EntoncesSeLanzaUnaValidationExcepcion()
    {
        var speciality = new SpecialityService(_mockPersistence);
        var request = new SpecialityModel.Request("", "Especialidad sin nombre");

        var exception = await Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await speciality.Add(request);
        });

        Assert.Equal(nameof(ErrorCodes.FIELD_REQUIRED), exception.Error.ErrorCode);
        Assert.Equal(string.Format(ErrorCodes.FIELD_REQUIRED, "name"), exception.Error.Message);

        await _mockPersistence.DidNotReceive().Add(Arg.Any<Speciality>());
    }
}