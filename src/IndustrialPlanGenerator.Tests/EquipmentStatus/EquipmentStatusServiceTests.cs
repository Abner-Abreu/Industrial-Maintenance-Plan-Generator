using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Enums;
using IndustrialPlanGenerator.Domain.Exceptions;
using IndustrialPlanGenerator.Domain.Persistence;
using IndustrialPlanGenerator.Domain.Repositories;
using IndustrialPlanGenerator.Services.Access;
using IndustrialPlanGenerator.Services.Auth;
using IndustrialPlanGenerator.Services.Contracts.EquipmentStatus;
using IndustrialPlanGenerator.Services.EquipmentStatus;
using IndustrialPlanGenerator.Services.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using CachedStatus = IndustrialPlanGenerator.Domain.Telemetry.EquipmentStatus;

namespace IndustrialPlanGenerator.Tests.EquipmentStatus;

public class EquipmentStatusServiceTests
{
    private static readonly DateTime Now = new(2026, 11, 8, 10, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IEquipmentSystemRepository> _equipments = new();
    private readonly Mock<IProductionLineRepository> _lines = new();
    private readonly Mock<IEquipmentStatusCache> _cache = new();
    private readonly Mock<ICurrentUser> _current = new();
    private readonly Mock<ILineAccessPolicy> _access = new();
    private readonly EquipmentStatusService _service;

    public EquipmentStatusServiceTests()
    {
        _uow.Setup(u => u.Equipments).Returns(_equipments.Object);
        _uow.Setup(u => u.Line).Returns(_lines.Object);
        Como(UserRole.Administrator);
        _service = new EquipmentStatusService(
            _uow.Object, _cache.Object, _current.Object, _access.Object,
            new FixedTimeProvider(Now), NullLogger<EquipmentStatusService>.Instance);
    }

    private sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow, TimeSpan.Zero);
    }

    private void Como(UserRole rol)
    {
        _current.SetupGet(c => c.Id).Returns(Guid.CreateVersion7());
        _current.SetupGet(c => c.Role).Returns(rol);
    }

    private static ProductionLine CrearLinea() => new()
    {
        Id = Guid.CreateVersion7(), Name = "L", ProductiveProcess = "P",
        EquipmentSystems = [], TechnicianLines = [], Operators = [], MaintenancePlans = []
    };

    private static EquipmentSystem CrearSistema(ProductionLine linea, string nombre, string estado = "Operational") => new()
    {
        Id = Guid.CreateVersion7(), Name = nombre, Description = "d", OperativeState = estado,
        LineId = linea.Id, ProductionLine = linea, Sensors = [], MaintenanceTasks = []
    };

    private EquipmentSystem HaySistema(string estado = "Operational", bool conAcceso = true)
    {
        var linea = CrearLinea();
        var sistema = CrearSistema(linea, "Compresor", estado);
        _equipments.Setup(e => e.GetByIdAsync(sistema.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sistema);
        _access.Setup(a => a.CanViewAsync(linea.Id, It.IsAny<CancellationToken>())).ReturnsAsync(conAcceso);
        return sistema;
    }

    private void CacheTiene(Guid id, string estado, DateTime fecha) =>
        _cache.Setup(c => c.GetAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CachedStatus { EquipmentId = id, OperatingStatus = estado, LastUpdated = fecha });

    // ======================= EST-1 =======================

    [Fact]
    public async Task Get_ConCache_DevuelveElValorDeLaCacheSinEscribirla()
    {
        var sistema = HaySistema(estado: "Operational");
        var fecha = Now.AddHours(-3);
        CacheTiene(sistema.Id, EquipmentStates.Faulty, fecha);

        var result = await _service.GetAsync(sistema.Id);

        Assert.Equal(EquipmentStates.Faulty, result.Status);
        Assert.Equal(fecha, result.LastUpdated);
        Assert.Equal("Compresor", result.SystemName);
        _cache.Verify(c => c.UpsertAsync(It.IsAny<CachedStatus>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Get_SinCache_DevuelvePostgresYRellenaLaCache()
    {
        var sistema = HaySistema(estado: EquipmentStates.Stopped);

        var result = await _service.GetAsync(sistema.Id);

        Assert.Equal(EquipmentStates.Stopped, result.Status);
        Assert.Equal(Now, result.LastUpdated);
        _cache.Verify(c => c.UpsertAsync(
            It.Is<CachedStatus>(s => s.EquipmentId == sistema.Id && s.OperatingStatus == EquipmentStates.Stopped && s.LastUpdated == Now),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Get_CacheConValorNoValido_SeTrataComoSiNoEstuviera()
    {
        var sistema = HaySistema(estado: EquipmentStates.Operational);
        CacheTiene(sistema.Id, "valor-raro", Now.AddDays(-1));

        var result = await _service.GetAsync(sistema.Id);

        Assert.Equal(EquipmentStates.Operational, result.Status);
        Assert.Equal(Now, result.LastUpdated);
    }

    [Fact]
    public async Task Get_MongoCaido_RespondeConPostgresSinLanzar()
    {
        var sistema = HaySistema(estado: EquipmentStates.UnderMaintenance);
        _cache.Setup(c => c.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("mongo"));

        var result = await _service.GetAsync(sistema.Id);

        Assert.Equal(EquipmentStates.UnderMaintenance, result.Status);
        // Tras el primer fallo no se vuelve a intentar escribir en la caché.
        _cache.Verify(c => c.UpsertAsync(It.IsAny<CachedStatus>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Get_SinAcceso_Lanza403()
    {
        var sistema = HaySistema(conAcceso: false);

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.GetAsync(sistema.Id));

        _cache.Verify(c => c.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Get_SistemaInexistente_Lanza404()
    {
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.GetAsync(Guid.CreateVersion7()));
    }

    // ======================= EST-2 =======================

    private (ProductionLine Linea, List<EquipmentSystem> Sistemas) HayLineaConSistemas(bool conAcceso = true)
    {
        var linea = CrearLinea();
        var sistemas = new List<EquipmentSystem>
        {
            CrearSistema(linea, "Prensa"), CrearSistema(linea, "bomba"), CrearSistema(linea, "Horno")
        };
        _lines.Setup(l => l.GetByIdAsync(linea.Id, It.IsAny<CancellationToken>())).ReturnsAsync(linea);
        _equipments.Setup(e => e.GetByLineaAsync(linea.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sistemas);
        _access.Setup(a => a.CanViewAsync(linea.Id, It.IsAny<CancellationToken>())).ReturnsAsync(conAcceso);
        return (linea, sistemas);
    }

    [Fact]
    public async Task ListByLine_DevuelveUnEstadoPorEquipoOrdenadosPorNombre()
    {
        var (linea, _) = HayLineaConSistemas();

        var result = await _service.ListByLineAsync(linea.Id);

        Assert.Equal(new[] { "bomba", "Horno", "Prensa" }, result.Select(s => s.SystemName));
    }

    [Fact]
    public async Task ListByLine_MongoCaido_SoloIntentaLaCacheUnaVez()
    {
        var (linea, _) = HayLineaConSistemas();
        _cache.Setup(c => c.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("mongo"));

        var result = await _service.ListByLineAsync(linea.Id);

        Assert.Equal(3, result.Count);
        _cache.Verify(c => c.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListByLine_SinAcceso_Lanza403()
    {
        var (linea, _) = HayLineaConSistemas(conAcceso: false);

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.ListByLineAsync(linea.Id));
    }

    [Fact]
    public async Task ListByLine_LineaInexistente_Lanza404()
    {
        var id = Guid.CreateVersion7();
        _access.Setup(a => a.CanViewAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.ListByLineAsync(id));
    }

    // ======================= EST-3 =======================

    [Fact]
    public async Task Update_Administrador_GuardaEnPostgresYActualizaLaCache()
    {
        var sistema = HaySistema(estado: EquipmentStates.Operational);

        var result = await _service.UpdateAsync(sistema.Id, new UpdateEquipmentStatusRequest(EquipmentStates.Faulty));

        Assert.Equal(EquipmentStates.Faulty, sistema.OperativeState);
        Assert.Equal(EquipmentStates.Faulty, result.Status);
        Assert.Equal(Now, result.LastUpdated);
        _equipments.Verify(e => e.Update(sistema), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cache.Verify(c => c.UpsertAsync(
            It.Is<CachedStatus>(s => s.EquipmentId == sistema.Id && s.OperatingStatus == EquipmentStates.Faulty && s.LastUpdated == Now),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_TecnicoConAcceso_Puede()
    {
        Como(UserRole.Technician);
        var sistema = HaySistema();

        var result = await _service.UpdateAsync(sistema.Id, new UpdateEquipmentStatusRequest(EquipmentStates.Stopped));

        Assert.Equal(EquipmentStates.Stopped, result.Status);
    }

    [Fact]
    public async Task Update_TecnicoSinAccesoALaLinea_Lanza403YNoCambiaNada()
    {
        Como(UserRole.Technician);
        var sistema = HaySistema(estado: EquipmentStates.Operational, conAcceso: false);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => _service.UpdateAsync(sistema.Id, new UpdateEquipmentStatusRequest(EquipmentStates.Faulty)));

        Assert.Equal(EquipmentStates.Operational, sistema.OperativeState);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(UserRole.PlantManager)]
    [InlineData(UserRole.Operator)]
    public async Task Update_JefeYOperario_Lanzan403(UserRole rol)
    {
        Como(rol);
        var sistema = HaySistema();

        await Assert.ThrowsAsync<ForbiddenException>(
            () => _service.UpdateAsync(sistema.Id, new UpdateEquipmentStatusRequest(EquipmentStates.Faulty)));

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_EstadoEnOtraMayuscula_SeGuardaConNombreCanonico()
    {
        var sistema = HaySistema();

        var result = await _service.UpdateAsync(sistema.Id, new UpdateEquipmentStatusRequest("  underMAINTENANCE "));

        Assert.Equal(EquipmentStates.UnderMaintenance, sistema.OperativeState);
        Assert.Equal(EquipmentStates.UnderMaintenance, result.Status);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Roto")]
    [InlineData("0")]
    public async Task Update_EstadoVacioODesconocido_Lanza400(string? estado)
    {
        var sistema = HaySistema();

        await Assert.ThrowsAsync<InvalidRequestException>(
            () => _service.UpdateAsync(sistema.Id, new UpdateEquipmentStatusRequest(estado!)));

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_SistemaInexistente_Lanza404()
    {
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _service.UpdateAsync(Guid.CreateVersion7(), new UpdateEquipmentStatusRequest(EquipmentStates.Faulty)));
    }

    [Fact]
    public async Task Update_FallaLaCache_ElCambioSeGuardaYSeIntentaInvalidar()
    {
        var sistema = HaySistema();
        _cache.Setup(c => c.UpsertAsync(It.IsAny<CachedStatus>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("mongo"));

        var result = await _service.UpdateAsync(sistema.Id, new UpdateEquipmentStatusRequest(EquipmentStates.Faulty));

        Assert.Equal(EquipmentStates.Faulty, result.Status);
        Assert.Equal(EquipmentStates.Faulty, sistema.OperativeState);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cache.Verify(c => c.InvalidateAsync(sistema.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_FallanActualizarEInvalidarLaCache_NoLanza()
    {
        var sistema = HaySistema();
        _cache.Setup(c => c.UpsertAsync(It.IsAny<CachedStatus>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("mongo"));
        _cache.Setup(c => c.InvalidateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("mongo"));

        var result = await _service.UpdateAsync(sistema.Id, new UpdateEquipmentStatusRequest(EquipmentStates.Stopped));

        Assert.Equal(EquipmentStates.Stopped, result.Status);
    }
}
