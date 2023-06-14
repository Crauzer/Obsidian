using Microsoft.Build.Construction;
using Microsoft.Build.Tasks;
using NuGet.Versioning;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Discord;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Git;
using Nuke.Common.Tools.GitHub;
using Nuke.Common.Tools.GitReleaseManager;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MinVer;
using Nuke.Common.Utilities.Collections;
using Octokit;
using Serilog;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.CompressionTasks;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[GitHubActions(
    "ci",
    GitHubActionsImage.WindowsLatest,
    EnableGitHubToken = true,
    AutoGenerate = true,
    FetchDepth = 0,
    OnPullRequestBranches = new[] { "main" },
    OnPushTags = new[]
    {
        "\"[0-9]+.[0-9]+.[0-9]+\"",
        "\"[0-9]+.[0-9]+.[0-9]+-rc.[0-9]+\"",
        "\"[0-9]+.[0-9]+.[0-9]+-beta.[0-9]+\""
    },
    InvokedTargets = new[] { nameof(Release) }
)]
class Build : NukeBuild {
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => x.Release);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild
        ? Configuration.Debug
        : Configuration.Release;

    [Solution(GenerateProjects = true)]
    readonly Solution Solution;

    [MinVer]
    readonly MinVer MinVer;

    [GitRepository]
    readonly GitRepository GitRepository;

    GitHubActions GitHubActions => GitHubActions.Instance;

    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    AbsolutePath ObsidianPublishDirectory => ArtifactsDirectory / "Obsidian";

    Target Clean =>
        _ =>
            _.Description("Clean")
                .Before(Restore)
                .Executes(() => {
                    DotNetClean(s => s.SetProject(Solution));

                    EnsureCleanDirectory(ArtifactsDirectory);
                });

    Target Restore =>
        _ =>
            _.Description("Restore")
                .DependsOn(Clean)
                .Executes(() => {
                    DotNetRestore(s => s.SetProjectFile(Solution));
                });

    Target Compile =>
        _ =>
            _.Description("Build")
                .DependsOn(Restore)
                .Executes(() => {
                    DotNetBuild(
                        s =>
                            s.SetProjectFile(Solution.GetProject("Obsidian"))
                                .SetConfiguration(Configuration)
                                .SetVersion(MinVer.Version)
                                .SetAssemblyVersion(MinVer.AssemblyVersion)
                                .SetFileVersion(MinVer.FileVersion)
                                .EnableNoRestore()
                    );
                });

    Target Test =>
        _ =>
            _.DependsOn(Compile)
                .Executes(() => {
                    DotNetTest(
                        s =>
                            s.SetProjectFile(Solution.GetProject("Obsidian.Tests"))
                                .SetConfiguration(Configuration)
                                .EnableNoRestore()
                    );
                });

    Target Publish =>
        _ =>
            _.DependsOn(Test)
                .Requires(() => Configuration.Equals(Configuration.Release))
                .Executes(() => {
                    // --nobuild makes it crash
                    DotNetPublish(
                        s =>
                            s.SetProject(Solution.GetProject("Obsidian"))
                                .SetOutput(ObsidianPublishDirectory)
                                .SetConfiguration(Configuration)
                                .EnableSelfContained()
                                .EnablePublishSingleFile()
                                .EnableNoRestore()
                                .SetVersion(MinVer.Version)
                                .SetAssemblyVersion(MinVer.AssemblyVersion)
                                .SetFileVersion(MinVer.FileVersion)
                    );
                });

    Target Release =>
        _ =>
            _.DependsOn(Publish)
                .Description("Release")
                .Requires(() => Configuration.Equals(Configuration.Release))
                .OnlyWhenStatic(() => GitRepository.Tags.Any())
                .Executes(async () => {
                    GitHubTasks.GitHubClient = new(new ProductHeaderValue(nameof(NukeBuild))) {
                        Credentials = new Credentials(GitHubActions.Token)
                    };

                    string owner = GitRepository.GetGitHubOwner();
                    string name = GitRepository.GetGitHubName();

                    Release release = await GitHubTasks.GitHubClient.Repository.Release.Get(
                        owner,
                        name,
                        MinVer.Version
                    );

                    Release createdRelease = await GitHubTasks.GitHubClient.Repository.Release.Edit(
                        owner,
                        name,
                        release.Id,
                        new() {
                            TagName = MinVer.Version,
                            TargetCommitish = GitHubActions.Sha,
                            Name = MinVer.Version,
                            Prerelease = !string.IsNullOrEmpty(MinVer.MinVerPreRelease)
                        }
                    );

                    string obsidianZip = ArtifactsDirectory / $"Obsidian_{MinVer.Version}.zip";

                    CompressZip(ObsidianPublishDirectory, obsidianZip);

                    await UploadReleaseAssetToGithub(createdRelease, obsidianZip);
                });

    private static async Task UploadReleaseAssetToGithub(Release release, string asset) {
        string assetFileName = Path.GetFileName(asset);

        ReleaseAssetUpload assetUpload =
            new() {
                FileName = assetFileName,
                ContentType = "application/octet-stream",
                RawData = File.OpenRead(asset),
            };

        await GitHubTasks.GitHubClient.Repository.Release.UploadAsset(release, assetUpload);
    }
}