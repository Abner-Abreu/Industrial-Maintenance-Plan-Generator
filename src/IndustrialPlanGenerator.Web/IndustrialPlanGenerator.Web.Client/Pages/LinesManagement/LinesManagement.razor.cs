using IndustrialPlanGenerator.Services.Contracts.Lines;
using IndustrialPlanGenerator.Services.Contracts.Systems;
using IndustrialPlanGenerator.Web.Client.Services.LinesManagementService;

// IMPORTANT: this namespace must be exactly the namespace of LinesManagement.razor
// (project root namespace + folder path of the file). If it differs, the compiler
// treats this as a different class and the page fails to build.
namespace IndustrialPlanGenerator.Web.Client.Pages.LinesManagement;

// Code-behind of LinesManagement.razor.
// The @page, @attribute, @implements and @inject directives stay in the markup file.
public partial class LinesManagement
{
    private readonly CancellationTokenSource _cts = new();

    private List<LineSummaryDto> _lines = [];
    private bool _loadingList = true;
    private string? _listError;

    private Guid? _selectedId;
    private LineDetailDto? _detail;
    private bool _loadingDetail;
    private string? _detailError;

    // Editing state: the line name, plus an editable draft for each system while in edit mode.
    private string _editName = string.Empty;
    private string _editProcess = string.Empty;
    private bool _saving;
    private string? _saveError;
    private bool _editingSystems;
    private readonly Dictionary<Guid, SystemDraft> _drafts = [];

    private sealed class SystemDraft
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    // Line creation state.
    private bool _addingLine;
    private bool _creatingLine;
    private string _newLineName = string.Empty;
    private string _newLineProcess = string.Empty;
    private string? _createLineError;

    // System creation state.
    private bool _addingSystem;
    private bool _creatingSystem;
    private string _newSystemName = string.Empty;
    private string _newSystemDescription = string.Empty;
    private string? _createSystemError;

    // System deletion state.
    private bool _deletingSystem;
    private string? _systemError;

    // Line deletion state.
    private bool _confirmingDelete;
    private bool _deleting;
    private string? _deleteError;

    // True when the line name or productive process differs from the saved values.
    private bool HasLineChanges =>
        _detail is not null
        && (_editName.Trim() != _detail.Name || _editProcess.Trim() != _detail.ProductiveProcess);

    // True while any operation that changes the details is running.
    private bool Busy => _saving || _deletingSystem || _creatingSystem || _creatingLine;

    // True when any system draft differs from the saved system.
    private bool HasSystemChanges =>
        _editingSystems
        && _detail is not null
        && _detail.Systems.Any(s => _drafts.TryGetValue(s.Id, out var draft) && IsChanged(s, draft));

    // True when anything in the details differs from what is saved: the line name or any system.
    private bool HasChanges =>
        _detail is not null && (HasLineChanges || HasSystemChanges);

    // System names cannot be blank.
    private bool DraftsAreValid =>
        !_editingSystems || _drafts.Values.All(d => !string.IsNullOrWhiteSpace(d.Name));

    // The global save needs at least one change, valid values, and no operation in progress.
    private bool CanSave =>
        !Busy
        && HasChanges
        && !string.IsNullOrWhiteSpace(_editName)
        && !string.IsNullOrWhiteSpace(_editProcess)
        && DraftsAreValid;

