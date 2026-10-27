// using IndustrialPlanGenerator.Web.Client.Pages;
using IndustrialPlanGenerator.Web.Server.Components;
using IndustrialPlanGenerator.Services;
using IndustrialPlanGenerator.Data.Postgres;
using IndustrialPlanGenerator.Data.Mongo;
using IndustrialPlanGenerator.Web.Server.Errors;
using IndustrialPlanGenerator.Web.Server.Auth;
using IndustrialPlanGenerator.Web.Server.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

// Register Data Layers
builder.Services.AddPostgresData(builder.Configuration);
builder.Services.AddMongoData(builder.Configuration);

builder.Services.AddApplicationServices();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();


// Para /api responde GlobalExceptionHandler; para las páginas Blazor, /Error.
app.UseExceptionHandler("/Error", createScopeForErrors: true);


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapAuthEndpoints();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(IndustrialPlanGenerator.Web.Client._Imports).Assembly);

app.Run();
