using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using PandaClaus.Web.Services;

namespace PantaClaus.Web.Tests;

[TestClass]
public class FeatureFlagsPersistenceTests
{
    private string _testDir = null!;

    [TestInitialize]
    public void SetUp() => _testDir = Path.Combine(Path.GetTempPath(), "pandaclaus-featureflags-" + Guid.NewGuid());

    [TestCleanup]
    public void TearDown()
    {
        if (Directory.Exists(_testDir)) Directory.Delete(_testDir, recursive: true);
    }

    [TestMethod]
    public void VisibilitySurvivesDeployIntoNewContentRoot()
    {
        var storagePath = Path.Combine(_testDir, "persistent", "featureflags.json");
        var releaseA = Path.Combine(_testDir, "release-a");
        var releaseB = Path.Combine(_testDir, "release-b");
        Directory.CreateDirectory(releaseA);
        Directory.CreateDirectory(Path.Combine(releaseB, "App_Data"));
        File.WriteAllText(Path.Combine(releaseB, "App_Data", "featureflags.json"), "{\"AddLetterLinkVisible\":false}");
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["FeatureFlags:StoragePath"] = storagePath
        }).Build();

        var before = new FeatureFlags(new TestEnvironment(releaseA), configuration);
        before.AddLetterLinkVisible = true;
        var after = new FeatureFlags(new TestEnvironment(releaseB), configuration);

        Assert.IsTrue(after.AddLetterLinkVisible);
        Assert.IsTrue(JsonDocument.Parse(File.ReadAllText(storagePath)).RootElement.GetProperty("AddLetterLinkVisible").GetBoolean());
        after.AddLetterLinkVisible = false;
        Assert.IsFalse(new FeatureFlags(new TestEnvironment(releaseA), configuration).AddLetterLinkVisible);
    }

    [TestMethod]
    public void ProductionWithoutPersistentPathDoesNotSilentlyUseDeploymentDirectory()
    {
        var release = Path.Combine(_testDir, "release");
        Directory.CreateDirectory(release);
        var configuration = new ConfigurationBuilder().Build();

        Assert.ThrowsExactly<InvalidOperationException>(() => new FeatureFlags(new TestEnvironment(release), configuration));
    }

    [TestMethod]
    public void StoragePathInsideDeploymentDirectoryIsRejected()
    {
        var release = Path.Combine(_testDir, "release");
        Directory.CreateDirectory(release);
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["FeatureFlags:StoragePath"] = Path.Combine(release, "App_Data", "featureflags.json")
        }).Build();

        Assert.ThrowsExactly<InvalidOperationException>(() => new FeatureFlags(new TestEnvironment(release), configuration));
    }

    [TestMethod]
    public void DevelopmentCanStillReadLocalFeatureFlags()
    {
        var release = Path.Combine(_testDir, "release");
        Directory.CreateDirectory(Path.Combine(release, "App_Data"));
        File.WriteAllText(Path.Combine(release, "App_Data", "featureflags.json"), "{\"AddLetterLinkVisible\":true}");
        var env = new TestEnvironment(release) { EnvironmentName = "Development" };

        Assert.IsTrue(new FeatureFlags(env, new ConfigurationBuilder().Build()).AddLetterLinkVisible);
    }

    private sealed class TestEnvironment(string contentRootPath) : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Production";
        public string ApplicationName { get; set; } = "PandaClaus.Web.Tests";
        public string ContentRootPath { get; set; } = contentRootPath;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = contentRootPath;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}
