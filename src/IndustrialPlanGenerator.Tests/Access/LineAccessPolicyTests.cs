using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Enums;
using IndustrialPlanGenerator.Domain.Persistence;
using IndustrialPlanGenerator.Domain.Repositories;
using IndustrialPlanGenerator.Services.Access;
using IndustrialPlanGenerator.Services.Auth;
using Moq;

namespace IndustrialPlanGenerator.Tests.Access;

public class LineAccessPolicyTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ITechnicianRepository> _technicians = new();
    private readonly Mock<IOperatorRepository> _operators = new();
    private readonly Mock<ICurrentUser> _current = new();
    private readonly LineAccessPolicy _policy;

    private readonly Guid _userId = Guid.CreateVersion7();
    private readonly Guid _lineId = Guid.CreateVersion7();

    public LineAccessPolicyTests()
    {
        _uow.Setup(u => u.Technicians).Returns(_technicians.Object);
        _uow.Setup(u => u.Operators).Returns(_operators.Object);
        _current.SetupGet(c => c.Id).Returns(_userId);
        _policy = new LineAccessPolicy(_uow.Object, _current.Object);
    }

    private void Como(UserRole rol) => _current.SetupGet(c => c.Role).Returns(rol);

    private void TecnicoConLinea(Guid lineId, TechnicianPermission permiso)
    {
        var tecnico = new Technician
        {
            Id = _userId, Specialty = "x",
            Lines = [], MaintenancePlans = [], MaintenanceTasks = [], ReinspectionRequests = []
        };
        tecnico.Lines.Add(new TechnicianLine
        {
            TechnicianId = _userId, LineId = lineId, Permission = permiso, Technician = tecnico, Line = null!
        });
        _technicians.Setup(t => t.GetWithLineAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(tecnico);
    }

    private void OperarioConLinea(Guid lineId)
    {
        var turno = new Shift { Id = Guid.CreateVersion7(), Name = "M", Schedule = "s", Operators = [] };
        var op = new Operator
        {
            Id = _userId, Age = 30, ShiftId = turno.Id, Shift = turno,
            Lines = [], Plans = [], ReinspectionRequests = []
        };
        op.Lines.Add(new OperatorLine { OperatorId = _userId, LineId = lineId, Operator = op, Line = null! });
        _operators.Setup(o => o.GetWithShiftAndLinesAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(op);
    }

    // ---------- ver ----------

    [Theory]
    [InlineData(UserRole.Administrator)]
    [InlineData(UserRole.PlantManager)]
    public async Task Administrador_y_jefe_ven_cualquier_linea(UserRole rol)
    {
        Como(rol);

        Assert.True(await _policy.CanViewAsync(_lineId));
    }

    [Fact]
    public async Task Tecnico_ve_sus_lineas_con_cualquier_permiso_y_no_las_ajenas()
    {
        Como(UserRole.Technician);
        TecnicoConLinea(_lineId, TechnicianPermission.Validate);

        Assert.True(await _policy.CanViewAsync(_lineId));
        Assert.False(await _policy.CanViewAsync(Guid.CreateVersion7()));
    }

    [Fact]
    public async Task Operario_ve_solo_sus_lineas()
    {
        Como(UserRole.Operator);
        OperarioConLinea(_lineId);

        Assert.True(await _policy.CanViewAsync(_lineId));
        Assert.False(await _policy.CanViewAsync(Guid.CreateVersion7()));
    }

    [Fact]
    public async Task Tecnico_u_operario_que_no_existe_no_ve_nada()
    {
        Como(UserRole.Technician);
        _technicians.Setup(t => t.GetWithLineAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync((Technician?)null);

        Assert.False(await _policy.CanViewAsync(_lineId));
    }

    // ---------- generar ----------

    [Fact]
    public async Task Solo_el_tecnico_con_permiso_Generate_genera()
    {
        Como(UserRole.Technician);
        _technicians.Setup(t => t.IsAuthorizedInLineAsync(
            _userId, _lineId, TechnicianPermission.Generate, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        Assert.True(await _policy.CanGenerateAsync(_lineId));
        Assert.False(await _policy.CanGenerateAsync(Guid.CreateVersion7()));
    }

    [Theory]
    [InlineData(UserRole.Administrator)]
    [InlineData(UserRole.PlantManager)]
    [InlineData(UserRole.Operator)]
    public async Task Los_demas_roles_no_generan(UserRole rol)
    {
        Como(rol);

        Assert.False(await _policy.CanGenerateAsync(_lineId));
        _technicians.Verify(t => t.IsAuthorizedInLineAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<TechnicianPermission>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---------- validar ----------

    [Fact]
    public async Task El_jefe_de_planta_valida_en_cualquier_linea()
    {
        Como(UserRole.PlantManager);

        Assert.True(await _policy.CanValidateAsync(_lineId));
    }

    [Fact]
    public async Task El_tecnico_valida_solo_con_permiso_Validate()
    {
        Como(UserRole.Technician);
        _technicians.Setup(t => t.IsAuthorizedInLineAsync(
            _userId, _lineId, TechnicianPermission.Validate, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        Assert.True(await _policy.CanValidateAsync(_lineId));
        Assert.False(await _policy.CanValidateAsync(Guid.CreateVersion7()));
    }

    [Theory]
    [InlineData(UserRole.Administrator)]
    [InlineData(UserRole.Operator)]
    public async Task Administrador_y_operario_no_validan(UserRole rol)
    {
        Como(rol);

        Assert.False(await _policy.CanValidateAsync(_lineId));
    }
}
