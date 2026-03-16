using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;

namespace Nestify.Options;

internal sealed class NestifyOptions
{
    private const string CollectionPath = "Nestify";
    private const string AutoNestEnabledProperty = "AutoNestEnabled";

    private readonly WritableSettingsStore _store;

    public NestifyOptions(WritableSettingsStore store)
    {
        _store = store;
        if (!_store.CollectionExists(CollectionPath))
        {
            _store.CreateCollection(CollectionPath);
        }
    }

    public bool AutoNestEnabled
    {
        get => _store.PropertyExists(CollectionPath, AutoNestEnabledProperty)
            && _store.GetBoolean(CollectionPath, AutoNestEnabledProperty);
        set => _store.SetBoolean(CollectionPath, AutoNestEnabledProperty, value);
    }
}