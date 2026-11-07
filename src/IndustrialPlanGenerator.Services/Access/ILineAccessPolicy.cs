namespace IndustrialPlanGenerator.Services.Access;

/// <summary>
/// The single component that decides whether the session user can view a line, generate plans on it,
/// or validate them. Other services consult it instead of repeating these rules.
/// </summary>
public interface ILineAccessPolicy
{
    /// <summary>
    /// Can the user view the line? Administrator and plant manager, all lines; technician and operator,
    /// only those to which they are assigned.
    /// </summary>
    Task<bool> CanViewAsync(Guid lineId, CancellationToken ct = default);

    /// <summary>Can the user generate plans on the line? Only a technician with the <c>Generate</c> permission on it.</summary>
    Task<bool> CanGenerateAsync(Guid lineId, CancellationToken ct = default);

    /// <summary>
    /// Can the user validate plans on the line? The plant manager (all lines) or a technician with the
    /// <c>Validate</c> permission on it.
    /// </summary>
    Task<bool> CanValidateAsync(Guid lineId, CancellationToken ct = default);
}
