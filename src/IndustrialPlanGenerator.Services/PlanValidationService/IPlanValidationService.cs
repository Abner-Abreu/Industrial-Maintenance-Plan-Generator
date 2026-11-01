using System.Security.Claims;
using IndustrialPlanGenerator.Services.Contracts.Validations;

public interface IPlanValidationService
{
    /// <summary>
    /// Generar una validacion para un plan de mantenimiento
    /// </summary>
    /// <param name="id"> Id del pan a validar
    /// <param name="user"> Usuario</param>
    /// <param name="info"> Informacion de la validacion</param>
    /// <returns>Retorna la informacion de la validacion, bool validateSelf: si uno valida su propio plan, bool noGeneratePlan: plan no generado, bool NonCriteria: criterios incumplidos, bool NonValidwithoutComments: rechazado sin comentarios</returns>
    Task<(ValidationDto result, bool validateSelf, bool noGeneratePlan, bool NonCriteria, bool NonValidwithoutComments)> ValidatePlan(Guid id, ClaimsPrincipal user, ValidCreateDto info);

    /// <summary>
    /// Obtener validaciones por filtro
    /// </summary>
    /// <param name="filter"> filtro con los parametros</param>
    /// <returns> Lista de validaciones que cumplen con los filtros</returns>
    Task<List<ValidationDto>> GetValidations(ValidFilterDto filter);

}