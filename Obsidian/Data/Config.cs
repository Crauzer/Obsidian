using System.Text.Json;

namespace Obsidian.Data;

public class Config
{
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
