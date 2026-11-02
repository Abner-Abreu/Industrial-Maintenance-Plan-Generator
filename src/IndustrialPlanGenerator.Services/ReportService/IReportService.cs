using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
namespace IndustrialPlanGenerator.Services.Contracts.Reports;

public interface IReportService
{
    /// <summary>
    /// Reporte de los planes y parametros de una linea
    /// </summary>
    /// <param name="lineId"> id de la linea (opcional)</param>
    /// <param name="user"> usuario (Tecnico (sus lineas))</param>
    /// <returns> planes y parametros de la linea, junto a booleanos de verificacion: lineNotFound->No existe Lineaid == lineid, techNoAuth->(tecnico)usuario no responde a la linea=lineId</returns>
    Task<(List<ReportPlanParametersDto> report, bool lineidNotFound, bool techNoAuth)> GetGeneratedPlans(Guid lineId, ClaimsPrincipal user);

    
    /// <summary>
    /// Reporte de las tareas mas ejecutadas en una linea
    /// </summary>
    /// <param name="lineId"> id de la linea (opcional)</param>
    /// <param name="user"> usuario (Tecnico (sus lineas))</param>
    /// <returns> tareas mas ejecutadas en una linea, junto a booleanos de verificacion: lineNotFound->No existe Lineaid == lineid, techNoAuth->(tecnico)usuario no responde a la linea=lineId</returns>
    Task<(List<ReportTasksDto> report, bool lineidNotFound, bool techNoAuth)> GetMoreExecTasks(Guid lineId, ClaimsPrincipal user);


    /// <summary>
    /// Conseguir los planes validados por un revisor
    /// </summary>
    /// <param name="reviewerid"> id del revisor</param>
    /// <returns></returns>
    Task<(List<ReportPlanValidationDto> report, bool revieweridNotFound)> GetValidPlans(Guid reviewerid);


    /// <summary>
    /// Conseguir reporte de rendimiento de los trabajadores en un plan determinado
    /// </summary>
    /// <param name="planId"> id del plan</param>
    /// <param name="user"> usuario (Tecnico solo puede ver los de su linea)</param>
    /// <returns> Reporte de Rendimiento</returns>
    Task<(List<ReportOperatorPerformanceDto> report, bool lineidNotFound, bool techNoAuth )> GetPerformance(Guid planId, ClaimsPrincipal user);


    /// <summary>
    /// Conseguir comparativa de planes de varias lineas
    /// </summary>
    /// <param name="lineIds"> identificadores de las lineas a comparar</param>
    /// <param name="user"> usuario (Tecnico solo puede ver las de su lineas)</param>
    /// <returns> Reporte de la Comparacion entre planes</returns>
    Task<(List<ReportComparativaPlanDto> report, bool lineidNotFound, bool techNoAuth )> GetPlansinLines(List<Guid>lineIds, ClaimsPrincipal user);


    /// <summary>
    /// Conseguir correlacion de entre el rendimiento promedio de los operadores y la criticidad
    /// </summary>
    /// <param name="lineId"> identificadores de las lineas a comparar</param>
    /// <param name="user"> usuario (Tecnico solo puede ver las de su lineas)</param>
    /// <returns> Reporte de Correlacion</returns>
    Task<(List<ReportLineAvgPerfDto> report, bool lineidNotFound, bool techNoAuth )> GetCorrelation(Guid lineId, ClaimsPrincipal user);


    /// <summary>
    /// Obtener las tareas con mayor tasa de incumplimiento
    /// </summary>
    /// <param name="lineId"> identificadores de las lineas a comparar</param>
    /// <param name="user"> usuario (Tecnico solo puede ver las de su lineas)</param>
    /// <returns> Reporte de tareas con mayor tasa de incumplimiento</returns>
    Task<(List<ReportNonComplianceTasksDto> report, bool lineidNotFound, bool techNoAuth )> GetNonComplianceTasks(Guid lineId, ClaimsPrincipal user);

    /// <summary>
    /// Obtener Reporte de reinspeccion contra operadores del turno
    /// </summary>
    /// <param name="lineId"> identificadores de las lineas a comparar</param>
    /// <param name="user"> usuario (Tecnico solo puede ver las de su lineas)</param>
    /// <returns> Reporte de reinspeccion contra operadores del turno</returns>
    Task<(ReportReinspectionVsTurnDto report, bool lineidNotFound, bool techNoAuth )> GetReinspVsTurn(Guid lineId, ClaimsPrincipal user);


}