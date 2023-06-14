using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obsidian.Shared;

public partial class TreeView<TItem> {
    private string _style =>
        new StyleBuilder()
            .AddStyle("height", this.Height, !string.IsNullOrEmpty(this.Height))
            .AddStyle("max-height", this.MaxHeight, !string.IsNullOrEmpty(this.Height))
            .AddStyle("overflow-y", "scroll")
            .AddStyle(this.Style)
            .Build();

    [Parameter]
    public ICollection<TItem> ItemsFlat { get; set; }

    [Parameter]
    public RenderFragment<TItem> ItemTemplate { get; set; }

    [Parameter]
    public float ItemSize { get; set; } = 50f;

    [Parameter]
    public int OverscanCount { get; set; } = 5;

    [Parameter]
    public string Height { get; set; }

    [Parameter]
    public string MaxHeight { get; set; }

    [Parameter]
    public string Style { get; set; }

    [Parameter]
    public EventCallback<TItem> OnSelectedItemChanged { get; set; }

    private Task OnItemClick(MouseEventArgs e, TItem item) => this.OnSelectedItemChanged.InvokeAsync(item);
}