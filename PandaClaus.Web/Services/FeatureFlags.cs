using System.Text.Json;

namespace PandaClaus.Web.Services;

public class FeatureFlags
{
    private readonly string _filePath;
    private readonly object _lock = new();
    private bool _addLetterLinkVisible;

    public FeatureFlags(IWebHostEnvironment env, IConfiguration configuration)
    {
        var storagePath = configuration["FeatureFlags:StoragePath"];
        if (string.IsNullOrWhiteSpace(storagePath))
        {
            if (!env.IsDevelopment())
                throw new InvalidOperationException("FeatureFlags:StoragePath must point to persistent storage outside the deployment directory.");
            storagePath = Path.Combine(env.ContentRootPath, "App_Data", "featureflags.json");
        }
        else
        {
            if (!Path.IsPathFullyQualified(storagePath))
                throw new InvalidOperationException("FeatureFlags:StoragePath must be an absolute path.");
            var relativeToRelease = Path.GetRelativePath(env.ContentRootPath, storagePath);
            if (relativeToRelease != ".." && !relativeToRelease.StartsWith(".." + Path.DirectorySeparatorChar) &&
                !Path.IsPathFullyQualified(relativeToRelease))
                throw new InvalidOperationException("FeatureFlags:StoragePath must be outside the deployment directory.");
        }

        _filePath = storagePath;
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);

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
