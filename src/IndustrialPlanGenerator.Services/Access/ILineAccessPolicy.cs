namespace IndustrialPlanGenerator.Services.Access;

/// <summary>
/// Única pieza que decide si el usuario de la sesión puede ver una línea, generar planes en ella
/// o validarlos. Los demás servicios la consultan en lugar de repetir estas reglas.
/// </summary>
public interface ILineAccessPolicy
{
    /// <summary>
    /// ¿Puede ver la línea? Administrador y jefe de planta, todas; técnico y operario, solo
    /// aquellas a las que están asignados.
    /// </summary>
    Task<bool> CanViewAsync(Guid lineId, CancellationToken ct = default);

    /// <summary>¿Puede generar planes en la línea? Solo un técnico con permiso <c>Generate</c> en ella.</summary>
    Task<bool> CanGenerateAsync(Guid lineId, CancellationToken ct = default);

    /// <summary>
    /// ¿Puede validar planes en la línea? El jefe de planta (todas las líneas) o un técnico con
    /// permiso <c>Validate</c> en ella.
    /// </summary>
    Task<bool> CanValidateAsync(Guid lineId, CancellationToken ct = default);
}
