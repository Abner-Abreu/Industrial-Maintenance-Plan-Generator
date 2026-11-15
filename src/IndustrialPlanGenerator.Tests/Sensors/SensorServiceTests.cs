using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Enums;
using IndustrialPlanGenerator.Domain.Exceptions;
using IndustrialPlanGenerator.Domain.Persistence;
using IndustrialPlanGenerator.Domain.Repositories;
using IndustrialPlanGenerator.Services.Access;
using IndustrialPlanGenerator.Services.Auth;
using IndustrialPlanGenerator.Services.Contracts.Sensors;
using IndustrialPlanGenerator.Services.Exceptions;
using IndustrialPlanGenerator.Services.Sensors;
using Moq;

namespace IndustrialPlanGenerator.Tests.Sensors;

public class SensorServiceTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IEquipmentSystemRepository> _equipments = new();
    private readonly Mock<ICurrentUser> _current = new();
    private readonly Mock<ILineAccessPolicy> _access = new();
    private readonly SensorService _service;

    public SensorServiceTests()
    {
        _uow.Setup(u => u.Equipments).Returns(_equipments.Object);
        Como(UserRole.Administrator);
        _service = new SensorService(_uow.Object, _current.Object, _access.Object);
    }

    private void Como(UserRole rol)
    {
        _current.SetupGet(c => c.Id).Returns(Guid.CreateVersion7());
        _current.SetupGet(c => c.Role).Returns(rol);
    }

    private EquipmentSystem HaySistema(bool conAcceso = true)
    {
        var linea = new ProductionLine
        {
            Id = Guid.CreateVersion7(), Name = "L", ProductiveProcess = "P",
            EquipmentSystems = [], TechnicianLines = [], Operators = [], MaintenancePlans = []
        };
        var sistema = new EquipmentSystem
        {
            Id = Guid.CreateVersion7(), Name = "Compresor", Description = "d", OperativeState = "Operational",
            LineId = linea.Id, ProductionLine = linea, Sensors = [], MaintenanceTasks = []
        };
        _equipments.Setup(e => e.GetByIdAsync(sistema.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sistema);
        _access.Setup(a => a.CanViewAsync(linea.Id, It.IsAny<CancellationToken>())).ReturnsAsync(conAcceso);
        return sistema;
    }

    private static Sensor CrearSensor(EquipmentSystem sistema, SensorType tipo) => new()
    {
        Id = Guid.CreateVersion7(), EquipmentId = sistema.Id, Type = tipo, Equipment = sistema
    };

    // ======================= SEN-1 =======================

    [Fact]
    public async Task List_ConAcceso_DevuelveSensoresOrdenadosPorTipo()
    {
        var sistema = HaySistema();
        sistema.Sensors.Add(CrearSensor(sistema, SensorType.Pressure));
        sistema.Sensors.Add(CrearSensor(sistema, SensorType.Temperature));
        sistema.Sensors.Add(CrearSensor(sistema, SensorType.Vibration));

        var result = await _service.ListBySystemAsync(sistema.Id);

        Assert.Equal(new[] { "Temperature", "Vibration", "Pressure" }, result.Select(s => s.Type));
        Assert.All(result, s => Assert.Equal(sistema.Id, s.SystemId));
    }

    [Fact]
    public async Task List_SistemaSinSensores_DevuelveVacio()
    {
        var sistema = HaySistema();

        Assert.Empty(await _service.ListBySystemAsync(sistema.Id));
    }

    [Fact]
    public async Task List_TecnicoSinAccesoALaLinea_Lanza403SinConsultarSensores()
    {
        Como(UserRole.Technician);
        var sistema = HaySistema(conAcceso: false);

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.ListBySystemAsync(sistema.Id));
    }

    [Fact]
    public async Task List_Operario_Lanza403AunqueTengaAccesoALaLinea()
    {
        Como(UserRole.Operator);
        var sistema = HaySistema(conAcceso: true);

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.ListBySystemAsync(sistema.Id));
    }

    [Fact]
    public async Task List_SistemaInexistente_Lanza404()
    {
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.ListBySystemAsync(Guid.CreateVersion7()));
    }

    // ======================= SEN-2 =======================

    [Theory]
    [InlineData("Temperature", SensorType.Temperature)]
    [InlineData("vibration", SensorType.Vibration)]
    [InlineData("  PRESSURE ", SensorType.Pressure)]
    public async Task Create_TipoValido_GuardaElSensor(string texto, SensorType esperado)
    {
        var sistema = HaySistema();

        var result = await _service.CreateAsync(sistema.Id, new CreateSensorRequest(texto));

        var guardado = Assert.Single(sistema.Sensors);
        Assert.Equal(esperado, guardado.Type);
        Assert.Equal(sistema.Id, guardado.EquipmentId);
        Assert.Equal(guardado.Id, result.Id);
        Assert.Equal(sistema.Id, result.SystemId);
        Assert.Equal(esperado.ToString(), result.Type);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Humedad")]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("99")]
    public async Task Create_TipoVacioODesconocido_Lanza400(string? tipo)
    {
        var sistema = HaySistema();

        await Assert.ThrowsAsync<InvalidRequestException>(
            () => _service.CreateAsync(sistema.Id, new CreateSensorRequest(tipo!)));

        Assert.Empty(sistema.Sensors);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_SistemaInexistente_Lanza404()
    {
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _service.CreateAsync(Guid.CreateVersion7(), new CreateSensorRequest("Temperature")));
    }
}
