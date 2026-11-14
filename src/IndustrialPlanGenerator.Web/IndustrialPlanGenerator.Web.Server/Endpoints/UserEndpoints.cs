using IndustrialPlanGenerator.Services.Contracts.Common;
using IndustrialPlanGenerator.Services.Contracts.Users;
using IndustrialPlanGenerator.Services.Users;
using IndustrialPlanGenerator.Web.Server.Auth;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace IndustrialPlanGenerator.Web.Server.Endpoints;

/// <summary>User endpoints (USR-1 to USR-8).</summary>
public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");

        group.MapGet("/", List)
            .RequireAuthorization(Policies.Administrator)
            .WithName("ListUsers")
            .WithSummary("Lists users with filters and pagination (USR-1).")
            .WithDescription("Optional filters: role (Administrator, Technician, Operator, PlantManager), " +
                             "lineId (technicians or operators with that line), and text (part of the name). " +
                             "page starts at 1; pageSize defaults to 20 and has a maximum of 100.")
            .Produces<PagedResult<UserSummaryDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/{id:guid}", Get)
            .RequireAuthorization()
            .WithName("GetUser")
            .WithSummary("User details (USR-2).")
            .WithDescription("Administrators can view any user; other roles can view only their own.")
            .Produces<UserDetailDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .RequireAuthorization(Policies.Administrator)
            .WithName("CreateUser")
            .WithSummary("Creates a user and its role record (USR-3).")
            .WithDescription("Specialty only for technicians; age (minimum 18) and shift only for operators. " +
                             "The user name must be unique.")
            .Produces<UserDetailDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPut("/{id:guid}", Update)
            .RequireAuthorization(Policies.Administrator)
            .WithName("UpdateUser")
            .WithSummary("Edits the name and the data specific to the role (USR-4).")
            .WithDescription("Does not change the role or password. The fields must correspond to the user's role. " +
                             "The already issued token keeps the previous name until it expires.")
            .Produces<UserDetailDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapDelete("/{id:guid}", Delete)
            .RequireAuthorization(Policies.Administrator)
            .WithName("DeleteUser")
            .WithSummary("Deletes a user (USR-5).")
            .WithDescription("Soft delete: the user and its role record stop appearing and can no longer sign in, " +
                             "but the history is kept. An administrator cannot delete themselves or the last administrator.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPut("/{id:guid}/technician-lines", SetTechnicianLines)
            .RequireAuthorization(Policies.Administrator)
            .WithName("SetTechnicianLines")
            .WithSummary("Replaces a technician's lines with their permission (USR-6).")
            .WithDescription("Permission: Generate or Validate. A line can appear only once (one permission per line). " +
                             "An empty list removes all of them. Does not alter already generated plans.")
            .Produces<UserDetailDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPut("/{id:guid}/operator-lines", SetOperatorLines)
            .RequireAuthorization(Policies.Administrator)
            .WithName("SetOperatorLines")
            .WithSummary("Replaces an operator's lines (USR-7).")
            .WithDescription("An empty list removes all of them.")
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
            .WithSummary("Technicians authorized on a line (USR-8).")
            .WithDescription("Optional permission filter: Generate or Validate. Administrator and plant manager can view any line; " +
                             "technicians and operators, only their own. Used to choose a support technician for a reinspection.")
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

    private static async Task<NoContent> Delete(Guid id, IUserService users, CancellationToken ct)
    {
        await users.DeleteAsync(id, ct);
        return TypedResults.NoContent();
    }

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
