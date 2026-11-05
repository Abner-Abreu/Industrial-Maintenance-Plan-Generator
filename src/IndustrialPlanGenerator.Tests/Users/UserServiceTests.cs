using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Enums;
using IndustrialPlanGenerator.Domain.Exceptions;
using IndustrialPlanGenerator.Domain.Persistence;
using IndustrialPlanGenerator.Domain.Repositories;
using IndustrialPlanGenerator.Services.Auth;
using IndustrialPlanGenerator.Services.Contracts.Users;
using IndustrialPlanGenerator.Services.Exceptions;
using IndustrialPlanGenerator.Services.Users;
using Moq;

namespace IndustrialPlanGenerator.Tests.Users;

public class UserServiceTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<ITechnicianRepository> _technicians = new();
    private readonly Mock<IOperatorRepository> _operators = new();
    private readonly Mock<IShiftRepository> _shifts = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<ICurrentUser> _current = new();
    private readonly UserService _service;

    private static readonly Guid AdminId = Guid.CreateVersion7();

    public UserServiceTests()
    {
        _uow.Setup(u => u.Users).Returns(_users.Object);
        _uow.Setup(u => u.Technicians).Returns(_technicians.Object);
        _uow.Setup(u => u.Operators).Returns(_operators.Object);
        _uow.Setup(u => u.Shifts).Returns(_shifts.Object);
        _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hash-generado");
        ComoAdministrador();
        _service = new UserService(_uow.Object, _hasher.Object, _current.Object);
    }

    private void ComoAdministrador()
    {
        _current.SetupGet(c => c.Id).Returns(AdminId);
        _current.SetupGet(c => c.Role).Returns(UserRole.Administrator);
    }

    private void Como(Guid id, UserRole rol)
    {
        _current.SetupGet(c => c.Id).Returns(id);
        _current.SetupGet(c => c.Role).Returns(rol);
    }

    private static User CrearUsuario(string nombre, UserRole rol, Guid? id = null) =>
        new() { Id = id ?? Guid.CreateVersion7(), Name = nombre, Role = rol, Credentials = "hash" };

    private static Shift CrearTurno() =>
        new() { Id = Guid.CreateVersion7(), Name = "Mañana", Schedule = "06:00-14:00", Operators = [] };

    // ======================= USR-1: listado =======================

    [Fact]
    public async Task Listar_traduce_los_filtros_y_devuelve_la_pagina_con_el_total()
    {
        var lineId = Guid.CreateVersion7();
        var ana = CrearUsuario("ana", UserRole.Technician);
        _users.Setup(u => u.SearchAsync(UserRole.Technician, lineId, "an", 2, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<User> { ana }, 11));

        var resultado = await _service.ListAsync("technician", lineId, "  an ", 2, 10);

        Assert.Equal(11, resultado.TotalCount);
        Assert.Equal(2, resultado.Page);
        Assert.Equal(10, resultado.PageSize);
        var item = Assert.Single(resultado.Items);
        Assert.Equal(new UserSummaryDto(ana.Id, "ana", "Technician"), item);
    }

    [Fact]
    public async Task Listar_sin_filtros_usa_pagina_1_y_tamano_por_defecto()
    {
        _users.Setup(u => u.SearchAsync(null, null, null, 1, UserService.DefaultPageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<User>(), 0));

        var resultado = await _service.ListAsync(null, null, "   ", 0, 0);

        Assert.Equal(1, resultado.Page);
        Assert.Equal(UserService.DefaultPageSize, resultado.PageSize);
        Assert.Empty(resultado.Items);
    }

    [Fact]
    public async Task Listar_limita_el_tamano_de_pagina_al_maximo()
    {
        _users.Setup(u => u.SearchAsync(null, null, null, 1, UserService.MaxPageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<User>(), 0));

        var resultado = await _service.ListAsync(null, null, null, 1, 5000);

        Assert.Equal(UserService.MaxPageSize, resultado.PageSize);
    }

    [Theory]
    [InlineData("jefe")]
    [InlineData("1")]
    [InlineData("99")]
    public async Task Listar_con_un_rol_invalido_lanza_InvalidRequestException(string rol)
    {
        await Assert.ThrowsAsync<InvalidRequestException>(() => _service.ListAsync(rol, null, null, 1, 10));
    }

    // ======================= USR-2: detalle =======================

    [Fact]
    public async Task Obtener_como_administrador_devuelve_un_tecnico_con_sus_lineas_y_permisos()
    {
        var user = CrearUsuario("ana", UserRole.Technician);
        var lineaId = Guid.CreateVersion7();
        var tecnico = new Technician
        {
            Id = user.Id, Specialty = "Mecánica",
            Lines = [new TechnicianLine
            {
                TechnicianId = user.Id, LineId = lineaId, Permission = TechnicianPermission.Validate,
                Technician = null!, Line = null!
            }],
            MaintenancePlans = [], MaintenanceTasks = [], ReinspectionRequests = []
        };
        _users.Setup(u => u.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _technicians.Setup(t => t.GetWithLineAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(tecnico);

        var dto = await _service.GetAsync(user.Id);

        Assert.Equal("Technician", dto.Role);
        Assert.Equal("Mecánica", dto.Specialty);
        var linea = Assert.Single(dto.Lines);
        Assert.Equal(lineaId, linea.LineId);
        Assert.Equal("Validate", linea.Permission);
        Assert.Null(dto.Age);
    }

    [Fact]
    public async Task Obtener_un_operario_incluye_edad_turno_y_lineas()
    {
        var user = CrearUsuario("luis", UserRole.Operator);
        var turno = CrearTurno();
        var lineaId = Guid.CreateVersion7();
        var op = new Operator
        {
            Id = user.Id, Age = 30, ShiftId = turno.Id, Shift = turno,
            Lines = [new OperatorLine { OperatorId = user.Id, LineId = lineaId, Operator = null!, Line = null! }],
            Plans = [], ReinspectionRequests = []
        };
        _users.Setup(u => u.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _operators.Setup(o => o.GetWithShiftAndLinesAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(op);

        var dto = await _service.GetAsync(user.Id);

        Assert.Equal(30, dto.Age);
        Assert.Equal(turno.Id, dto.ShiftId);
        Assert.Equal("Mañana", dto.ShiftName);
        Assert.Equal(lineaId, Assert.Single(dto.Lines).LineId);
        Assert.Null(dto.Lines[0].Permission);
    }

    [Fact]
    public async Task Obtener_el_propio_usuario_sin_ser_administrador_esta_permitido()
    {
        var user = CrearUsuario("ana", UserRole.PlantManager);
        Como(user.Id, UserRole.PlantManager);
        _users.Setup(u => u.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var dto = await _service.GetAsync(user.Id);

        Assert.Equal("ana", dto.Name);
        Assert.Empty(dto.Lines);
    }

    [Fact]
    public async Task Obtener_otro_usuario_sin_ser_administrador_lanza_ForbiddenException_sin_consultar()
    {
        Como(Guid.CreateVersion7(), UserRole.Technician);

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.GetAsync(Guid.CreateVersion7()));

        _users.Verify(u => u.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Obtener_un_usuario_inexistente_lanza_EntityNotFoundException()
    {
        var id = Guid.CreateVersion7();
        _users.Setup(u => u.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.GetAsync(id));
    }

    // ======================= USR-3: alta =======================

    private void CapturarAltas(out List<User> usuarios)
    {
        var lista = new List<User>();
        _users.Setup(u => u.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => lista.Add(u)).Returns(Task.CompletedTask);
        usuarios = lista;
    }

    [Fact]
    public async Task Crear_administrador_guarda_usuario_con_hash_y_registro_de_rol_con_el_mismo_id()
    {
        CapturarAltas(out var usuarios);
        Administrator? admin = null;
        _users.Setup(u => u.AddAdministratorAsync(It.IsAny<Administrator>(), It.IsAny<CancellationToken>()))
            .Callback<Administrator, CancellationToken>((a, _) => admin = a).Returns(Task.CompletedTask);

        var dto = await _service.CreateAsync(new CreateUserRequest(" root ", "clave-1234", "administrator"));

        var user = Assert.Single(usuarios);
        Assert.Equal("root", user.Name);
        Assert.Equal(UserRole.Administrator, user.Role);
        Assert.Equal("hash-generado", user.Credentials);
        Assert.NotNull(admin);
        Assert.Equal(user.Id, admin!.Id);
        Assert.Equal(user.Id, dto.Id);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Crear_tecnico_guarda_su_especialidad_con_el_mismo_id_del_usuario()
    {
        CapturarAltas(out var usuarios);
        Technician? tecnico = null;
        _technicians.Setup(t => t.AddAsync(It.IsAny<Technician>(), It.IsAny<CancellationToken>()))
            .Callback<Technician, CancellationToken>((t, _) => tecnico = t).Returns(Task.CompletedTask);

        var dto = await _service.CreateAsync(new CreateUserRequest("ana", "clave-1234", "Technician", Specialty: " Mecánica "));

        Assert.NotNull(tecnico);
        Assert.Equal(usuarios[0].Id, tecnico!.Id);
        Assert.Equal("Mecánica", tecnico.Specialty);
        Assert.Equal("Mecánica", dto.Specialty);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Crear_operario_guarda_edad_y_turno()
    {
        CapturarAltas(out var usuarios);
        var turno = CrearTurno();
        _shifts.Setup(s => s.GetByIdAsync(turno.Id, It.IsAny<CancellationToken>())).ReturnsAsync(turno);
        Operator? op = null;
        _operators.Setup(o => o.AddAsync(It.IsAny<Operator>(), It.IsAny<CancellationToken>()))
            .Callback<Operator, CancellationToken>((o, _) => op = o).Returns(Task.CompletedTask);

        var dto = await _service.CreateAsync(new CreateUserRequest("luis", "clave-1234", "Operator", Age: 25, ShiftId: turno.Id));

        Assert.NotNull(op);
        Assert.Equal(usuarios[0].Id, op!.Id);
        Assert.Equal(25, op.Age);
        Assert.Equal(turno.Id, op.ShiftId);
        Assert.Equal("Mañana", dto.ShiftName);
    }

    [Fact]
    public async Task Crear_jefe_de_planta_usa_su_propio_registro_de_rol()
    {
        CapturarAltas(out _);

        await _service.CreateAsync(new CreateUserRequest("jefa", "clave-1234", "PlantManager"));

        _users.Verify(u => u.AddPlantManagerAsync(It.IsAny<PlantManager>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Crear_con_un_nombre_ya_existente_lanza_ConflictException_y_no_guarda()
    {
        _users.Setup(u => u.IsUserNameExistingAsync("ana", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.CreateAsync(new CreateUserRequest("ana", "clave-1234", "Administrator")));

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("", "clave-1234", "Administrator")]
    [InlineData("   ", "clave-1234", "Administrator")]
    [InlineData("ana", "corta", "Administrator")]
    [InlineData("ana", "", "Administrator")]
    [InlineData("ana", "clave-1234", "")]
    [InlineData("ana", "clave-1234", "jefe")]
    [InlineData("ana", "clave-1234", "2")]
    public async Task Crear_con_nombre_contrasena_o_rol_invalidos_lanza_InvalidRequestException(
        string nombre, string clave, string rol)
    {
        await Assert.ThrowsAsync<InvalidRequestException>(
            () => _service.CreateAsync(new CreateUserRequest(nombre, clave, rol)));

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Crear_un_nombre_demasiado_largo_lanza_InvalidRequestException()
    {
        var largo = new string('a', UserService.MaxNameLength + 1);

        await Assert.ThrowsAsync<InvalidRequestException>(
            () => _service.CreateAsync(new CreateUserRequest(largo, "clave-1234", "Administrator")));
    }

    [Fact]
    public async Task Crear_tecnico_sin_especialidad_lanza_InvalidRequestException()
    {
        await Assert.ThrowsAsync<InvalidRequestException>(
            () => _service.CreateAsync(new CreateUserRequest("ana", "clave-1234", "Technician")));
    }

    [Fact]
    public async Task Crear_tecnico_con_edad_o_turno_lanza_InvalidRequestException()
    {
        await Assert.ThrowsAsync<InvalidRequestException>(
            () => _service.CreateAsync(new CreateUserRequest("ana", "clave-1234", "Technician", "Mecánica", Age: 30)));
    }

    [Fact]
    public async Task Crear_operario_sin_edad_o_sin_turno_lanza_InvalidRequestException()
    {
        await Assert.ThrowsAsync<InvalidRequestException>(
            () => _service.CreateAsync(new CreateUserRequest("luis", "clave-1234", "Operator", Age: 30)));
        await Assert.ThrowsAsync<InvalidRequestException>(
            () => _service.CreateAsync(new CreateUserRequest("luis", "clave-1234", "Operator", ShiftId: Guid.CreateVersion7())));
    }

    [Fact]
    public async Task Crear_operario_con_especialidad_lanza_InvalidRequestException()
    {
        await Assert.ThrowsAsync<InvalidRequestException>(
            () => _service.CreateAsync(
                new CreateUserRequest("luis", "clave-1234", "Operator", "Mecánica", 30, Guid.CreateVersion7())));
    }

    [Fact]
    public async Task Crear_administrador_con_datos_de_otro_rol_lanza_InvalidRequestException()
    {
        await Assert.ThrowsAsync<InvalidRequestException>(
            () => _service.CreateAsync(new CreateUserRequest("root", "clave-1234", "Administrator", Specialty: "Mecánica")));
    }

    [Fact]
    public async Task Crear_operario_menor_de_edad_lanza_DomainException()
    {
        await Assert.ThrowsAsync<DomainException>(
            () => _service.CreateAsync(new CreateUserRequest("luis", "clave-1234", "Operator", Age: 17, ShiftId: Guid.CreateVersion7())));

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Crear_operario_con_un_turno_inexistente_lanza_DomainException()
    {
        var turnoId = Guid.CreateVersion7();
        _shifts.Setup(s => s.GetByIdAsync(turnoId, It.IsAny<CancellationToken>())).ReturnsAsync((Shift?)null);

        await Assert.ThrowsAsync<DomainException>(
            () => _service.CreateAsync(new CreateUserRequest("luis", "clave-1234", "Operator", Age: 30, ShiftId: turnoId)));

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
