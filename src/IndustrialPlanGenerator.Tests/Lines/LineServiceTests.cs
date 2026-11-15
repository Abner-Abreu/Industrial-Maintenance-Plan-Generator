using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Enums;
using IndustrialPlanGenerator.Domain.Exceptions;
using IndustrialPlanGenerator.Domain.Persistence;
using IndustrialPlanGenerator.Domain.Repositories;
using IndustrialPlanGenerator.Services.Access;
using IndustrialPlanGenerator.Services.Auth;
using IndustrialPlanGenerator.Services.Contracts.Lines;
using IndustrialPlanGenerator.Services.Exceptions;
using IndustrialPlanGenerator.Services.Lines;
using Moq;

namespace IndustrialPlanGenerator.Tests.Lines;

public class LineServiceTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IProductionLineRepository> _lines = new();
    private readonly Mock<ITechnicianRepository> _technicians = new();
    private readonly Mock<IOperatorRepository> _operators = new();
    private readonly Mock<IEquipmentSystemRepository> _equipments = new();
    private readonly Mock<IMaintenancePlanRepository> _plans = new();
    private readonly Mock<ICurrentUser> _current = new();
    private readonly Mock<ILineAccessPolicy> _access = new();
    private readonly LineService _service;

    private readonly Guid _userId = Guid.CreateVersion7();

    public LineServiceTests()
    {
        _uow.Setup(u => u.Line).Returns(_lines.Object);
        _uow.Setup(u => u.Technicians).Returns(_technicians.Object);
        _uow.Setup(u => u.Operators).Returns(_operators.Object);
        _uow.Setup(u => u.Equipments).Returns(_equipments.Object);
        _uow.Setup(u => u.Plans).Returns(_plans.Object);
        // Por defecto la línea no tiene planes ni usuarios asignados.
        _plans.Setup(p => p.GetByLineAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MaintenancePlan>());
        _technicians.Setup(t => t.GetByLineAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Technician>());
        _operators.Setup(o => o.GetByLineAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Operator>());
        _current.SetupGet(c => c.Id).Returns(_userId);
        Como(UserRole.Administrator);
        _service = new LineService(_uow.Object, _current.Object, _access.Object);
    }

    private void Como(UserRole rol) => _current.SetupGet(c => c.Role).Returns(rol);

    private static ProductionLine CrearLinea(string nombre, string proceso = "Ensamblaje", int sistemas = 0)
    {
        var linea = new ProductionLine
        {
            Id = Guid.CreateVersion7(), Name = nombre, ProductiveProcess = proceso,
            EquipmentSystems = [], TechnicianLines = [], Operators = [], MaintenancePlans = []
        };
        for (var i = 0; i < sistemas; i++)
        {
            linea.EquipmentSystems.Add(new EquipmentSystem
            {
                Id = Guid.CreateVersion7(), Name = $"Sistema {i}", Description = "d", OperativeState = "Operational",
                LineId = linea.Id, ProductionLine = linea, Sensors = [], MaintenanceTasks = []
            });
        }
        return linea;
    }

    private void HayLineas(params ProductionLine[] lineas)
    {
        _lines.Setup(l => l.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(lineas.ToList());
        foreach (var l in lineas)
        {
            _lines.Setup(r => r.GetWithEquipmentAsync(l.Id, It.IsAny<CancellationToken>())).ReturnsAsync(l);
            _equipments.Setup(e => e.GetByLineaAsync(l.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(l.EquipmentSystems.ToList());
        }
    }

    private void TecnicoConLineas(params Guid[] lineIds)
    {
        var tecnico = new Technician
        {
            Id = _userId, Specialty = "x",
            Lines = [], MaintenancePlans = [], MaintenanceTasks = [], ReinspectionRequests = []
        };
        foreach (var id in lineIds)
            tecnico.Lines.Add(new TechnicianLine
            {
                TechnicianId = _userId, LineId = id, Permission = TechnicianPermission.Generate,
                Technician = tecnico, Line = null!
            });
        _technicians.Setup(t => t.GetWithLineAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(tecnico);
    }

    private void OperarioConLineas(params Guid[] lineIds)
    {
        var turno = new Shift { Id = Guid.CreateVersion7(), Name = "M", Schedule = "s", Operators = [] };
        var op = new Operator
        {
            Id = _userId, Age = 30, ShiftId = turno.Id, Shift = turno,
            Lines = [], Plans = [], ReinspectionRequests = []
        };
        foreach (var id in lineIds)
            op.Lines.Add(new OperatorLine { OperatorId = _userId, LineId = id, Operator = op, Line = null! });
        _operators.Setup(o => o.GetWithShiftAndLinesAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(op);
    }

    // ======================= LIN-1 =======================

    [Theory]
    [InlineData(UserRole.Administrator)]
    [InlineData(UserRole.PlantManager)]
    public async Task List_AdministradorYJefe_VenTodasOrdenadasConCantidadDeSistemas(UserRole rol)
    {
        Como(rol);
        HayLineas(CrearLinea("Línea B", sistemas: 2), CrearLinea("línea A", sistemas: 0));

        var result = await _service.ListAsync();

        Assert.Equal(new[] { "línea A", "Línea B" }, result.Select(l => l.Name));
        Assert.Equal(new[] { 0, 2 }, result.Select(l => l.SystemCount));
    }

    [Fact]
    public async Task List_Tecnico_VeSoloSusLineas()
    {
        Como(UserRole.Technician);
        var propia = CrearLinea("Propia");
        var ajena = CrearLinea("Ajena");
        HayLineas(propia, ajena);
        TecnicoConLineas(propia.Id);

        var result = await _service.ListAsync();

        var unica = Assert.Single(result);
        Assert.Equal(propia.Id, unica.Id);
    }

    [Fact]
    public async Task List_Operario_VeSoloSusLineas()
    {
        Como(UserRole.Operator);
        var propia = CrearLinea("Propia");
        var ajena = CrearLinea("Ajena");
        HayLineas(propia, ajena);
        OperarioConLineas(propia.Id);

        var result = await _service.ListAsync();

        Assert.Equal(propia.Id, Assert.Single(result).Id);
    }

    [Fact]
    public async Task List_TecnicoSinRegistro_DevuelveVacio()
    {
        Como(UserRole.Technician);
        HayLineas(CrearLinea("Ajena"));

        Assert.Empty(await _service.ListAsync());
    }

    // ======================= LIN-2 =======================

    [Fact]
    public async Task Get_SinAcceso_Lanza403SinBuscarLaLinea()
    {
        var id = Guid.CreateVersion7();
        _access.Setup(a => a.CanViewAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.GetAsync(id));

        _lines.Verify(l => l.GetWithEquipmentAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Get_ConAcceso_DevuelveDetalleConSistemasOrdenados()
    {
        var linea = CrearLinea("Línea 1", "Soldadura", sistemas: 3);
        HayLineas(linea);
        _access.Setup(a => a.CanViewAsync(linea.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _service.GetAsync(linea.Id);

        Assert.Equal("Línea 1", result.Name);
        Assert.Equal("Soldadura", result.ProductiveProcess);
        Assert.Equal(new[] { "Sistema 0", "Sistema 1", "Sistema 2" }, result.Systems.Select(s => s.Name));
    }

    [Fact]
    public async Task Get_Inexistente_Lanza404()
    {
        var id = Guid.CreateVersion7();
        _access.Setup(a => a.CanViewAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.GetAsync(id));
    }

    // ======================= LIN-3 =======================

    [Fact]
    public async Task Create_Valido_GuardaConEspaciosRecortados()
    {
        ProductionLine? guardada = null;
        _lines.Setup(l => l.AddAsync(It.IsAny<ProductionLine>(), It.IsAny<CancellationToken>()))
            .Callback<ProductionLine, CancellationToken>((l, _) => guardada = l)
            .Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(new CreateLineRequest("  Línea 1 ", " Ensamblaje "));

        Assert.NotNull(guardada);
        Assert.Equal("Línea 1", guardada!.Name);
        Assert.Equal("Ensamblaje", guardada.ProductiveProcess);
        Assert.Equal(guardada.Id, result.Id);
        Assert.Empty(result.Systems);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(null, "Ensamblaje")]
    [InlineData("", "Ensamblaje")]
    [InlineData("   ", "Ensamblaje")]
    [InlineData("Línea 1", null)]
    [InlineData("Línea 1", "")]
    [InlineData("Línea 1", "   ")]
    public async Task Create_DatosVacios_Lanza400(string? nombre, string? proceso)
    {
        await Assert.ThrowsAsync<InvalidRequestException>(
            () => _service.CreateAsync(new CreateLineRequest(nombre!, proceso!)));

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_NombreODemasiadoLargo_Lanza400()
    {
        await Assert.ThrowsAsync<InvalidRequestException>(() => _service.CreateAsync(
            new CreateLineRequest(new string('a', LineService.MaxNameLength + 1), "Ensamblaje")));
        await Assert.ThrowsAsync<InvalidRequestException>(() => _service.CreateAsync(
            new CreateLineRequest("Línea 1", new string('a', LineService.MaxProcessLength + 1))));
    }

    // ======================= LIN-4 =======================

    [Fact]
    public async Task Update_Valido_CambiaNombreYProceso()
    {
        var linea = CrearLinea("Vieja", "Antiguo", sistemas: 1);
        HayLineas(linea);

        var result = await _service.UpdateAsync(linea.Id, new UpdateLineRequest("Nueva", "Moderno"));

        Assert.Equal("Nueva", result.Name);
        Assert.Equal("Moderno", result.ProductiveProcess);
        Assert.Single(result.Systems);
        _lines.Verify(l => l.Update(linea), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_Inexistente_Lanza404()
    {
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _service.UpdateAsync(Guid.CreateVersion7(), new UpdateLineRequest("Nueva", "Moderno")));
    }

    [Fact]
    public async Task Update_DatosVacios_Lanza400YNoCambiaNada()
    {
        var linea = CrearLinea("Vieja", "Antiguo");
        HayLineas(linea);

        await Assert.ThrowsAsync<InvalidRequestException>(
            () => _service.UpdateAsync(linea.Id, new UpdateLineRequest(" ", "Moderno")));

        Assert.Equal("Vieja", linea.Name);
    }

    // ======================= LIN-5 =======================

    [Fact]
    public async Task Delete_SinDependencias_LaElimina()
    {
        var linea = CrearLinea("Vacía");
        HayLineas(linea);

        await _service.DeleteAsync(linea.Id);

        _lines.Verify(l => l.Remove(linea), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_ConSistemas_Lanza409()
    {
        var linea = CrearLinea("Con sistemas", sistemas: 1);
        HayLineas(linea);

        await Assert.ThrowsAsync<ConflictException>(() => _service.DeleteAsync(linea.Id));

        _lines.Verify(l => l.Remove(It.IsAny<ProductionLine>()), Times.Never);
    }

    [Fact]
    public async Task Delete_ConPlanes_Lanza409()
    {
        var linea = CrearLinea("Con planes");
        HayLineas(linea);
        // Solo se usa el número de elementos; basta con un marcador.
        _plans.Setup(p => p.GetByLineAsync(linea.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MaintenancePlan> { null! });

        await Assert.ThrowsAsync<ConflictException>(() => _service.DeleteAsync(linea.Id));

        _lines.Verify(l => l.Remove(It.IsAny<ProductionLine>()), Times.Never);
    }

    [Fact]
    public async Task Delete_ConTecnicosAsignados_Lanza409()
    {
        var linea = CrearLinea("Con técnicos");
        HayLineas(linea);
        _technicians.Setup(t => t.GetByLineAsync(linea.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Technician> { null! });

        await Assert.ThrowsAsync<ConflictException>(() => _service.DeleteAsync(linea.Id));

        _lines.Verify(l => l.Remove(It.IsAny<ProductionLine>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Delete_Inexistente_Lanza404()
    {
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.DeleteAsync(Guid.CreateVersion7()));
    }

    [Fact]
    public async Task Delete_ConOperariosAsignados_Lanza409()
    {
        var linea = CrearLinea("Con operarios");
        HayLineas(linea);
        _operators.Setup(o => o.GetByLineAsync(linea.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Operator> { null! });

        await Assert.ThrowsAsync<ConflictException>(() => _service.DeleteAsync(linea.Id));

        _lines.Verify(l => l.Remove(It.IsAny<ProductionLine>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
