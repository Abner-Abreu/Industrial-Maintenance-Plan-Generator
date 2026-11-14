using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Enums;
using IndustrialPlanGenerator.Domain.Exceptions;
using IndustrialPlanGenerator.Domain.Persistence;
using IndustrialPlanGenerator.Domain.Repositories;
using IndustrialPlanGenerator.Services.Access;
using IndustrialPlanGenerator.Services.Auth;
using IndustrialPlanGenerator.Services.Users;
using Moq;

namespace IndustrialPlanGenerator.Tests.Users;

/// <summary>Tests for USR-5 (soft delete of a user).</summary>
public class UserServiceDeleteTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<ITechnicianRepository> _technicians = new();
    private readonly Mock<IOperatorRepository> _operators = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<ICurrentUser> _current = new();
    private readonly Mock<ILineAccessPolicy> _access = new();
    private readonly Guid _currentId = Guid.CreateVersion7();
    private readonly UserService _service;

    public UserServiceDeleteTests()
    {
        _uow.Setup(u => u.Users).Returns(_users.Object);
        _uow.Setup(u => u.Technicians).Returns(_technicians.Object);
        _uow.Setup(u => u.Operators).Returns(_operators.Object);
        _current.SetupGet(c => c.Id).Returns(_currentId);
        _current.SetupGet(c => c.Role).Returns(UserRole.Administrator);
        _service = new UserService(_uow.Object, _hasher.Object, _current.Object, _access.Object);
    }

    // ---------- helpers ----------

    private User HayUsuario(UserRole rol)
    {
        var user = new User { Id = Guid.CreateVersion7(), Name = "usuario", Role = rol, Credentials = "hash" };
        _users.Setup(u => u.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        return user;
    }

    private void HayAdministradoresActivos(int total) =>
        _users.Setup(u => u.SearchAsync(
                UserRole.Administrator, It.IsAny<Guid?>(), It.IsAny<string?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(((IReadOnlyList<User>)new List<User>(), total));

    // ---------- roles ----------

    [Fact]
    public async Task Delete_Tecnico_MarcaElRegistroDeRolYElUsuario()
    {
        var user = HayUsuario(UserRole.Technician);
        var tecnico = new Technician
        {
            Id = user.Id, Specialty = "Mecánica",
            Lines = [], MaintenancePlans = [], MaintenanceTasks = [], ReinspectionRequests = []
        };
        _technicians.Setup(t => t.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(tecnico);

        await _service.DeleteAsync(user.Id);

        _technicians.Verify(t => t.Remove(tecnico), Times.Once);
        _users.Verify(u => u.Remove(user), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_Operario_MarcaElRegistroDeRolYElUsuario()
    {
        var user = HayUsuario(UserRole.Operator);
        var turno = new Shift { Id = Guid.CreateVersion7(), Name = "M", Schedule = "s", Operators = [] };
        var op = new Operator
        {
            Id = user.Id, Age = 30, ShiftId = turno.Id, Shift = turno,
            Lines = [], Plans = [], ReinspectionRequests = []
        };
        _operators.Setup(o => o.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(op);

        await _service.DeleteAsync(user.Id);

        _operators.Verify(o => o.Remove(op), Times.Once);
        _users.Verify(u => u.Remove(user), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_JefeDePlanta_MarcaElRegistroDeRolYElUsuario()
    {
        var user = HayUsuario(UserRole.PlantManager);
        var jefe = new PlantManager { Id = user.Id, PlanValidations = [] };
        _users.Setup(u => u.GetPlantManagerAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(jefe);

        await _service.DeleteAsync(user.Id);

        _users.Verify(u => u.RemovePlantManager(jefe), Times.Once);
        _users.Verify(u => u.Remove(user), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_Administrador_ConOtrosAdministradores_LoMarca()
    {
        var user = HayUsuario(UserRole.Administrator);
        var admin = new Administrator { Id = user.Id };
        _users.Setup(u => u.GetAdministratorAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(admin);
        HayAdministradoresActivos(2);

        await _service.DeleteAsync(user.Id);

        Assert.True(admin.IsDeleted);
        _users.Verify(u => u.Remove(user), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---------- reglas ----------

    [Fact]
    public async Task Delete_UnoMismo_Lanza422SinBuscarNiGuardar()
    {
        await Assert.ThrowsAsync<DomainException>(() => _service.DeleteAsync(_currentId));

        _users.Verify(u => u.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Delete_ElUltimoAdministrador_Lanza422YNoCambiaNada()
    {
        var user = HayUsuario(UserRole.Administrator);
        var admin = new Administrator { Id = user.Id };
        _users.Setup(u => u.GetAdministratorAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(admin);
        HayAdministradoresActivos(1);

        await Assert.ThrowsAsync<DomainException>(() => _service.DeleteAsync(user.Id));

        Assert.False(admin.IsDeleted);
        _users.Verify(u => u.Remove(It.IsAny<User>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Delete_UsuarioInexistenteOYaEliminado_Lanza404()
    {
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.DeleteAsync(Guid.CreateVersion7()));

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Delete_SinRegistroDeRol_Lanza404YNoMarcaElUsuario()
    {
        var user = HayUsuario(UserRole.Technician);   // el técnico no existe

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.DeleteAsync(user.Id));

        _users.Verify(u => u.Remove(It.IsAny<User>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
