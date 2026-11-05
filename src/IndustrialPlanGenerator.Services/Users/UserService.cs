using IndustrialPlanGenerator.Domain.Constants;
using IndustrialPlanGenerator.Domain.Entities;
using IndustrialPlanGenerator.Domain.Enums;
using IndustrialPlanGenerator.Domain.Exceptions;
using IndustrialPlanGenerator.Domain.Persistence;
using IndustrialPlanGenerator.Services.Auth;
using IndustrialPlanGenerator.Services.Contracts.Common;
using IndustrialPlanGenerator.Services.Contracts.Users;
using IndustrialPlanGenerator.Services.Exceptions;

namespace IndustrialPlanGenerator.Services.Users;

/// <inheritdoc cref="IUserService"/>
public sealed class UserService(
    IUnitOfWork uow,
    IPasswordHasher hasher,
    ICurrentUser currentUser) : IUserService
{
    /// <summary>Longitud mínima de la contraseña inicial (igual que en el cambio de contraseña).</summary>
    public const int MinimumPasswordLength = 8;
    public const int MaxNameLength = 200;
    public const int MaxSpecialtyLength = 200;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public async Task<PagedResult<UserSummaryDto>> ListAsync(
        string? role, Guid? lineId, string? text, int page, int pageSize, CancellationToken ct = default)
    {
        UserRole? parsedRole = string.IsNullOrWhiteSpace(role) ? null : ParseRole(role);
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);
        var search = string.IsNullOrWhiteSpace(text) ? null : text.Trim();

        var (users, total) = await uow.Users.SearchAsync(parsedRole, lineId, search, page, pageSize, ct);

        var items = users.Select(u => new UserSummaryDto(u.Id, u.Name, u.Role.ToString())).ToList();
        return new PagedResult<UserSummaryDto>(items, page, pageSize, total);
    }

    public async Task<UserDetailDto> GetAsync(Guid id, CancellationToken ct = default)
    {
        // El permiso se comprueba antes de buscar, para que un usuario sin permiso no pueda
        // averiguar qué ids existen.
        if (currentUser.Role != UserRole.Administrator && currentUser.Id != id)
            throw new ForbiddenException();

        var user = await uow.Users.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(typeof(User), id);

        switch (user.Role)
        {
            case UserRole.Technician:
                var technician = await uow.Technicians.GetWithLineAsync(user.Id, ct);
                var technicianLines = technician?.Lines
                    .Select(l => new UserLineDto(l.LineId, l.Permission.ToString())).ToList()
                    ?? [];
                return new UserDetailDto(user.Id, user.Name, user.Role.ToString(),
                    technician?.Specialty, null, null, null, technicianLines);

            case UserRole.Operator:
                var op = await uow.Operators.GetWithShiftAndLinesAsync(user.Id, ct);
                var operatorLines = op?.Lines.Select(l => new UserLineDto(l.LineId, null)).ToList() ?? [];
                return new UserDetailDto(user.Id, user.Name, user.Role.ToString(),
                    null, op?.Age, op?.ShiftId, op?.Shift?.Name, operatorLines);

            default:
                return new UserDetailDto(user.Id, user.Name, user.Role.ToString(), null, null, null, null, []);
        }
    }

    public async Task<UserDetailDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        // ---- Validación de la entrada (HTTP 400) ----
        var name = request.Name?.Trim();
        if (string.IsNullOrEmpty(name))
            throw new InvalidRequestException("El nombre de usuario es obligatorio.");
        if (name.Length > MaxNameLength)
            throw new InvalidRequestException($"El nombre de usuario no puede superar {MaxNameLength} caracteres.");

        if (string.IsNullOrEmpty(request.Password) || request.Password.Length < MinimumPasswordLength)
            throw new InvalidRequestException(
                $"La contraseña debe tener al menos {MinimumPasswordLength} caracteres.");

        var role = ParseRole(request.Role);
        var specialty = request.Specialty?.Trim();

        switch (role)
        {
            case UserRole.Technician:
                if (string.IsNullOrEmpty(specialty))
                    throw new InvalidRequestException("La especialidad es obligatoria para un técnico.");
                if (specialty.Length > MaxSpecialtyLength)
                    throw new InvalidRequestException($"La especialidad no puede superar {MaxSpecialtyLength} caracteres.");
                if (request.Age is not null || request.ShiftId is not null)
                    throw new InvalidRequestException("La edad y el turno solo aplican a operarios.");
                break;

            case UserRole.Operator:
                if (request.Age is null || request.ShiftId is null)
                    throw new InvalidRequestException("La edad y el turno son obligatorios para un operario.");
                if (!string.IsNullOrEmpty(specialty))
                    throw new InvalidRequestException("La especialidad solo aplica a técnicos.");
                break;

            default:
                if (!string.IsNullOrEmpty(specialty) || request.Age is not null || request.ShiftId is not null)
                    throw new InvalidRequestException(
                        "La especialidad, la edad y el turno no aplican a este rol.");
                break;
        }

        // ---- Reglas de negocio ----
        // Ojo: esta comprobación sola no evita dos altas simultáneas con el mismo nombre; para eso
        // la base de datos debe tener además un índice único sobre users.name.
        if (await uow.Users.IsUserNameExistingAsync(name, ct))
            throw new ConflictException($"Ya existe un usuario con el nombre '{name}'.");

        Shift? shift = null;
        if (role == UserRole.Operator)
        {
            if (request.Age!.Value < BusinessRules.MinimumOperatorAge)
                throw new DomainException($"Un operario debe tener al menos {BusinessRules.MinimumOperatorAge} años.");

            shift = await uow.Shifts.GetByIdAsync(request.ShiftId!.Value, ct)
                ?? throw new DomainException("El turno indicado no existe.");
        }

        // ---- Alta: usuario y registro de rol comparten Id y se guardan con un único SaveChanges ----
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Role = role,
            Credentials = hasher.Hash(request.Password)
        };
        await uow.Users.AddAsync(user, ct);

        switch (role)
        {
            case UserRole.Technician:
                await uow.Technicians.AddAsync(new Technician
                {
                    Id = user.Id,
                    Specialty = specialty!,
                    Lines = new List<TechnicianLine>(),
                    MaintenancePlans = new List<MaintenancePlan>(),
                    MaintenanceTasks = new List<MaintenanceTask>(),
                    ReinspectionRequests = new List<ReinspectionRequest>()
                }, ct);
                break;

            case UserRole.Operator:
                await uow.Operators.AddAsync(new Operator
                {
                    Id = user.Id,
                    Age = request.Age!.Value,
                    ShiftId = shift!.Id,
                    Shift = shift,
                    Lines = new List<OperatorLine>(),
                    Plans = new List<PlanAssignment>(),
                    ReinspectionRequests = new List<ReinspectionRequest>()
                }, ct);
                break;

            case UserRole.PlantManager:
                await uow.Users.AddPlantManagerAsync(new PlantManager
                {
                    Id = user.Id,
                    PlanValidations = new List<PlanValidation>()
                }, ct);
                break;

            case UserRole.Administrator:
                await uow.Users.AddAdministratorAsync(new Administrator { Id = user.Id }, ct);
                break;
        }

        await uow.SaveChangesAsync(ct);

        return new UserDetailDto(user.Id, user.Name, role.ToString(),
            role == UserRole.Technician ? specialty : null,
            role == UserRole.Operator ? request.Age : null,
            role == UserRole.Operator ? shift!.Id : null,
            role == UserRole.Operator ? shift!.Name : null,
            []);
    }

    private static UserRole ParseRole(string? value)
    {
        var text = value?.Trim();

        // Se rechazan los números ("1") para que el rol se indique siempre por nombre.
        if (string.IsNullOrEmpty(text)
            || int.TryParse(text, out _)
            || !Enum.TryParse<UserRole>(text, ignoreCase: true, out var role)
            || !Enum.IsDefined(role))
        {
            throw new InvalidRequestException(
                "Rol inválido. Valores permitidos: Administrator, Technician, Operator, PlantManager.");
        }

        return role;
    }
}
