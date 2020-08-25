using System.Collections.Generic;

namespace Obsidian.MVVM
{
    public interface ILocalizable
    {
        Dictionary<string, string> LocalizationMap { get; set; }
    }
}
