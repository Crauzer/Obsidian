using System.Text.Json;

namespace Obsidian.Data;

public class Config
{
    #region Wad Hashtable Checksums
    public string GameHashesChecksum
    {
        get => this._gameHashesChecksum;
        set
        {
            this._gameHashesChecksum = value;
            Save();
        }
    }
    private string _gameHashesChecksum;

    public string LcuHashesChecksum
    {
        get => this._lcuHashesChecksum;
        set
        {
            this._lcuHashesChecksum = value;
            Save();
        }
    }
    private string _lcuHashesChecksum;
    #endregion

    #region Bin Hashtable Checksums
    public string BinFieldsHashesChecksum
    {
        get => this._binFieldsHashesChecksum;
        set
        {
            this._binFieldsHashesChecksum = value;
            Save();
        }
    }
    private string _binFieldsHashesChecksum;

    public string BinTypesHashesChecksum
    {
        get => this._binTypesHashesChecksum;
        set
        {
            this._binTypesHashesChecksum = value;
            Save();
        }
    }
    private string _binTypesHashesChecksum;

    public string BinHashesHashesChecksum
    {
        get => this._binHashesHashesChecksum;
        set
        {
            this._binHashesHashesChecksum = value;
            Save();
        }
    }
    private string _binHashesHashesChecksum;

    public string BinEntriesHashesChecksum
    {
        get => this._binEntriesHashesChecksum;
        set
        {
            this._binEntriesHashesChecksum = value;
            Save();
        }
    }
    private string _binEntriesHashesChecksum;
    #endregion

    public string GameDataDirectory 
    {
        get => this._gameDataDirectory;
        set
        {
            this._gameDataDirectory = value;
            Save();
        }
    }
    private string _gameDataDirectory;

    public string DefaultExtractDirectory
    {
        get => this._defaultExportDirectory;
        set
        {
            this._defaultExportDirectory = value;
            Save();
        }
    }
    private string _defaultExportDirectory;

    public bool SyncHashtables 
    {
        get => this._syncHashtables;
        set
        {
            this._syncHashtables = value;
            Save();
        }
    }
    private bool _syncHashtables = true;

    public bool LoadSkinnedMeshAnimations
    {
        get => this._loadSkinnedMeshAnimations;
        set
        {
            this._loadSkinnedMeshAnimations = value;
            Save();
        }
    }
    private bool _loadSkinnedMeshAnimations = false;

    private const string CONFIG_FILE = "config.json";

    public Config() { }

    public static Config Load()
    {
        if (File.Exists(CONFIG_FILE) is false)
            return new();

        using FileStream configStream = File.OpenRead(CONFIG_FILE);

        return JsonSerializer.Deserialize<Config>(configStream);
    }

    public void Save()
    {
        // this is an ugly as fuck hack but it works so idc
        try
        {
            File.WriteAllText(
                CONFIG_FILE,
                JsonSerializer.Serialize(
                    this,
                    new JsonSerializerOptions() { AllowTrailingCommas = true, WriteIndented = true }
                )
            );
        }
        catch (Exception) { }
    }
}
