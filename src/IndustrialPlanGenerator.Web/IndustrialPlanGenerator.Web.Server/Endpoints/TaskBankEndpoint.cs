using Microsoft.AspNetCore.Http.HttpResults;
using IndustrialPlanGenerator.Web.Server.Auth;
using System.Security.Claims;
using IndustrialPlanGenerator.Services.Contracts.TaskBank;


public static class TaskBankEndpoints
{
    public static IEndpointRouteBuilder MapTaskBankEndpoints (this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");
        var tareas = api.MapGroup("/tareas");

        // TAR-1: GET /api/tareas [PDF]
        tareas.MapGet("/", GetTaskwithFilter)
        .WithName("GetTaskWithFilters")
        .WithSummary("Devuelve las tareas que cumplan con los filtros correspondientes.")
        .Produces<List<TaskDto>>(StatusCodes.Status200OK)
        .RequireAuthorization(policy => policy.RequireRole(Policies.Technician, Policies.PlantManager, Policies.Administrator));

        // // TAR-2: GET /api/tareas/{id}
        tareas.MapGet("/{id:guid}", GetTaskbyId)
        .WithName("GetTaskbyId")
        .WithSummary("Devuelve una tareas que posea el id correspondiente.")
        .Produces<TaskDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization(policy => policy.RequireRole(Policies.Technician, Policies.PlantManager, Policies.Administrator));

        // TAR-3: POST /api/tareas
        tareas.MapPost("/", CreateTask)
        .WithName("CreateTask")
        .WithSummary("Genera una tarea nueva")
        .Produces<TaskDto>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .RequireAuthorization(policy => policy.RequireRole(Policies.Technician));

        // TAR-4: PUT /api/tareas/{id}
        tareas.MapPut("/{id:guid}", ReclassifyTask)
        .WithName("ReclassifyTask")
        .WithSummary("Reclasifica una tarea de mantenimiento")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
        .RequireAuthorization(policy => policy.RequireRole(Policies.Technician));
        
        // TAR-5: GET /api/tareas/duplicados (se coloca antes de {id} para evitar colisión de rutas)
        tareas.MapGet("/duplicados", ViewDuplicates)
        .WithName("ViewDuplicatesTasks")
        .WithSummary("Ver tareas de mantenimiento duplicadas")
        .Produces<List<TaskDto>>(StatusCodes.Status200OK)
        .RequireAuthorization(policy => policy.RequireRole(Policies.Administrator));
        
        // TAR-6: PUT /api/tareas/{id}/marcar-duplicada         (Cambie el tipo de Post -> Put)
        tareas.MapPut("/{id:guid}/marcar-duplicada", MarkDuplicates)
        .WithName("MarkDuplicatesTasks")
        .WithSummary("Marcar tareas de mantenimiento duplicadas")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
        .RequireAuthorization(policy => policy.RequireRole(Policies.Administrator));

        //Cree este endpoint por si acaso >:3
        tareas.MapPut("/{id:guid}/desmarcar-duplicada", DisMarkDuplicates)
        .WithName("DisMarkDuplicatesTasks")
        .WithSummary("Desmarcar tareas de mantenimiento como duplicadas")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
        .RequireAuthorization(policy => policy.RequireRole(Policies.Administrator));

        return app;
    }


    /// <summary>
    /// Llama al servicio del banco de tareas para conseguir tareas por filtrado
    /// </summary>
    /// <param name="user"> Usuario (Necesario para separar la logica por rol)</param>
    /// <param name="filter"> Filtros</param>
    /// <param name="service"> Servicio del Banco de Tareas</param>
    /// <returns> Tareas Filtradas</returns>
    private static async Task<Ok<List<TaskDto>>> GetTaskwithFilter(ClaimsPrincipal user, TaskFilterDto filter, ITaskBankService service)
    {
        List<TaskDto> result = await service.FilterTasks(user, filter);
        return TypedResults.Ok(result);
    }


