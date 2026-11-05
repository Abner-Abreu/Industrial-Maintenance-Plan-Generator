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

    // ======================= USR-1 =======================

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

    // ======================= USR-2 =======================

    public async Task<UserDetailDto> GetAsync(Guid id, CancellationToken ct = default)
    {
        // El permiso se comprueba antes de buscar, para que un usuario sin permiso no pueda
        // averiguar qué ids existen.
        if (currentUser.Role != UserRole.Administrator && currentUser.Id != id)
            throw new ForbiddenException();

        var user = await uow.Users.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(typeof(User), id);

        return await BuildDetailAsync(user, ct);
    }

    // ======================= USR-3 =======================

    public async Task<UserDetailDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        // ---- Validación de la entrada (HTTP 400) ----
        var name = ValidateName(request.Name);

        if (string.IsNullOrEmpty(request.Password) || request.Password.Length < MinimumPasswordLength)
            throw new InvalidRequestException(
                $"La contraseña debe tener al menos {MinimumPasswordLength} caracteres.");

        var role = ParseRole(request.Role);
        var specialty = ValidateRoleData(role, request.Specialty, request.Age, request.ShiftId);

        // ---- Reglas de negocio ----
        // Ojo: esta comprobación sola no evita dos altas simultáneas con el mismo nombre; para eso
        // la base de datos debe tener además un índice único sobre users.name.
        if (await uow.Users.IsUserNameExistingAsync(name, ct))
            throw new ConflictException($"Ya existe un usuario con el nombre '{name}'.");

        var shift = role == UserRole.Operator
            ? await ValidateOperatorDataAsync(request.Age!.Value, request.ShiftId!.Value, ct)
            : null;

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

    // ======================= USR-4 =======================

    public async Task<UserDetailDto> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await uow.Users.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(typeof(User), id);

        var name = ValidateName(request.Name);
        var specialty = ValidateRoleData(user.Role, request.Specialty, request.Age, request.ShiftId);

        // El nombre debe ser único, pero el propio usuario puede conservar el suyo.
        var sameName = await uow.Users.GetByUserNameAsync(name, ct);
        if (sameName is not null && sameName.Id != id)
            throw new ConflictException($"Ya existe un usuario con el nombre '{name}'.");

        switch (user.Role)
        {
            case UserRole.Technician:
                var technician = await uow.Technicians.GetByIdAsync(id, ct)
                    ?? throw new EntityNotFoundException(typeof(Technician), id);
                technician.Specialty = specialty!;
                break;

            case UserRole.Operator:
                var shift = await ValidateOperatorDataAsync(request.Age!.Value, request.ShiftId!.Value, ct);
                var op = await uow.Operators.GetByIdAsync(id, ct)
                    ?? throw new EntityNotFoundException(typeof(Operator), id);
                op.Age = request.Age!.Value;
                op.ShiftId = shift.Id;
                op.Shift = shift;
                break;
        }

        user.Name = name;
        await uow.SaveChangesAsync(ct);

        return await BuildDetailAsync(user, ct);
    }

    // ======================= USR-6 =======================

    public async Task<UserDetailDto> SetTechnicianLinesAsync(
        Guid id, SetTechnicianLinesRequest request, CancellationToken ct = default)
    {
        if (request?.Lines is null)
            throw new InvalidRequestException("Debe indicar la lista de líneas.");
        if (request.Lines.GroupBy(l => l.LineId).Any(g => g.Count() > 1))
            throw new InvalidRequestException("Hay líneas repetidas en la lista.");

        var wanted = request.Lines.ToDictionary(l => l.LineId, l => ParsePermission(l.Permission));

        var user = await uow.Users.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(typeof(User), id);
        if (user.Role != UserRole.Technician)
            throw new DomainException("El usuario no es técnico.");

        var technician = await uow.Technicians.GetWithLineAsync(id, ct)
            ?? throw new EntityNotFoundException(typeof(Technician), id);

        var lines = await LoadExistingLinesAsync(wanted.Keys, ct);

        // Se sincroniza la colección en lugar de borrar todo y volver a crear: así cambiar solo el
        // permiso de una línea es una actualización y no un borrado más un alta con la misma clave.
        foreach (var current in technician.Lines.ToList())
        {
            if (wanted.TryGetValue(current.LineId, out var permission))
                current.Permission = permission;
            else
                technician.Lines.Remove(current);
        }

        foreach (var (lineId, permission) in wanted)
        {
            if (technician.Lines.Any(l => l.LineId == lineId))
                continue;

            technician.Lines.Add(new TechnicianLine
            {
                TechnicianId = technician.Id,
                LineId = lineId,
                Permission = permission,
                Technician = technician,
                Line = lines[lineId]
            });
        }

        await uow.SaveChangesAsync(ct);

        return await BuildDetailAsync(user, ct);
    }

    // ======================= USR-7 =======================

    public async Task<UserDetailDto> SetOperatorLinesAsync(
        Guid id, SetOperatorLinesRequest request, CancellationToken ct = default)
    {
        if (request?.LineIds is null)
            throw new InvalidRequestException("Debe indicar la lista de líneas.");
        if (request.LineIds.Distinct().Count() != request.LineIds.Count)
            throw new InvalidRequestException("Hay líneas repetidas en la lista.");

        var user = await uow.Users.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(typeof(User), id);
        if (user.Role != UserRole.Operator)
            throw new DomainException("El usuario no es operario.");

        var op = await uow.Operators.GetWithShiftAndLinesAsync(id, ct)
            ?? throw new EntityNotFoundException(typeof(Operator), id);

        var lines = await LoadExistingLinesAsync(request.LineIds, ct);
        var wanted = request.LineIds.ToHashSet();

        foreach (var current in op.Lines.ToList())
        {
            if (!wanted.Contains(current.LineId))
                op.Lines.Remove(current);
        }

        foreach (var lineId in wanted)
        {
            if (op.Lines.Any(l => l.LineId == lineId))
                continue;

            op.Lines.Add(new OperatorLine
            {
                OperatorId = op.Id,
                LineId = lineId,
                Operator = op,
                Line = lines[lineId]
            });
        }

        await uow.SaveChangesAsync(ct);

        return await BuildDetailAsync(user, ct);
    }

    // ======================= auxiliares =======================

    private async Task<UserDetailDto> BuildDetailAsync(User user, CancellationToken ct)
    {
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

    /// <summary>Carga las líneas pedidas y falla si alguna no existe.</summary>
    private async Task<Dictionary<Guid, ProductionLine>> LoadExistingLinesAsync(
        IEnumerable<Guid> lineIds, CancellationToken ct)
    {
        var all = (await uow.Line.GetAllAsync(ct)).ToDictionary(l => l.Id);
        var result = new Dictionary<Guid, ProductionLine>();

        foreach (var lineId in lineIds)
        {
            if (!all.TryGetValue(lineId, out var line))
                throw new DomainException($"La línea '{lineId}' no existe.");
            result[lineId] = line;
        }

        return result;
    }

    private static string ValidateName(string? value)
    {
        var name = value?.Trim();
        if (string.IsNullOrEmpty(name))
            throw new InvalidRequestException("El nombre de usuario es obligatorio.");
        if (name.Length > MaxNameLength)
            throw new InvalidRequestException($"El nombre de usuario no puede superar {MaxNameLength} caracteres.");
        return name;
    }

    /// <summary>Comprueba que solo vengan los campos que corresponden al rol. Devuelve la especialidad normalizada.</summary>
    private static string? ValidateRoleData(UserRole role, string? rawSpecialty, int? age, Guid? shiftId)
    {
        var specialty = rawSpecialty?.Trim();

        switch (role)
        {
            case UserRole.Technician:
                if (string.IsNullOrEmpty(specialty))
                    throw new InvalidRequestException("La especialidad es obligatoria para un técnico.");
                if (specialty.Length > MaxSpecialtyLength)
                    throw new InvalidRequestException($"La especialidad no puede superar {MaxSpecialtyLength} caracteres.");
                if (age is not null || shiftId is not null)
                    throw new InvalidRequestException("La edad y el turno solo aplican a operarios.");
                break;

            case UserRole.Operator:
                if (age is null || shiftId is null)
                    throw new InvalidRequestException("La edad y el turno son obligatorios para un operario.");
                if (!string.IsNullOrEmpty(specialty))
                    throw new InvalidRequestException("La especialidad solo aplica a técnicos.");
                break;

            default:
                if (!string.IsNullOrEmpty(specialty) || age is not null || shiftId is not null)
                    throw new InvalidRequestException(
                        "La especialidad, la edad y el turno no aplican a este rol.");
                break;
        }

        return specialty;
    }

    /// <summary>Reglas de negocio de un operario (edad mínima y turno existente). Devuelve el turno.</summary>
    private async Task<Shift> ValidateOperatorDataAsync(int age, Guid shiftId, CancellationToken ct)
    {
        if (age < BusinessRules.MinimumOperatorAge)
            throw new DomainException($"Un operario debe tener al menos {BusinessRules.MinimumOperatorAge} años.");

        return await uow.Shifts.GetByIdAsync(shiftId, ct)
            ?? throw new DomainException("El turno indicado no existe.");
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

    private static TechnicianPermission ParsePermission(string? value)
    {
        var text = value?.Trim();

        if (string.IsNullOrEmpty(text)
            || int.TryParse(text, out _)
            || !Enum.TryParse<TechnicianPermission>(text, ignoreCase: true, out var permission)
            || !Enum.IsDefined(permission))
        {
            throw new InvalidRequestException("Permiso inválido. Valores permitidos: Generate, Validate.");
        }

        return permission;
    }
}
