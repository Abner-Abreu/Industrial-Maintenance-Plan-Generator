using System.Net;
using System.Text.RegularExpressions;
using IndustrialPlanGenerator.Services.Contracts.Catalogs;
using IndustrialPlanGenerator.Web.Client.Services.CatalogsService;
using IndustrialPlanGenerator.Web.Client.Services.LinesManagementService;
using Microsoft.AspNetCore.Components;

namespace IndustrialPlanGenerator.Web.Client.Pages.CatalogsManagement;

/// <summary>
/// Code-behind of the catalogs management page (administrators only).
/// Shifts have full CRUD (CAT-1 to CAT-4); maintenance types and criticality levels are
/// fixed domain values shown read-only (CAT-5, CAT-6).
/// </summary>
public partial class CatalogsManagement : IDisposable
{
    /// <summary>The catalogs the user can switch between in the left-hand list.</summary>
    private enum CatalogKind
    {
        Shifts,
        MaintenanceTypes,
        Criticalities
    }

    /// <summary>Message shown above the catalog content after an action.</summary>
    /// <param name="Text">Message text.</param>
    /// <param name="IsError">True for failures (red, announced as an alert); false for confirmations.</param>
    private sealed record Notice(string Text, bool IsError);

    /// <summary>Editable state of the create/edit shift form, including its validation messages.</summary>
    private sealed class ShiftForm
    {
        /// <summary>Id of the shift being edited; null when creating a new one.</summary>
        public Guid? Id { get; init; }

        /// <summary>Current value of the name field.</summary>
        public string Name { get; set; } = "";

        /// <summary>Current value of the schedule field, for example "06:00-14:00".</summary>
        public string Schedule { get; set; } = "";

        /// <summary>Validation message shown under the name field, if any.</summary>
        public string? NameError { get; set; }

        /// <summary>Validation message shown under the schedule field, if any.</summary>
        public string? ScheduleError { get; set; }

        /// <summary>Error returned by the server on save (not tied to a single field), if any.</summary>
        public string? FormError { get; set; }

        /// <summary>True when the form creates a shift, false when it edits an existing one.</summary>
        public bool IsNew => Id is null;
    }

    /// <summary>Accepted schedule format: HH:mm-HH:mm in 24 hours (overnight ranges are allowed).</summary>
    private static readonly Regex ScheduleFormat =
        new(@"^([01]\d|2[0-3]):[0-5]\d-([01]\d|2[0-3]):[0-5]\d$", RegexOptions.Compiled);

    /// <summary>Cancelled on dispose so pending requests stop when the user leaves the page.</summary>
    private readonly CancellationTokenSource _cts = new();

    /// <summary>Data access for the catalogs (real HTTP service or the mock).</summary>
    [Inject]
    private ICatalogsService Catalogs { get; set; } = default!;

    /// <summary>Catalog currently shown in the right-hand panel.</summary>
    private CatalogKind _selected = CatalogKind.Shifts;

    /// <summary>Loaded shifts (CAT-1).</summary>
    private IReadOnlyList<ShiftDto> _shifts = Array.Empty<ShiftDto>();

    /// <summary>Loaded maintenance types, read-only (CAT-5).</summary>
    private IReadOnlyList<string> _maintenanceTypes = Array.Empty<string>();

    /// <summary>Loaded criticality levels, read-only (CAT-6).</summary>
    private IReadOnlyList<string> _criticalities = Array.Empty<string>();

    /// <summary>True while the initial load (or a retry) is in progress.</summary>
    private bool _isLoading = true;

    /// <summary>Message shown when the initial load fails; null when it succeeded.</summary>
    private string? _loadError;

    /// <summary>True while a save or delete request is running; disables the actions to avoid double submits.</summary>
    private bool _isBusy;

    /// <summary>Result of the last action, or null when there is nothing to report.</summary>
    private Notice? _notice;

    /// <summary>The open create/edit form, or null when it is closed.</summary>
    private ShiftForm? _form;

    /// <summary>The shift waiting for delete confirmation, or null when no dialog is open.</summary>
    private ShiftDto? _pendingDelete;

    /// <inheritdoc />
    protected override Task OnInitializedAsync() => LoadAsync();

    /// <summary>Cancels any request still in flight when the page is left.</summary>
    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    /// <summary>Rows of the left-hand catalog list: kind, title, subtitle and number of values.</summary>
    private (CatalogKind Kind, string Title, string Subtitle, int Count)[] GetEntries() =>
    [
        (CatalogKind.Shifts, "Shifts", "Work schedules", _shifts.Count),
        (CatalogKind.MaintenanceTypes, "Maintenance types", "Fixed values", _maintenanceTypes.Count),
        (CatalogKind.Criticalities, "Criticality levels", "Fixed values", _criticalities.Count),
    ];

    /// <summary>Switches the right-hand panel to another catalog, discarding any open form or message.</summary>
    private void SelectCatalog(CatalogKind kind)
    {
        if (_isBusy) return;
        _selected = kind;
        _form = null;
        _notice = null;
    }

    /// <summary>Hides the current message.</summary>
    private void DismissNotice() => _notice = null;

    // ---- Loading ----------------------------------------------------------

    /// <summary>
    /// Loads the three catalogs in parallel. Also used by the "Retry" button.
    /// On failure, sets <see cref="_loadError"/> instead of throwing.
    /// </summary>
    private async Task LoadAsync()
    {
        _isLoading = true;
        _loadError = null;
        try
        {
            var shiftsTask = Catalogs.LoadShiftsAsync(_cts.Token);
            var typesTask = Catalogs.LoadMaintenanceTypesAsync(_cts.Token);
            var criticalitiesTask = Catalogs.LoadCriticalitiesAsync(_cts.Token);
            await Task.WhenAll(shiftsTask, typesTask, criticalitiesTask);

            _shifts = await shiftsTask;
            _maintenanceTypes = await typesTask;
            _criticalities = await criticalitiesTask;
        }
        catch (OperationCanceledException)
        {
            // The page was left; nothing to show.
        }
        catch (HttpRequestException)
        {
            _loadError = "The catalogs could not be loaded.";
        }
        finally
        {
            _isLoading = false;
        }
    }

