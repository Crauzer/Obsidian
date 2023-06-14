using Microsoft.AspNetCore.Components;
using MudBlazor;
using Obsidian.Services;
using Obsidian.Utils;

namespace Obsidian.Shared;

public partial class HashtableProvider : ComponentBase {
    [Inject]
    public ISnackbar Snackbar { get; set; }

    [Inject]
    public HashtableService Hashtable { get; set; }

    [Parameter]
    public EventCallback OnLoadingStart { get; set; }

    [Parameter]
    public EventCallback OnLoadingFinished { get; set; }

    protected override void OnInitialized() => base.OnInitialized();

    protected override async Task OnAfterRenderAsync(bool firstRender) {
        if (firstRender is false)
            return;

        await this.OnLoadingStart.InvokeAsync();
        try {
            await this.Hashtable.Initialize();
        } catch (Exception exception) {
            SnackbarUtils.ShowHardError(this.Snackbar, exception);
        } finally {
            await this.OnLoadingFinished.InvokeAsync();
        }
    }
}