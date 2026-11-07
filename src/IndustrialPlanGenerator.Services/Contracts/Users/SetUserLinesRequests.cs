namespace IndustrialPlanGenerator.Services.Contracts.Users;

/// <summary>Lines of a technician (USR-6). Replaces all existing lines; an empty list removes all of them.</summary>
/// <param name="Lines">Pairs (line, permission). The permission is <c>Generate</c> or <c>Validate</c>; a line cannot be repeated.</param>
public sealed record SetTechnicianLinesRequest(IReadOnlyList<UserLineDto> Lines);

/// <summary>Lines of an operator (USR-7). Replaces all existing lines; an empty list removes all of them.</summary>
/// <param name="LineIds">Line IDs; duplicates are not allowed.</param>
public sealed record SetOperatorLinesRequest(IReadOnlyList<Guid> LineIds);
