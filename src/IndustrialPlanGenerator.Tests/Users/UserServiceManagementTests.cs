using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Enums;
using IndustrialPlanGenerator.Domain.Exceptions;
using IndustrialPlanGenerator.Domain.Persistence;
using IndustrialPlanGenerator.Domain.Repositories;
using IndustrialPlanGenerator.Services.Access;
using IndustrialPlanGenerator.Services.Auth;
using IndustrialPlanGenerator.Services.Contracts.Users;
using IndustrialPlanGenerator.Services.Exceptions;
using IndustrialPlanGenerator.Services.Users;
using Moq;

namespace IndustrialPlanGenerator.Tests.Users;

/// <summary>Tests for USR-4 (edit), USR-6 and USR-7 (lines), and USR-8 (technicians of a line).</summary>
public class UserServiceManagementTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<ITechnicianRepository> _technicians = new();
    private readonly Mock<IOperatorRepository> _operators = new();
    private readonly Mock<IShiftRepository> _shifts = new();
    private readonly Mock<IProductionLineRepository> _lines = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<ICurrentUser> _current = new();
    private readonly Mock<ILineAccessPolicy> _access = new();
    private readonly UserService _service;

    public UserServiceManagementTests()
    {
        _uow.Setup(u => u.Users).Returns(_users.Object);
        _uow.Setup(u => u.Technicians).Returns(_technicians.Object);
        _uow.Setup(u => u.Operators).Returns(_operators.Object);
        _uow.Setup(u => u.Shifts).Returns(_shifts.Object);
        _uow.Setup(u => u.Line).Returns(_lines.Object);
        _current.SetupGet(c => c.Id).Returns(Guid.CreateVersion7());
        _current.SetupGet(c => c.Role).Returns(UserRole.Administrator);
        _service = new UserService(_uow.Object, _hasher.Object, _current.Object, _access.Object);
    }

    // ---------- helpers ----------

    private static User Usuario(string nombre, UserRole rol) =>
        new() { Id = Guid.CreateVersion7(), Name = nombre, Role = rol, Credentials = "hash" };

    private static ProductionLine Linea(string nombre = "L") =>
        new()
        {
            Id = Guid.CreateVersion7(), Name = nombre, ProductiveProcess = "p",
            EquipmentSystems = [], TechnicianLines = [], Operators = [], MaintenancePlans = []
        };

    private static Technician Tecnico(Guid id, string especialidad = "Mecánica") =>
        new()
        {
            Id = id, Specialty = especialidad,
            Lines = [], MaintenancePlans = [], MaintenanceTasks = [], ReinspectionRequests = []
        };

    private static TechnicianLine Enlace(Technician t, Guid lineId, TechnicianPermission p) =>
        new() { TechnicianId = t.Id, LineId = lineId, Permission = p, Technician = t, Line = null! };

    private static Shift Turno(string nombre = "Mañana") =>
        new() { Id = Guid.CreateVersion7(), Name = nombre, Schedule = "06:00-14:00", Operators = [] };

    private static Operator Operario(Guid id, Shift turno, int edad = 30) =>
        new()
        {
            Id = id, Age = edad, ShiftId = turno.Id, Shift = turno,
            Lines = [], Plans = [], ReinspectionRequests = []
        };

    private void Existe(User u) =>
        _users.Setup(x => x.GetByIdAsync(u.Id, It.IsAny<CancellationToken>())).ReturnsAsync(u);

    private void ExistenLineas(params ProductionLine[] lineas) =>
        _lines.Setup(l => l.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(lineas);

    // ======================= USR-4: editar =======================

    [Fact]
    public async Task Editar_tecnico_cambia_nombre_y_especialidad_y_guarda()
    {
        var user = Usuario("ana", UserRole.Technician);
        var tecnico = Tecnico(user.Id, "Mecánica");
        Existe(user);
        _technicians.Setup(t => t.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(tecnico);
        _technicians.Setup(t => t.GetWithLineAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(tecnico);

        var dto = await _service.UpdateAsync(user.Id, new UpdateUserRequest(" ana.lopez ", " Eléctrica "));

        Assert.Equal("ana.lopez", user.Name);
        Assert.Equal("Eléctrica", tecnico.Specialty);
        Assert.Equal("ana.lopez", dto.Name);
        Assert.Equal("Eléctrica", dto.Specialty);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Editar_operario_cambia_edad_y_turno()
    {
        var user = Usuario("luis", UserRole.Operator);
        var viejo = Turno("Mañana");
        var nuevo = Turno("Noche");
        var op = Operario(user.Id, viejo, 30);
        Existe(user);
        _operators.Setup(o => o.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(op);
        _operators.Setup(o => o.GetWithShiftAndLinesAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(op);
        _shifts.Setup(s => s.GetByIdAsync(nuevo.Id, It.IsAny<CancellationToken>())).ReturnsAsync(nuevo);

        var dto = await _service.UpdateAsync(user.Id, new UpdateUserRequest("luis", Age: 41, ShiftId: nuevo.Id));

        Assert.Equal(41, op.Age);
        Assert.Equal(nuevo.Id, op.ShiftId);
        Assert.Equal("Noche", dto.ShiftName);
    }

    [Fact]
    public async Task Editar_permite_conservar_el_propio_nombre()
    {
        var user = Usuario("root", UserRole.Administrator);
        Existe(user);
        _users.Setup(u => u.GetByUserNameAsync("root", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var dto = await _service.UpdateAsync(user.Id, new UpdateUserRequest("root"));

        Assert.Equal("root", dto.Name);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Editar_con_el_nombre_de_otro_usuario_lanza_ConflictException()
    {
        var user = Usuario("ana", UserRole.Administrator);
        var otro = Usuario("luis", UserRole.Administrator);
        Existe(user);
        _users.Setup(u => u.GetByUserNameAsync("luis", It.IsAny<CancellationToken>())).ReturnsAsync(otro);

        await Assert.ThrowsAsync<ConflictException>(() => _service.UpdateAsync(user.Id, new UpdateUserRequest("luis")));

        Assert.Equal("ana", user.Name);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Editar_un_usuario_inexistente_lanza_EntityNotFoundException()
    {
        var id = Guid.CreateVersion7();
        _users.Setup(u => u.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.UpdateAsync(id, new UpdateUserRequest("x")));
    }

    [Fact]
    public async Task Editar_tecnico_sin_especialidad_lanza_InvalidRequestException()
    {
        var user = Usuario("ana", UserRole.Technician);
        Existe(user);

        await Assert.ThrowsAsync<InvalidRequestException>(() => _service.UpdateAsync(user.Id, new UpdateUserRequest("ana")));
    }

    [Fact]
    public async Task Editar_administrador_con_especialidad_lanza_InvalidRequestException()
    {
        var user = Usuario("root", UserRole.Administrator);
        Existe(user);

        await Assert.ThrowsAsync<InvalidRequestException>(
            () => _service.UpdateAsync(user.Id, new UpdateUserRequest("root", Specialty: "Mecánica")));
    }

    [Fact]
    public async Task Editar_nombre_vacio_lanza_InvalidRequestException()
    {
        var user = Usuario("root", UserRole.Administrator);
        Existe(user);

        await Assert.ThrowsAsync<InvalidRequestException>(() => _service.UpdateAsync(user.Id, new UpdateUserRequest("  ")));
    }

    [Fact]
    public async Task Editar_operario_menor_de_edad_lanza_DomainException_y_no_guarda()
    {
        var user = Usuario("luis", UserRole.Operator);
        Existe(user);

        await Assert.ThrowsAsync<DomainException>(
            () => _service.UpdateAsync(user.Id, new UpdateUserRequest("luis", Age: 16, ShiftId: Guid.CreateVersion7())));

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ======================= USR-6: líneas del técnico =======================

    [Fact]
    public async Task Lineas_del_tecnico_se_sincronizan_quita_cambia_y_agrega()
    {
        var user = Usuario("ana", UserRole.Technician);
        var tecnico = Tecnico(user.Id);
        var l1 = Linea("1"); var l2 = Linea("2"); var l3 = Linea("3");
        tecnico.Lines.Add(Enlace(tecnico, l1.Id, TechnicianPermission.Generate));
        tecnico.Lines.Add(Enlace(tecnico, l2.Id, TechnicianPermission.Generate));
        Existe(user);
        _technicians.Setup(t => t.GetWithLineAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(tecnico);
        ExistenLineas(l1, l2, l3);

        var dto = await _service.SetTechnicianLinesAsync(user.Id, new SetTechnicianLinesRequest(
            [new UserLineDto(l1.Id, "validate"), new UserLineDto(l3.Id, "Generate")]));

        Assert.Equal(2, tecnico.Lines.Count);
        Assert.Equal(TechnicianPermission.Validate, tecnico.Lines.Single(l => l.LineId == l1.Id).Permission);
        Assert.Equal(TechnicianPermission.Generate, tecnico.Lines.Single(l => l.LineId == l3.Id).Permission);
        Assert.DoesNotContain(tecnico.Lines, l => l.LineId == l2.Id);
        Assert.Equal(2, dto.Lines.Count);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Lineas_del_tecnico_con_lista_vacia_quita_todas()
    {
        var user = Usuario("ana", UserRole.Technician);
        var tecnico = Tecnico(user.Id);
        var l1 = Linea();
        tecnico.Lines.Add(Enlace(tecnico, l1.Id, TechnicianPermission.Generate));
        Existe(user);
        _technicians.Setup(t => t.GetWithLineAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(tecnico);
        ExistenLineas(l1);

        var dto = await _service.SetTechnicianLinesAsync(user.Id, new SetTechnicianLinesRequest([]));

        Assert.Empty(tecnico.Lines);
        Assert.Empty(dto.Lines);
    }

    [Fact]
    public async Task Lineas_del_tecnico_repetidas_lanzan_InvalidRequestException()
    {
        var id = Guid.CreateVersion7();
        var lineaId = Guid.CreateVersion7();

        await Assert.ThrowsAsync<InvalidRequestException>(() => _service.SetTechnicianLinesAsync(id,
            new SetTechnicianLinesRequest([new UserLineDto(lineaId, "Generate"), new UserLineDto(lineaId, "Validate")])));
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("")]
    [InlineData("0")]
    public async Task Lineas_del_tecnico_con_permiso_invalido_lanzan_InvalidRequestException(string permiso)
    {
        await Assert.ThrowsAsync<InvalidRequestException>(() => _service.SetTechnicianLinesAsync(Guid.CreateVersion7(),
            new SetTechnicianLinesRequest([new UserLineDto(Guid.CreateVersion7(), permiso)])));
    }

    [Fact]
    public async Task Lineas_del_tecnico_con_una_linea_inexistente_lanzan_DomainException_y_no_guardan()
    {
        var user = Usuario("ana", UserRole.Technician);
        Existe(user);
        _technicians.Setup(t => t.GetWithLineAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(Tecnico(user.Id));
        ExistenLineas(Linea());

        await Assert.ThrowsAsync<DomainException>(() => _service.SetTechnicianLinesAsync(user.Id,
            new SetTechnicianLinesRequest([new UserLineDto(Guid.CreateVersion7(), "Generate")])));

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Lineas_del_tecnico_de_un_usuario_que_no_es_tecnico_lanzan_DomainException()
    {
        var user = Usuario("luis", UserRole.Operator);
        Existe(user);

        await Assert.ThrowsAsync<DomainException>(
            () => _service.SetTechnicianLinesAsync(user.Id, new SetTechnicianLinesRequest([])));
    }

    [Fact]
    public async Task Lineas_del_tecnico_de_un_usuario_inexistente_lanzan_EntityNotFoundException()
    {
        var id = Guid.CreateVersion7();
        _users.Setup(u => u.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _service.SetTechnicianLinesAsync(id, new SetTechnicianLinesRequest([])));
    }

    // ======================= USR-7: líneas del operario =======================

    [Fact]
    public async Task Lineas_del_operario_se_sincronizan()
    {
        var user = Usuario("luis", UserRole.Operator);
        var op = Operario(user.Id, Turno());
        var l1 = Linea("1"); var l2 = Linea("2"); var l3 = Linea("3");
        op.Lines.Add(new OperatorLine { OperatorId = op.Id, LineId = l1.Id, Operator = op, Line = null! });
        op.Lines.Add(new OperatorLine { OperatorId = op.Id, LineId = l2.Id, Operator = op, Line = null! });
        Existe(user);
        _operators.Setup(o => o.GetWithShiftAndLinesAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(op);
        ExistenLineas(l1, l2, l3);

        var dto = await _service.SetOperatorLinesAsync(user.Id, new SetOperatorLinesRequest([l2.Id, l3.Id]));

        Assert.Equal(2, op.Lines.Count);
        Assert.DoesNotContain(op.Lines, l => l.LineId == l1.Id);
        Assert.Contains(op.Lines, l => l.LineId == l3.Id);
        Assert.Equal(2, dto.Lines.Count);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Lineas_del_operario_repetidas_lanzan_InvalidRequestException()
    {
        var lineaId = Guid.CreateVersion7();

        await Assert.ThrowsAsync<InvalidRequestException>(
            () => _service.SetOperatorLinesAsync(Guid.CreateVersion7(), new SetOperatorLinesRequest([lineaId, lineaId])));
    }

    [Fact]
    public async Task Lineas_del_operario_con_una_linea_inexistente_lanzan_DomainException()
    {
        var user = Usuario("luis", UserRole.Operator);
        Existe(user);
        _operators.Setup(o => o.GetWithShiftAndLinesAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Operario(user.Id, Turno()));
        ExistenLineas();

        await Assert.ThrowsAsync<DomainException>(
            () => _service.SetOperatorLinesAsync(user.Id, new SetOperatorLinesRequest([Guid.CreateVersion7()])));

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Lineas_del_operario_de_un_usuario_que_no_es_operario_lanzan_DomainException()
    {
        var user = Usuario("ana", UserRole.Technician);
        Existe(user);

        await Assert.ThrowsAsync<DomainException>(
            () => _service.SetOperatorLinesAsync(user.Id, new SetOperatorLinesRequest([])));
    }

    // ======================= USR-8: técnicos de una línea =======================

    private void ConfigurarLineaConTecnicos(
        ProductionLine linea, params (User User, Technician Tech, TechnicianPermission Permission)[] items)
    {
        _access.Setup(a => a.CanViewAsync(linea.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _lines.Setup(l => l.GetByIdAsync(linea.Id, It.IsAny<CancellationToken>())).ReturnsAsync(linea);

        foreach (var (_, tech, permission) in items)
            tech.Lines.Add(Enlace(tech, linea.Id, permission));

        _technicians.Setup(t => t.GetByLineAsync(linea.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(items.Select(i => i.Tech).ToList());
        _users.Setup(u => u.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(items.Select(i => i.User).ToList());
    }

    [Fact]
    public async Task Tecnicos_de_la_linea_devuelve_nombre_especialidad_y_permiso_ordenados_por_nombre()
    {
        var linea = Linea();
        var zoe = Usuario("zoe", UserRole.Technician);
        var ana = Usuario("ana", UserRole.Technician);
        ConfigurarLineaConTecnicos(linea,
            (zoe, Tecnico(zoe.Id, "Eléctrica"), TechnicianPermission.Validate),
            (ana, Tecnico(ana.Id, "Mecánica"), TechnicianPermission.Generate));

        var lista = await _service.ListLineTechniciansAsync(linea.Id, null);

        Assert.Equal(["ana", "zoe"], lista.Select(t => t.Name).ToArray());
        Assert.Equal("Mecánica", lista[0].Specialty);
        Assert.Equal("Generate", lista[0].Permission);
        Assert.Equal("Validate", lista[1].Permission);
    }

    [Fact]
    public async Task Tecnicos_de_la_linea_filtra_por_permiso()
    {
        var linea = Linea();
        var zoe = Usuario("zoe", UserRole.Technician);
        var ana = Usuario("ana", UserRole.Technician);
        ConfigurarLineaConTecnicos(linea,
            (zoe, Tecnico(zoe.Id), TechnicianPermission.Validate),
            (ana, Tecnico(ana.Id), TechnicianPermission.Generate));

        var lista = await _service.ListLineTechniciansAsync(linea.Id, "validate");

        Assert.Equal("zoe", Assert.Single(lista).Name);
    }

    [Fact]
    public async Task Tecnicos_de_la_linea_sin_acceso_lanza_ForbiddenException_sin_consultar()
    {
        var lineaId = Guid.CreateVersion7();
        _access.Setup(a => a.CanViewAsync(lineaId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.ListLineTechniciansAsync(lineaId, null));

        _lines.Verify(l => l.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _technicians.Verify(t => t.GetByLineAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Tecnicos_de_una_linea_inexistente_lanza_EntityNotFoundException()
    {
        var lineaId = Guid.CreateVersion7();
        _access.Setup(a => a.CanViewAsync(lineaId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _lines.Setup(l => l.GetByIdAsync(lineaId, It.IsAny<CancellationToken>())).ReturnsAsync((ProductionLine?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.ListLineTechniciansAsync(lineaId, null));
    }

    [Fact]
    public async Task Tecnicos_de_la_linea_con_permiso_invalido_lanza_InvalidRequestException()
    {
        await Assert.ThrowsAsync<InvalidRequestException>(
            () => _service.ListLineTechniciansAsync(Guid.CreateVersion7(), "todos"));
    }
}
