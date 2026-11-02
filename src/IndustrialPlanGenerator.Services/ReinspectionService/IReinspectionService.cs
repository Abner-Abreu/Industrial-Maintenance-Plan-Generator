using System.Security.Claims;
using IndustrialPlanGenerator.Services.Contracts.Reinspection;

public interface IReinspectionService
{
    /// <summary>
    /// Genera la reinspeccion
    /// </summary>
    /// <param name="data"> Informacion de la reinspeccion</param>
    /// <param name="user"> usuario para verificar asignacion</param>
    /// <returns> Arroja la reinspeccion y booleanos de verificacion: MyExec->Si la ejecucion pertenece al usuario, TechAuthorize->Si el tecnico posee autorizacion de reinspeccion</returns>
    Task<(ReinspectionDto reinspection, bool MyExec, bool TechAuthorize)> GenerateReinspection(ReinspectionCreateDto data, ClaimsPrincipal user);


    /// <summary>
    /// Obtener Lista de Reinspecciones por filtro
    /// </summary>
    /// <param name="user"> usuario para obtener su rol</param>
    /// <param name="filter"> datos de filtrado</param>
    /// <returns> Lista de Reinspecciones</returns>
    Task<ReinspectionListDto> GetReinspections(ClaimsPrincipal user, ReinspectionFilterDto filter);


    /// <summary>
    /// Obtener Reinspeccion por id
    /// </summary>
    /// <param name="id"> id de reinspeccion</param>
    /// <param name="user"> usuario para obtener su rol</param>
    /// <returns> Datos de la reinspeccion</returns>
    Task<(ReinspectionDto reinspection, bool notFound)> GetReinspectionsbyId(Guid id, ClaimsPrincipal user);


    /// <summary>
    /// Insertar respuesta de Reinspeccion en la solicitud de reinspeccion
    /// </summary>
    /// <param name="id"> id de la reinspeccion</param>
    /// <param name="user"> usuario, saber si es tecnico de apoyo, o en caso de no haber, autorizado en la linea</param>
    /// <param name="result"> Resultado de la reinspeccion</param>
    /// <returns> Boolean para saber si el usuario "NO" cumple con los criterios para la insercion del resultado</returns>
    Task<bool> PutReinspectionResult(Guid id, ClaimsPrincipal user, ReinspectionResultDto result);

}