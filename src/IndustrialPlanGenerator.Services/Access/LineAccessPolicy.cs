using IndustrialPlanGenerator.Domain.Enums;
using IndustrialPlanGenerator.Domain.Persistence;
using IndustrialPlanGenerator.Services.Auth;

namespace IndustrialPlanGenerator.Services.Access;

/// <inheritdoc cref="ILineAccessPolicy"/>
public sealed class LineAccessPolicy(IUnitOfWork uow, ICurrentUser currentUser) : ILineAccessPolicy
{
    public async Task<bool> CanViewAsync(Guid lineId, CancellationToken ct = default)
    {
        switch (currentUser.Role)
        {
            case UserRole.Administrator:
            case UserRole.PlantManager:
                return true;

            case UserRole.Technician:
                var technician = await uow.Technicians.GetWithLineAsync(currentUser.Id, ct);
                return technician?.Lines.Any(l => l.LineId == lineId) ?? false;

            case UserRole.Operator:
                var op = await uow.Operators.GetWithShiftAndLinesAsync(currentUser.Id, ct);
                return op?.Lines.Any(l => l.LineId == lineId) ?? false;

            default:
                return false;
        }
    }

    public async Task<bool> CanGenerateAsync(Guid lineId, CancellationToken ct = default)
    {
        if (currentUser.Role != UserRole.Technician)
            return false;

        return await uow.Technicians.IsAuthorizedInLineAsync(
            currentUser.Id, lineId, TechnicianPermission.Generate, ct);
    }

    public async Task<bool> CanValidateAsync(Guid lineId, CancellationToken ct = default)
    {
        switch (currentUser.Role)
        {
            case UserRole.PlantManager:
                return true;

            case UserRole.Technician:
                return await uow.Technicians.IsAuthorizedInLineAsync(
                    currentUser.Id, lineId, TechnicianPermission.Validate, ct);

            default:
                return false;
        }
    }
}
