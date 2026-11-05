using IndustrialPlanGenerator.Services.Contracts.Common;
using IndustrialPlanGenerator.Services.Contracts.Users;
using IndustrialPlanGenerator.Services.Users;
using IndustrialPlanGenerator.Web.Server.Auth;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace IndustrialPlanGenerator.Web.Server.Endpoints;

/// <summary>Endpoints de usuarios (USR-1 a USR-4 y USR-6 a USR-8).</summary>
public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");

        group.MapGet("/", List)
            .RequireAuthorization(Policies.Administrator)
            .WithName("ListUsers")
            .WithSummary("Lista usuarios con filtros y paginación (USR-1).")
            .WithDescription("Filtros opcionales: role (Administrator, Technician, Operator, PlantManager), " +
                             "lineId (técnicos u operarios con esa línea) y text (parte del nombre). " +
                             "page empieza en 1; pageSize por defecto 20 y máximo 100.")
            .Produces<PagedResult<UserSummaryDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/{id:guid}", Get)
            .RequireAuthorization()
            .WithName("GetUser")
            .WithSummary("Detalle de un usuario (USR-2).")
            .WithDescription("El administrador puede ver cualquier usuario; el resto de roles, solo el suyo.")
            .Produces<UserDetailDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .RequireAuthorization(Policies.Administrator)
            .WithName("CreateUser")
            .WithSummary("Crea un usuario y su registro de rol (USR-3).")
            .WithDescription("Especialidad solo para técnicos; edad (mínimo 18) y turno solo para operarios. " +
                             "El nombre de usuario debe ser único.")
            .Produces<UserDetailDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPut("/{id:guid}", Update)
            .RequireAuthorization(Policies.Administrator)
            .WithName("UpdateUser")
            .WithSummary("Edita el nombre y los datos propios del rol (USR-4).")
            .WithDescription("No cambia el rol ni la contraseña. Los campos deben corresponder al rol del usuario. " +
                             "El token ya emitido conserva el nombre anterior hasta que expire.")
            .Produces<UserDetailDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPut("/{id:guid}/technician-lines", SetTechnicianLines)
            .RequireAuthorization(Policies.Administrator)
            .WithName("SetTechnicianLines")
            .WithSummary("Reemplaza las líneas de un técnico con su permiso (USR-6).")
            .WithDescription("Permiso: Generate o Validate. Una línea solo puede aparecer una vez (un permiso por línea). " +
                             "Una lista vacía quita todas. No altera los planes ya generados.")
            .Produces<UserDetailDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPut("/{id:guid}/operator-lines", SetOperatorLines)
            .RequireAuthorization(Policies.Administrator)
            .WithName("SetOperatorLines")
            .WithSummary("Reemplaza las líneas de un operario (USR-7).")
            .WithDescription("Una lista vacía quita todas.")
            .Produces<UserDetailDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        // USR-8 cuelga de /api/lines, pero es un endpoint de usuarios (técnicos de una línea).
        app.MapGet("/api/lines/{lineId:guid}/technicians", ListLineTechnicians)
            .WithTags("Users")
            .RequireAuthorization()
            .WithName("ListLineTechnicians")
            .WithSummary("Técnicos autorizados en una línea (USR-8).")
            .WithDescription("Filtro opcional permission: Generate o Validate. Administrador y jefe de planta ven cualquier línea; " +
                             "técnicos y operarios, solo las suyas. Sirve para elegir técnico de apoyo en una reinspección.")
            .Produces<IReadOnlyList<LineTechnicianDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<Ok<PagedResult<UserSummaryDto>>> List(
        IUserService users,
        CancellationToken ct,
        [FromQuery] string? role = null,
        [FromQuery] Guid? lineId = null,
        [FromQuery] string? text = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = UserService.DefaultPageSize)
        => TypedResults.Ok(await users.ListAsync(role, lineId, text, page, pageSize, ct));

    private static async Task<Ok<UserDetailDto>> Get(Guid id, IUserService users, CancellationToken ct)
        => TypedResults.Ok(await users.GetAsync(id, ct));

    private static async Task<Created<UserDetailDto>> Create(
        [FromBody] CreateUserRequest request, IUserService users, CancellationToken ct)
    {
        var created = await users.CreateAsync(request, ct);
        return TypedResults.Created($"/api/users/{created.Id}", created);
    }

    private static async Task<Ok<UserDetailDto>> Update(
        Guid id, [FromBody] UpdateUserRequest request, IUserService users, CancellationToken ct)
        => TypedResults.Ok(await users.UpdateAsync(id, request, ct));

    private static async Task<Ok<UserDetailDto>> SetTechnicianLines(
        Guid id, [FromBody] SetTechnicianLinesRequest request, IUserService users, CancellationToken ct)
        => TypedResults.Ok(await users.SetTechnicianLinesAsync(id, request, ct));

    private static async Task<Ok<UserDetailDto>> SetOperatorLines(
        Guid id, [FromBody] SetOperatorLinesRequest request, IUserService users, CancellationToken ct)
        => TypedResults.Ok(await users.SetOperatorLinesAsync(id, request, ct));

    private static async Task<Ok<IReadOnlyList<LineTechnicianDto>>> ListLineTechnicians(
        Guid lineId, IUserService users, CancellationToken ct, [FromQuery] string? permission = null)
        => TypedResults.Ok(await users.ListLineTechniciansAsync(lineId, permission, ct));
}
