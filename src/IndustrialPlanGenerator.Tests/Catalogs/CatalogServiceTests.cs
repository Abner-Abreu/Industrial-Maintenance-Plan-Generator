using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Exceptions;
using IndustrialPlanGenerator.Domain.Persistence;
using IndustrialPlanGenerator.Domain.Repositories;
using IndustrialPlanGenerator.Services.Catalogs;
using IndustrialPlanGenerator.Services.Contracts.Catalogs;
using IndustrialPlanGenerator.Services.Exceptions;
using Moq;

namespace IndustrialPlanGenerator.Tests.Catalogs;

public class CatalogServiceTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IShiftRepository> _shifts = new();
    private readonly CatalogService _service;
    private readonly List<Shift> _existing = [];

    public CatalogServiceTests()
    {
        _uow.Setup(u => u.Shifts).Returns(_shifts.Object);
        _shifts.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => _existing.ToList());
        _shifts.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => _existing.FirstOrDefault(s => s.Id == id));
        _service = new CatalogService(_uow.Object);
    }

    private Shift AgregarTurno(string nombre, string horario)
    {
        var turno = new Shift { Id = Guid.CreateVersion7(), Name = nombre, Schedule = horario, Operators = [] };
        _existing.Add(turno);
        return turno;
    }

    // ======================= CAT-1 =======================

    [Fact]
    public async Task ListShifts_DevuelveLosTurnosOrdenadosPorNombre()
    {
        AgregarTurno("Noche", "22:00-06:00");
        AgregarTurno("mañana", "06:00-14:00");
        AgregarTurno("Tarde", "14:00-22:00");

        var result = await _service.ListShiftsAsync();

        Assert.Equal(new[] { "mañana", "Noche", "Tarde" }, result.Select(s => s.Name));
    }

    [Fact]
    public async Task ListShifts_SinTurnos_DevuelveListaVacia()
    {
        Assert.Empty(await _service.ListShiftsAsync());
    }

    // ======================= CAT-2 =======================

    [Fact]
    public async Task CreateShift_Valido_GuardaConEspaciosRecortados()
    {
        Shift? guardado = null;
        _shifts.Setup(s => s.AddAsync(It.IsAny<Shift>(), It.IsAny<CancellationToken>()))
            .Callback<Shift, CancellationToken>((s, _) => guardado = s)
            .Returns(Task.CompletedTask);

        var result = await _service.CreateShiftAsync(new CreateShiftRequest("  Mañana ", " 06:00-14:00  "));

        Assert.NotNull(guardado);
        Assert.Equal("Mañana", guardado!.Name);
        Assert.Equal("06:00-14:00", guardado.Schedule);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(guardado.Id, result.Id);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(null, "06:00-14:00")]
    [InlineData("", "06:00-14:00")]
    [InlineData("   ", "06:00-14:00")]
    [InlineData("Mañana", null)]
    [InlineData("Mañana", "")]
    [InlineData("Mañana", "   ")]
    public async Task CreateShift_NombreOHorarioVacios_Lanza400(string? nombre, string? horario)
    {
        await Assert.ThrowsAsync<InvalidRequestException>(
            () => _service.CreateShiftAsync(new CreateShiftRequest(nombre!, horario!)));

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateShift_NombreDemasiadoLargo_Lanza400()
    {
        var largo = new string('a', CatalogService.MaxShiftNameLength + 1);

        await Assert.ThrowsAsync<InvalidRequestException>(
            () => _service.CreateShiftAsync(new CreateShiftRequest(largo, "06:00-14:00")));
    }

    [Fact]
    public async Task CreateShift_HorarioDemasiadoLargo_Lanza400()
    {
        var largo = new string('9', CatalogService.MaxShiftScheduleLength + 1);

        await Assert.ThrowsAsync<InvalidRequestException>(
            () => _service.CreateShiftAsync(new CreateShiftRequest("Mañana", largo)));
    }

    [Fact]
    public async Task CreateShift_NombreRepetidoSinDistinguirMayusculas_Lanza409()
    {
        AgregarTurno("Mañana", "06:00-14:00");

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.CreateShiftAsync(new CreateShiftRequest("mañana", "07:00-15:00")));

        _shifts.Verify(s => s.AddAsync(It.IsAny<Shift>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateShift_HorarioRepetido_Lanza409()
    {
        AgregarTurno("Mañana", "06:00-14:00");

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.CreateShiftAsync(new CreateShiftRequest("Turno A", "06:00-14:00")));
    }

    // ======================= CAT-3 =======================

    [Fact]
    public async Task UpdateShift_Valido_CambiaNombreYHorario()
    {
        var turno = AgregarTurno("Mañana", "06:00-14:00");

        var result = await _service.UpdateShiftAsync(turno.Id, new UpdateShiftRequest("Diurno", "07:00-15:00"));

        Assert.Equal("Diurno", result.Name);
        Assert.Equal("07:00-15:00", result.Schedule);
        Assert.Equal("Diurno", turno.Name);
        _shifts.Verify(s => s.Update(turno), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateShift_ConservandoSuPropioNombre_NoEsConflicto()
    {
        var turno = AgregarTurno("Mañana", "06:00-14:00");

        var result = await _service.UpdateShiftAsync(turno.Id, new UpdateShiftRequest("Mañana", "05:00-13:00"));

        Assert.Equal("05:00-13:00", result.Schedule);
    }

    [Fact]
    public async Task UpdateShift_NombreDeOtroTurno_Lanza409()
    {
        AgregarTurno("Mañana", "06:00-14:00");
        var tarde = AgregarTurno("Tarde", "14:00-22:00");

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.UpdateShiftAsync(tarde.Id, new UpdateShiftRequest("Mañana", "14:00-22:00")));

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateShift_HorarioDeOtroTurno_Lanza409()
    {
        AgregarTurno("Mañana", "06:00-14:00");
        var tarde = AgregarTurno("Tarde", "14:00-22:00");

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.UpdateShiftAsync(tarde.Id, new UpdateShiftRequest("Tarde", "06:00-14:00")));
    }

    [Fact]
    public async Task UpdateShift_Inexistente_Lanza404()
    {
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _service.UpdateShiftAsync(Guid.CreateVersion7(), new UpdateShiftRequest("Diurno", "07:00-15:00")));
    }

    [Fact]
    public async Task UpdateShift_DatosVacios_Lanza400()
    {
        var turno = AgregarTurno("Mañana", "06:00-14:00");

        await Assert.ThrowsAsync<InvalidRequestException>(
            () => _service.UpdateShiftAsync(turno.Id, new UpdateShiftRequest(" ", "07:00-15:00")));

        Assert.Equal("Mañana", turno.Name);
    }

    // ======================= CAT-4 =======================

    [Fact]
    public async Task DeleteShift_SinOperarios_LoElimina()
    {
        var turno = AgregarTurno("Mañana", "06:00-14:00");

        await _service.DeleteShiftAsync(turno.Id);

        _shifts.Verify(s => s.Remove(turno), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteShift_ConOperarios_Lanza409YNoElimina()
    {
        var turno = AgregarTurno("Mañana", "06:00-14:00");
        turno.Operators.Add(new Operator
        {
            Id = Guid.CreateVersion7(), Age = 30, ShiftId = turno.Id, Shift = turno,
            Lines = [], Plans = [], ReinspectionRequests = []
        });

        await Assert.ThrowsAsync<ConflictException>(() => _service.DeleteShiftAsync(turno.Id));

        _shifts.Verify(s => s.Remove(It.IsAny<Shift>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteShift_Inexistente_Lanza404()
    {
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.DeleteShiftAsync(Guid.CreateVersion7()));
    }

    // ======================= CAT-5 y CAT-6 =======================

    [Fact]
    public void ListMaintenanceTypes_DevuelveLosValoresDelEnum()
    {
        Assert.Equal(new[] { "Preventive", "Corrective", "Predictive" }, _service.ListMaintenanceTypes());
    }

    [Fact]
    public void ListCriticalities_DevuelveLosValoresDelEnum()
    {
        Assert.Equal(new[] { "Low", "Medium", "High" }, _service.ListCriticalities());
    }
}
