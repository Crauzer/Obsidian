using Microsoft.AspNetCore.Components;
using Obsidian.Data;
using Obsidian.Services;
using Octokit;

namespace Obsidian.Shared;

public partial class HashtableProvider : ComponentBase
{
    [Inject]
    public HashtableService Hashtable { get; set; }

    [Parameter]
    public EventCallback OnLoadingStart { get; set; }

    [Parameter]
    public EventCallback OnLoadingFinished { get; set; }

    protected override void OnInitialized() => base.OnInitialized();
    protected override async Task OnAfterRenderAsync(bool firstRender) 
    {
        if (firstRender is false)
            return;

        await this.OnLoadingStart.InvokeAsync();
        await this.Hashtable.Initialize();
        await this.OnLoadingFinished.InvokeAsync();
    }
}
