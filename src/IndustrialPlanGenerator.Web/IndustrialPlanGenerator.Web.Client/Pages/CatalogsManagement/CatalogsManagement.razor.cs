using System.Text.RegularExpressions;
using IndustrialPlanGenerator.Services.Contracts.Catalogs;
using IndustrialPlanGenerator.Web.Client.Services.LinesManagementService;
using Microsoft.AspNetCore.Components;

namespace IndustrialPlanGenerator.Web.Client.Pages.CatalogsManagement;

public partial class CatalogsManagement
{
    private enum CatalogKind { Shifts, MaintenanceTypes, Criticalities }

    private sealed record Notice(string Text, bool IsError);

    private sealed class ShiftForm
    {
        public Guid? Id { get; init; }
        public string Name { get; set; } = "";
        public string Schedule { get; set; } = "";
        public string? NameError { get; set; }
        public string? ScheduleError { get; set; }
        public string? FormError { get; set; }
        public bool IsNew => Id is null;
    }

    private static readonly Regex ScheduleFormat =
        new(@"^([01]\d|2[0-3]):[0-5]\d-([01]\d|2[0-3]):[0-5]\d$", RegexOptions.Compiled);

    private readonly CancellationTokenSource cts = new();

    private CatalogKind selected = CatalogKind.Shifts;
    private IReadOnlyList<ShiftDto> shifts = Array.Empty<ShiftDto>();
    private IReadOnlyList<string> maintenanceTypes = Array.Empty<string>();
    private IReadOnlyList<string> criticalities = Array.Empty<string>();

    private bool isLoading = true;
    private string? loadError;
    private bool isBusy;
    private Notice? notice;
    private ShiftForm? form;
    private ShiftDto? pendingDelete;

    protected override Task OnInitializedAsync() => LoadAsync();

    public void Dispose()
    {
        cts.Cancel();
        cts.Dispose();
    }

    private (CatalogKind Kind, string Title, string Subtitle, int Count)[] GetEntries() =>
    [
        (CatalogKind.Shifts, "Shifts", "Work schedules", shifts.Count),
        (CatalogKind.MaintenanceTypes, "Maintenance types", "Fixed values", maintenanceTypes.Count),
        (CatalogKind.Criticalities, "Criticality levels", "Fixed values", criticalities.Count),
    ];

    private void SelectCatalog(CatalogKind kind)
    {
        if (isBusy) return;
        selected = kind;
        form = null;
        notice = null;
    }

    private void DismissNotice() => notice = null;

    // ---- Loading ----------------------------------------------------------

    private async Task LoadAsync()
    {
        isLoading = true;
        loadError = null;
        try
        {
            var shiftsTask = Catalogs.LoadShiftsAsync(cts.Token);
            var typesTask = Catalogs.LoadMaintenanceTypesAsync(cts.Token);
            var criticalitiesTask = Catalogs.LoadCriticalitiesAsync(cts.Token);
            await Task.WhenAll(shiftsTask, typesTask, criticalitiesTask);

            shifts = await shiftsTask;
            maintenanceTypes = await typesTask;
            criticalities = await criticalitiesTask;
        }
        catch (OperationCanceledException)
        {
            // The page was left; nothing to show.
        }
        catch (HttpRequestException)
        {
            loadError = "The catalogs could not be loaded.";
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task ReloadShiftsAsync()
    {
        try
        {
            shifts = await Catalogs.LoadShiftsAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
        }
        catch (HttpRequestException)
        {
            notice = new Notice("The list of shifts could not be refreshed.", true);
        }
    }

    // ---- Create / edit ----------------------------------------------------

    private void StartCreate()
    {
        notice = null;
        form = new ShiftForm();
    }

    private void StartEdit(ShiftDto shift)
    {
        notice = null;
        form = new ShiftForm { Id = shift.Id, Name = shift.Name, Schedule = shift.Schedule };
    }

    private void CancelForm() => form = null;

    private bool Validate(ShiftForm current)
    {
        current.NameError = null;
        current.ScheduleError = null;

        if (current.Name.Length == 0)
        {
            current.NameError = "Name is required.";
        }
        else if (shifts.Any(s => s.Id != current.Id && string.Equals(s.Name, current.Name, StringComparison.OrdinalIgnoreCase)))
        {
            current.NameError = "A shift with this name already exists.";
        }

        if (current.Schedule.Length == 0)
        {
            current.ScheduleError = "Schedule is required.";
        }
        else if (!ScheduleFormat.IsMatch(current.Schedule))
        {
            current.ScheduleError = "Use the format HH:mm-HH:mm, for example 06:00-14:00.";
        }
        else if (current.Schedule[..5] == current.Schedule[6..])
        {
            current.ScheduleError = "Start and end times must be different.";
        }
        else if (shifts.Any(s => s.Id != current.Id && s.Schedule == current.Schedule))
        {
            current.ScheduleError = "A shift with this schedule already exists.";
        }

        return current.NameError is null && current.ScheduleError is null;
    }

    private async Task SaveShiftAsync()
    {
        var current = form;
        if (current is null || isBusy) return;

        current.Name = current.Name.Trim();
        current.Schedule = current.Schedule.Trim();
        current.FormError = null;
        if (!Validate(current)) return;

        isBusy = true;
        try
        {
            if (current.IsNew)
            {
                await Catalogs.CreateShiftAsync(new CreateShiftRequest(current.Name, current.Schedule), cts.Token);
                notice = new Notice($"Shift \"{current.Name}\" created.", false);
            }
            else
            {
                await Catalogs.UpdateShiftAsync(current.Id!.Value, new UpdateShiftRequest(current.Name, current.Schedule), cts.Token);
                notice = new Notice($"Shift \"{current.Name}\" updated.", false);
            }

            form = null;
            await ReloadShiftsAsync();
        }
        catch (OperationCanceledException)
        {
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            form = null;
            notice = new Notice("This shift no longer exists.", true);
            await ReloadShiftsAsync();
        }
        catch (HttpRequestException ex)
        {
            current.FormError = ex.StatusCode switch
            {
                System.Net.HttpStatusCode.BadRequest => "The data is not valid. Check the name and the schedule.",
                System.Net.HttpStatusCode.Conflict => "A shift with this name or schedule already exists.",
                _ => "The shift could not be saved. Try again."
            };
        }
        finally
        {
            isBusy = false;
        }
    }

    // ---- Delete -----------------------------------------------------------

    private async Task ConfirmDeleteAsync()
    {
        var shift = pendingDelete;
        if (shift is null || isBusy) return;

        isBusy = true;
        try
        {
            var result = await Catalogs.DeleteShiftAsync(shift.Id, cts.Token);
            pendingDelete = null;

            notice = result switch
            {
                DeleteResult.Deleted => new Notice($"Shift \"{shift.Name}\" deleted.", false),
                DeleteResult.NotFound => new Notice($"Shift \"{shift.Name}\" no longer exists.", false),
                DeleteResult.HasDependencies => new Notice($"Shift \"{shift.Name}\" cannot be deleted because it still has operators assigned.", true),
                _ => new Notice("The shift could not be deleted.", true)
            };

            if (result != DeleteResult.HasDependencies)
            {
                if (form?.Id == shift.Id) form = null;
                await ReloadShiftsAsync();
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (HttpRequestException)
        {
            pendingDelete = null;
            notice = new Notice("The shift could not be deleted. Try again.", true);
        }
        finally
        {
            isBusy = false;
        }
    }

    // ---- Read-only catalogs ----------------------------------------------
}