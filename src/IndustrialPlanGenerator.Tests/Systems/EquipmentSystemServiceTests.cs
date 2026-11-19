using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Enums;
using IndustrialPlanGenerator.Domain.Exceptions;
using IndustrialPlanGenerator.Domain.Persistence;
using IndustrialPlanGenerator.Domain.Repositories;
using IndustrialPlanGenerator.Services.Access;
using IndustrialPlanGenerator.Services.Contracts.EquipmentStatus;
using IndustrialPlanGenerator.Services.Contracts.Systems;
using IndustrialPlanGenerator.Services.Exceptions;
using IndustrialPlanGenerator.Services.Systems;
using Moq;

namespace IndustrialPlanGenerator.Tests.Systems;

public class EquipmentSystemServiceTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IProductionLineRepository> _lines = new();
    private readonly Mock<IEquipmentSystemRepository> _equipments = new();
    private readonly Mock<ILineAccessPolicy> _access = new();
    private readonly EquipmentSystemService _service;

    public EquipmentSystemServiceTests()
    {
        _uow.Setup(u => u.Line).Returns(_lines.Object);
        _uow.Setup(u => u.Equipments).Returns(_equipments.Object);
        _service = new EquipmentSystemService(_uow.Object, _access.Object);
    }

    private static ProductionLine CrearLinea() => new()
    {
        Id = Guid.CreateVersion7(), Name = "Línea 1", ProductiveProcess = "Ensamblaje",
        EquipmentSystems = [], TechnicianLines = [], Operators = [], MaintenancePlans = []
    };

    private static EquipmentSystem CrearSistema(ProductionLine linea, string nombre, string estado = "Faulty") => new()
    {
        Id = Guid.CreateVersion7(), Name = nombre, Description = "Descripción", OperativeState = estado,
        LineId = linea.Id, ProductionLine = linea, Sensors = [], MaintenanceTasks = []
    };

    private ProductionLine HayLinea(bool conAcceso = true)
    {
        var linea = CrearLinea();
        _lines.Setup(l => l.GetByIdAsync(linea.Id, It.IsAny<CancellationToken>())).ReturnsAsync(linea);
        _access.Setup(a => a.CanViewAsync(linea.Id, It.IsAny<CancellationToken>())).ReturnsAsync(conAcceso);
        return linea;
    }

    private EquipmentSystem HaySistema(string nombre = "Compresor")
    {
        var sistema = CrearSistema(CrearLinea(), nombre);
        _equipments.Setup(e => e.GetByIdAsync(sistema.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sistema);
        return sistema;
    }

    // ======================= SIS-1 =======================

    [Fact]
    public async Task ListByLine_ConAcceso_DevuelveSistemasOrdenadosPorNombre()
    {
        var linea = HayLinea();
        _equipments.Setup(e => e.GetByLineaAsync(linea.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EquipmentSystem> { CrearSistema(linea, "Prensa"), CrearSistema(linea, "bomba") });

        var result = await _service.ListByLineAsync(linea.Id);

        Assert.Equal(new[] { "bomba", "Prensa" }, result.Select(s => s.Name));
    }

    [Fact]
    public async Task ListByLine_SinAcceso_Lanza403SinConsultarSistemas()
    {
        var linea = HayLinea(conAcceso: false);

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.ListByLineAsync(linea.Id));

        _equipments.Verify(e => e.GetByLineaAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ListByLine_LineaInexistente_Lanza404()
    {
        var id = Guid.CreateVersion7();
        _access.Setup(a => a.CanViewAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.ListByLineAsync(id));
    }

    // ======================= SIS-2 =======================

    [Fact]
    public async Task Create_Valido_GuardaConEstadoInicialOperativo()
    {
        var linea = HayLinea();
        EquipmentSystem? guardado = null;
        _equipments.Setup(e => e.AddAsync(It.IsAny<EquipmentSystem>(), It.IsAny<CancellationToken>()))
            .Callback<EquipmentSystem, CancellationToken>((s, _) => guardado = s)
            .Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(new CreateSystemRequest(linea.Id, "  Compresor ", " Aire comprimido "));

        Assert.NotNull(guardado);
        Assert.Equal("Compresor", guardado!.Name);
        Assert.Equal("Aire comprimido", guardado.Description);
        Assert.Equal(linea.Id, guardado.LineId);
        Assert.Equal(EquipmentStates.Operational, guardado.OperativeState);
        Assert.Equal(guardado.Id, result.Id);
        Assert.Equal(EquipmentStates.Operational, result.OperativeState);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(null, "Descripción")]
    [InlineData("", "Descripción")]
    [InlineData("   ", "Descripción")]
    [InlineData("Compresor", null)]
    [InlineData("Compresor", "")]
    [InlineData("Compresor", "   ")]
    public async Task Create_DatosVacios_Lanza400(string? nombre, string? descripcion)
    {
        var linea = HayLinea();

        await Assert.ThrowsAsync<InvalidRequestException>(
            () => _service.CreateAsync(new CreateSystemRequest(linea.Id, nombre!, descripcion!)));

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_NombreODescripcionDemasiadoLargos_Lanza400()
    {
        var linea = HayLinea();

        await Assert.ThrowsAsync<InvalidRequestException>(() => _service.CreateAsync(
            new CreateSystemRequest(linea.Id, new string('a', EquipmentSystemService.MaxNameLength + 1), "d")));
        await Assert.ThrowsAsync<InvalidRequestException>(() => _service.CreateAsync(
            new CreateSystemRequest(linea.Id, "Compresor", new string('a', EquipmentSystemService.MaxDescriptionLength + 1))));
    }

    [Fact]
    public async Task Create_LineaInexistente_Lanza404()
    {
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _service.CreateAsync(new CreateSystemRequest(Guid.CreateVersion7(), "Compresor", "Descripción")));
    }

    // ======================= SIS-3 =======================

    [Fact]
    public async Task Update_Valido_CambiaNombreYDescripcionSinTocarElEstado()
    {
        var sistema = HaySistema();
        var lineaOriginal = sistema.LineId;

        var result = await _service.UpdateAsync(sistema.Id, new UpdateSystemRequest("Nuevo", "Otra descripción"));

        Assert.Equal("Nuevo", result.Name);
        Assert.Equal("Otra descripción", result.Description);
        Assert.Equal("Faulty", sistema.OperativeState);
        Assert.Equal(lineaOriginal, sistema.LineId);
        _equipments.Verify(e => e.Update(sistema), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_Inexistente_Lanza404()
    {
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _service.UpdateAsync(Guid.CreateVersion7(), new UpdateSystemRequest("Nuevo", "Descripción")));
    }

    [Fact]
    public async Task Update_DatosVacios_Lanza400YNoCambiaNada()
    {
        var sistema = HaySistema("Original");

        await Assert.ThrowsAsync<InvalidRequestException>(
            () => _service.UpdateAsync(sistema.Id, new UpdateSystemRequest(" ", "Descripción")));

        Assert.Equal("Original", sistema.Name);
    }

    // ======================= SIS-4 =======================

    [Fact]
    public async Task Delete_SinTareasNiSensores_LoElimina()
    {
        var sistema = HaySistema();

        await _service.DeleteAsync(sistema.Id);

        _equipments.Verify(e => e.Remove(sistema), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_ConTareas_Lanza409()
    {
        var sistema = HaySistema();
        // Solo se usa el número de elementos; basta con un marcador.
        sistema.MaintenanceTasks.Add(null!);

        await Assert.ThrowsAsync<ConflictException>(() => _service.DeleteAsync(sistema.Id));

        _equipments.Verify(e => e.Remove(It.IsAny<EquipmentSystem>()), Times.Never);
    }

    [Fact]
    public async Task Delete_ConSensores_Lanza409()
    {
        var sistema = HaySistema();
        sistema.Sensors.Add(new Sensor
        {
            Id = Guid.CreateVersion7(), EquipmentId = sistema.Id, Type = SensorType.Temperature, Equipment = sistema
        });

        await Assert.ThrowsAsync<ConflictException>(() => _service.DeleteAsync(sistema.Id));

        _equipments.Verify(e => e.Remove(It.IsAny<EquipmentSystem>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Delete_Inexistente_Lanza404()
    {
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.DeleteAsync(Guid.CreateVersion7()));
    }
}