    /// <summary>
    /// Refreshes only the shift list after a change. Failures are reported through
    /// <see cref="_notice"/> so callers do not need to handle them.
    /// </summary>
    private async Task ReloadShiftsAsync()
    {
        try
        {
            _shifts = await Catalogs.LoadShiftsAsync(_cts.Token);
        }
        catch (OperationCanceledException)
        {
            // The page was left; nothing to show.
        }
        catch (HttpRequestException)
        {
            _notice = new Notice("The list of shifts could not be refreshed.", true);
        }
    }

    // ---- Create / edit ----------------------------------------------------

    /// <summary>Opens an empty form to create a shift.</summary>
    private void StartCreate()
    {
        _notice = null;
        _form = new ShiftForm();
    }

    /// <summary>Opens the form pre-filled with the data of an existing shift.</summary>
    private void StartEdit(ShiftDto shift)
    {
        _notice = null;
        _form = new ShiftForm { Id = shift.Id, Name = shift.Name, Schedule = shift.Schedule };
    }

    /// <summary>Closes the form without saving.</summary>
    private void CancelForm() => _form = null;

    /// <summary>
    /// Validates the form before calling the service: required fields, schedule format,
    /// different start and end, and no duplicated name or schedule among the loaded shifts.
    /// The server repeats these checks (400 / 409); this only gives immediate feedback.
    /// </summary>
    /// <returns>True when the form has no errors.</returns>
    private bool Validate(ShiftForm current)
    {
        current.NameError = null;
        current.ScheduleError = null;

        if (current.Name.Length == 0)
        {
            current.NameError = "Name is required.";
        }
        else if (_shifts.Any(s => s.Id != current.Id && string.Equals(s.Name, current.Name, StringComparison.OrdinalIgnoreCase)))
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
        else if (_shifts.Any(s => s.Id != current.Id && s.Schedule == current.Schedule))
        {
            current.ScheduleError = "A shift with this schedule already exists.";
        }

        return current.NameError is null && current.ScheduleError is null;
    }

    /// <summary>
    /// Saves the open form: creates the shift (CAT-2) or updates it (CAT-3), then refreshes the list.
    /// Server errors are shown inside the form: 400 invalid data, 409 duplicated name or schedule;
    /// a 404 closes the form because the shift was deleted by someone else.
    /// </summary>
    private async Task SaveShiftAsync()
    {
        var current = _form;
        if (current is null || _isBusy) return;

        current.Name = current.Name.Trim();
        current.Schedule = current.Schedule.Trim();
        current.FormError = null;
        if (!Validate(current)) return;

        _isBusy = true;
        try
        {
            if (current.IsNew)
            {
                await Catalogs.CreateShiftAsync(new CreateShiftRequest(current.Name, current.Schedule), _cts.Token);
                _notice = new Notice($"Shift \"{current.Name}\" created.", false);
            }
            else
            {
                await Catalogs.UpdateShiftAsync(current.Id!.Value, new UpdateShiftRequest(current.Name, current.Schedule), _cts.Token);
                _notice = new Notice($"Shift \"{current.Name}\" updated.", false);
            }

            _form = null;
            await ReloadShiftsAsync();
        }
        catch (OperationCanceledException)
        {
            // The page was left; nothing to show.
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _form = null;
            _notice = new Notice("This shift no longer exists.", true);
            await ReloadShiftsAsync();
        }
        catch (HttpRequestException ex)
        {
            current.FormError = ex.StatusCode switch
            {
                HttpStatusCode.BadRequest => "The data is not valid. Check the name and the schedule.",
                HttpStatusCode.Conflict => "A shift with this name or schedule already exists.",
                _ => "The shift could not be saved. Try again."
            };
        }
        finally
        {
            _isBusy = false;
        }
    }

    // ---- Delete -----------------------------------------------------------

    /// <summary>
    /// Deletes the shift waiting in <see cref="_pendingDelete"/> (CAT-4) and reports the outcome.
    /// A shift that still has operators is not deleted and an error message explains why.
    /// </summary>
    private async Task ConfirmDeleteAsync()
    {
        var shift = _pendingDelete;
        if (shift is null || _isBusy) return;

        _isBusy = true;
        try
        {
            var result = await Catalogs.DeleteShiftAsync(shift.Id, _cts.Token);
            _pendingDelete = null;

            _notice = result switch
            {
                DeleteResult.Deleted => new Notice($"Shift \"{shift.Name}\" deleted.", false),
                DeleteResult.NotFound => new Notice($"Shift \"{shift.Name}\" no longer exists.", false),
                DeleteResult.HasDependencies => new Notice($"Shift \"{shift.Name}\" cannot be deleted because it still has operators assigned.", true),
                _ => new Notice("The shift could not be deleted.", true)
            };

            if (result != DeleteResult.HasDependencies)
            {
                // The shift is gone: close the form if it was editing it, and refresh the list.
                if (_form?.Id == shift.Id) _form = null;
                await ReloadShiftsAsync();
            }
        }
        catch (OperationCanceledException)
        {
            // The page was left; nothing to show.
        }
        catch (HttpRequestException)
        {
            _pendingDelete = null;
            _notice = new Notice("The shift could not be deleted. Try again.", true);
        }
        finally
        {
            _isBusy = false;
        }
    }
}