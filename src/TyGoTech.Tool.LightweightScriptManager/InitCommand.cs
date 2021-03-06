namespace TyGoTech.Tool.LightweightScriptManager;

using System.IO;

public class InitCommand : CommandExt
{
    private const string CommandName = "init";

    private const string CommandDescription = "Initialize runtime config for repository.";

    private static readonly IReadOnlyList<Option> CommandOptions = new Option[]
    {
        new Option(
            new[] { "--force", "-f" },
            "Overwrites the current runtime config file if one exists."),
        new Option<Uri>(
            new[] { "--package-uri", "-p" },
            () => Constants.DefaultResourcesUri,
            "The base URI that hosts the config files."),
    };

    private static readonly ResourceMap RuntimeConfigMap = new(Constants.RuntimeConfigFileName);

    public InitCommand()
        : base(
            CommandName,
            CommandDescription,
            CommandOptions,
            CommandHandler.Create<bool, Uri>(ExecuteAsync))
    {
    }

    public static async Task ExecuteAsync(bool force, Uri packageUri)
    {
        var repo = new DirectoryInfo("./");
        var runtimeConfig = RuntimeConfigMap.GetLocal(repo);
        if (runtimeConfig.Exists && !force)
        {
            throw new InvalidOperationException(
                $"The runtime config file {runtimeConfig} already exists (use the --force Luke).");
        }

        using var downloader = new ResourceDownloader(packageUri, repo);
        await downloader.DownloadAsync(RuntimeConfigMap);

        var settings = await repo.DeserializeConfigAsync();
        settings.PackageUri = packageUri;
        await settings.SerializeConfigAsync(repo);
    }
}