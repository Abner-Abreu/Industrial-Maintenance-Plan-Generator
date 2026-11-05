namespace IndustrialPlanGenerator.Services.Contracts.Users;

/// <summary>Líneas de un técnico (USR-6). Reemplaza por completo las que tenía; una lista vacía las quita todas.</summary>
/// <param name="Lines">Pares (línea, permiso). El permiso es <c>Generate</c> o <c>Validate</c>; una línea no puede repetirse.</param>
public sealed record SetTechnicianLinesRequest(IReadOnlyList<UserLineDto> Lines);

/// <summary>Líneas de un operario (USR-7). Reemplaza por completo las que tenía; una lista vacía las quita todas.</summary>
/// <param name="LineIds">Ids de las líneas; no puede haber repetidos.</param>
public sealed record SetOperatorLinesRequest(IReadOnlyList<Guid> LineIds);