    private static bool IsChanged(EquipmentSystemDto system, SystemDraft draft) =>
        draft.Name.Trim() != system.Name || draft.Description.Trim() != system.Description;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _lines = [.. await LinesService.LoadLinesAsync(_cts.Token)];
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception)
        {
            _listError = "Could not load the lines.";
        }
        finally
        {
            _loadingList = false;
        }
    }

    private async Task SelectLineAsync(Guid id)
    {
        _selectedId = id;
        _detail = null;
        _detailError = null;
        _saveError = null;
        _saving = false;
        StopEditingSystems();
        StopAddingSystem();
        _creatingSystem = false;
        _deletingSystem = false;
        _systemError = null;
        _deleteError = null;
        _confirmingDelete = false;
        _loadingDetail = true;

        try
        {
            var detail = await LinesService.LoadLineAsync(id, _cts.Token);

            // Ignore the response if the user selected another line while this one was loading.
            if (_selectedId == id)
            {
                _detail = detail;
                _editName = detail.Name;
                _editProcess = detail.ProductiveProcess;
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception)
        {
            if (_selectedId == id)
            {
                _detailError = "Could not load the line details.";
            }
        }
        finally
        {
            if (_selectedId == id)
            {
                _loadingDetail = false;
            }
        }
    }

    // Global save: persists the line name (if changed) and every edited system.
    private async Task SaveChangesAsync()
    {
        if (!CanSave || _detail is null)
        {
            return;
        }

        var id = _detail.Id;

        _saving = true;
        _saveError = null;

        try
        {
            // 1. Line name and productive process.
            if (HasLineChanges)
            {
                var request = new UpdateLineRequest(_editName.Trim(), _editProcess.Trim());
                var updated = await LinesService.UpdateLineAsync(id, request, _cts.Token);

                UpdateListItem(updated);
                _detail = updated;
                _editName = updated.Name;
                _editProcess = updated.ProductiveProcess;
            }

            // 2. Edited systems, one request each. Each saved system is applied to the details right away,
            //    so if one fails, the ones already saved are not lost and the rest can be retried.
            if (_editingSystems)
            {
                foreach (var system in _detail.Systems.ToList())
                {
                    if (!_drafts.TryGetValue(system.Id, out var draft) || !IsChanged(system, draft))
                    {
                        continue;
                    }

                    var request = new UpdateSystemRequest(draft.Name.Trim(), draft.Description.Trim());
                    var updatedSystem = await LinesService.UpdateSystemAsync(system.Id, request, _cts.Token);

                    ReplaceSystem(updatedSystem);
                }

                StopEditingSystems();
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception)
        {
            _saveError = "Could not save the changes.";
        }
        finally
        {
            _saving = false;
        }
    }

    private void ToggleSystemsEditing()
    {
        if (_editingSystems)
        {
            StopEditingSystems();
            return;
        }

        if (_detail is null)
        {
            return;
        }

        _systemError = null;
        _drafts.Clear();

        foreach (var system in _detail.Systems)
        {
            _drafts[system.Id] = new SystemDraft { Name = system.Name, Description = system.Description };
        }

        _editingSystems = true;
    }

    private void StopEditingSystems()
    {
        _editingSystems = false;
        _drafts.Clear();
    }

    // Replaces a system in the detail panel with its saved version.
    private void ReplaceSystem(EquipmentSystemDto updated)
    {
        if (_detail is null)
        {
            return;
        }

        _detail = _detail with
        {
            Systems = [.. _detail.Systems.Select(s => s.Id == updated.Id ? updated : s)]
        };
    }

    private void StartAddingLine()
    {
        _newLineName = string.Empty;
        _newLineProcess = string.Empty;
        _createLineError = null;
        _addingLine = true;
    }

    private void StopAddingLine()
    {
        _addingLine = false;
        _newLineName = string.Empty;
        _newLineProcess = string.Empty;
        _createLineError = null;
    }

    // Creates a line, adds it to the list and selects it so its details are shown.
    private async Task CreateLineAsync()
    {
        if (Busy || string.IsNullOrWhiteSpace(_newLineName) || string.IsNullOrWhiteSpace(_newLineProcess))
        {
            return;
        }

        LineDetailDto? created = null;

        _creatingLine = true;
        _createLineError = null;

        try
        {
            var request = new CreateLineRequest(_newLineName.Trim(), _newLineProcess.Trim());
            created = await LinesService.CreateLineAsync(request, _cts.Token);

            _lines.Add(new LineSummaryDto(created.Id, created.Name, created.ProductiveProcess, created.Systems.Count));
            StopAddingLine();
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception)
        {
            _createLineError = "Could not add the line.";
        }
        finally
        {
            _creatingLine = false;
        }

        if (created is not null)
        {
            await SelectLineAsync(created.Id);
        }
    }

    private void StartAddingSystem()
    {
        _newSystemName = string.Empty;
        _newSystemDescription = string.Empty;
        _createSystemError = null;
        _systemError = null;
        _addingSystem = true;
    }

    private void StopAddingSystem()
    {
        _addingSystem = false;
        _newSystemName = string.Empty;
        _newSystemDescription = string.Empty;
        _createSystemError = null;
    }

    // Creates a system in the selected line through the systems API and adds it to the local list.
    private async Task CreateSystemAsync()
    {
        if (_detail is null || Busy || string.IsNullOrWhiteSpace(_newSystemName))
        {
            return;
        }

        var lineId = _detail.Id;

        _creatingSystem = true;
        _createSystemError = null;

        try
        {
            var request = new CreateSystemRequest(lineId, _newSystemName.Trim(), _newSystemDescription.Trim());
            var created = await LinesService.CreateSystemAsync(request, _cts.Token);

            AddSystemToLine(lineId, created);
            StopAddingSystem();
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception)
        {
            _createSystemError = "Could not add the system.";
        }
        finally
        {
            _creatingSystem = false;
        }
    }

    // Adds a system to the detail panel (if that line is still selected) and updates the list counter.
    private void AddSystemToLine(Guid lineId, EquipmentSystemDto created)
    {
        if (_selectedId == lineId && _detail is not null)
        {
            _detail = _detail with { Systems = [.. _detail.Systems, created] };
        }

        var index = _lines.FindIndex(l => l.Id == lineId);
        if (index >= 0)
        {
            _lines[index] = _lines[index] with { SystemCount = _lines[index].SystemCount + 1 };
        }
    }

    // Deletes a system through the systems API and, if it succeeds, removes it from the local list.
    private async Task DeleteSystemAsync(Guid systemId)
    {
        if (_detail is null || Busy)
        {
            return;
        }

        var lineId = _detail.Id;

        _deletingSystem = true;
        _systemError = null;

        try
        {
            var result = await LinesService.DeleteSystemAsync(systemId, _cts.Token);

            switch (result)
            {
                case DeleteResult.Deleted:
                case DeleteResult.NotFound: // Already deleted elsewhere: the goal is reached.
                    RemoveSystemFromLine(lineId, systemId);
                    break;

                case DeleteResult.HasDependencies:
                    if (_selectedId == lineId)
                    {
                        _systemError = "This system cannot be deleted because it still has associated tasks or sensors.";
                    }
                    break;
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception)
        {
            if (_selectedId == lineId)
            {
                _systemError = "Could not delete the system.";
            }
        }
        finally
        {
            if (_selectedId == lineId)
            {
                _deletingSystem = false;
            }
        }
    }

    // Removes a system from the detail panel (if that line is still selected) and updates the list counter.
    private void RemoveSystemFromLine(Guid lineId, Guid systemId)
    {
        if (_selectedId == lineId && _detail is not null)
        {
            _detail = _detail with { Systems = [.. _detail.Systems.Where(s => s.Id != systemId)] };
        }

        var index = _lines.FindIndex(l => l.Id == lineId);
        if (index >= 0)
        {
            _lines[index] = _lines[index] with { SystemCount = Math.Max(0, _lines[index].SystemCount - 1) };
        }
    }

    // Refreshes the summary shown in the list after a line has been updated.
    private void UpdateListItem(LineDetailDto updated)
    {
        var index = _lines.FindIndex(l => l.Id == updated.Id);
        if (index >= 0)
        {
            _lines[index] = _lines[index] with
            {
                Name = updated.Name,
                ProductiveProcess = updated.ProductiveProcess
            };
        }
    }

    private void ShowDeleteConfirmation()
    {
        _deleteError = null;
        _confirmingDelete = true;
    }

    private void CancelDelete()
    {
        if (!_deleting)
        {
            _confirmingDelete = false;
        }
    }

    private async Task DeleteLineAsync()
    {
        if (_detail is null || _deleting)
        {
            return;
        }

        var id = _detail.Id;

        _deleting = true;
        _deleteError = null;

        try
        {
            var result = await LinesService.DeleteLineAsync(id, _cts.Token);

            switch (result)
            {
                case DeleteResult.Deleted:
                case DeleteResult.NotFound: // Already deleted elsewhere: the goal is reached.
                    RemoveLine(id);
                    break;

                case DeleteResult.HasDependencies:
                    _deleteError = "This line cannot be deleted because it still has associated systems, plans, or assignments.";
                    break;
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception)
        {
            _deleteError = "Could not delete the line.";
        }
        finally
        {
            _deleting = false;
            _confirmingDelete = false;
        }
    }

    // Removes a line from the list and clears the detail panel if that line was selected.
    private void RemoveLine(Guid id)
    {
        _lines.RemoveAll(l => l.Id == id);

        if (_selectedId == id)
        {
            _selectedId = null;
            _detail = null;
            _editName = string.Empty;
            _editProcess = string.Empty;
            _saveError = null;
            _deleteError = null;
            StopEditingSystems();
            StopAddingSystem();
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}