    /// <summary>
    /// Llama al Servicio del Banco de Tareas para obtener una Tarea por un Id
    /// </summary>
    /// <param name="user"> Usuario (Necesario para separar la logica por rol)</param>
    /// <param name="id"> Identificador de la Tarea</param>
    /// <param name="service"> Servicio del Banco de Tareas</param>
    /// <returns> Tarea de igual id, o null en caso de no existir</returns>
    private static async Task<Results<Ok<TaskDto>, NotFound>> GetTaskbyId(Guid id, ClaimsPrincipal user, ITaskBankService service)
    {
        var (result, notFound) = await service.GetTaskbyId(user, id);
        if(notFound)
        {
            return TypedResults.NotFound();
        }
        return TypedResults.Ok(result);
    }


    /// <summary>
    /// Llamada al servicio para la creacion de Tareas de Mantenimiento nuevas (en caso de haber una identica retorna la existente)
    /// </summary>
    /// <param name="info"> Informacion de la nueva tarea</param>
    /// <param name="service"> Servicio del Banco de Tareas</param>
    /// <returns> Tarea Creada o la existente</returns>
    private static async Task<Results<Created<TaskDto>, Conflict>> CreateTask(TaskCreateDto info, ITaskBankService service)
    {
        var (result, exist) = await service.CreateTask(info);
        if (exist)
        {
            return TypedResults.Conflict();
        }
        return TypedResults.Created("", result);
    }


    /// <summary>
    /// Llama al servicio para actualizar los parametros de una tarea que no este en un plan valido
    /// </summary>
    /// <param name="id"> Identificador de la tarea</param>
    /// <param name="user"> Usuario para verificar pertenecia de la tarea</param>
    /// <param name="updates"> Datos a actualizar</param>
    /// <param name="service"> Servicio del Banco de Tareas</param>
    /// <returns> Tarea con los parametros actualizados, o null en caso de estar en plan valido</returns>
    private static async Task<Results<Ok, UnprocessableEntity, NotFound>> ReclassifyTask(Guid id, ClaimsPrincipal user, TaskUpdateDto updates, ITaskBankService service)
    {
        var (invalidplan, notFound) = await service.ReclassifyTask(id, user, updates);
        if (invalidplan)
        {
            return TypedResults.UnprocessableEntity();
        }
        else if(notFound){
            return TypedResults.NotFound();
        }
        return TypedResults.Ok();
    }


    /// <summary>
    /// Obtener mediante el servicio las tareas duplicadas de un grupo de tareas activas
    /// </summary>
    /// <param name="service"> Servicio del Banco de Tareas</param>
    /// <returns> Tareas Duplicadas</returns>
    private static async Task<Ok<List<TaskDto>>> ViewDuplicates(ITaskBankService service)
    {
        List<TaskDto> result = await service.GetDuplicatesTasks();
        return TypedResults.Ok(result);
    }


    /// <summary>
    /// Marcar tarea de mantenimiento como duplicada
    /// </summary>
    /// <param name="id"> id de la tarea duplicada</param>
    /// <param name="service"> Servicio del Banco de Tareas</param>
    /// <returns> Respuesta Ok</returns>
    private static async Task<Results<Ok, UnprocessableEntity, NotFound>> MarkDuplicates(Guid id, ITaskBankService service)
    {
        var (notFound, isAlreadyDuplicade) = await service.MarkDuplicateTasks(id);
        if (isAlreadyDuplicade)
        {
            return TypedResults.UnprocessableEntity();
        }
        else if(notFound){
            return TypedResults.NotFound();
        }
        return TypedResults.Ok();
    }


    /// <summary>
    /// Desmarcar tarea de mantenimiento como duplicada
    /// </summary>
    /// <param name="id"> id de la tarea duplicada</param>
    /// <param name="service"> Servicio del Banco de Tareas</param>
    /// <returns> Respuesta Ok</returns>
    private static async Task<Results<Ok, UnprocessableEntity, NotFound>> DisMarkDuplicates(Guid id, ITaskBankService service)
    {
        var (notFound, isAlreadyDuplicade) = await service.DisMarkDuplicateTasks(id);
        if (isAlreadyDuplicade)
        {
            return TypedResults.UnprocessableEntity();
        }
        else if(notFound){
            return TypedResults.NotFound();
        }
        return TypedResults.Ok();
    }

}

