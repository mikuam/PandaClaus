using System.Text.Json;

namespace PandaClaus.Web.Services;

public class FeatureFlags
{
    private readonly string _filePath;
    private readonly object _lock = new();
    private bool _addLetterLinkVisible;

    public FeatureFlags(IWebHostEnvironment env)
    {
        var dir = Path.Combine(env.ContentRootPath, "App_Data");
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "featureflags.json");

        if (File.Exists(_filePath))
        {
            try
            {
                _addLetterLinkVisible = JsonSerializer.Deserialize<State>(File.ReadAllText(_filePath))?.AddLetterLinkVisible ?? false;
            }
            catch (JsonException)
            {
            }
        }
    }

    public bool AddLetterLinkVisible
    {
        get { lock (_lock) { return _addLetterLinkVisible; } }
        set
        {
            lock (_lock)
            {
                _addLetterLinkVisible = value;
                File.WriteAllText(_filePath, JsonSerializer.Serialize(new State { AddLetterLinkVisible = value }));
            }
        }
    }

    private class State
    {
        public bool AddLetterLinkVisible { get; set; }
    }
}
