using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Obsidian.MVVM
{
    public interface ILocalizable
    {
        ReadOnlyDictionary<string, string> LocalizationMap { get; }
    }
}
