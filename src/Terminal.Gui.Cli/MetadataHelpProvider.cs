using System.Text;

namespace Terminal.Gui.Cli;

/// <summary>Generates help text from registry metadata.</summary>
public sealed class MetadataHelpProvider : IHelpProvider
{
    /// <inheritdoc />
    public string? GetRootHelp (ICommandRegistry registry)
    {
        ArgumentNullException.ThrowIfNull (registry);

        StringBuilder builder = new ();
        builder.AppendLine ("Commands:");

        foreach (ICliCommand command in registry.All)
        {
            builder.AppendLine ($"  {command.PrimaryAlias}\t{command.Description}");
        }

        builder.AppendLine ();
        builder.AppendLine ("Framework options:");
        builder.AppendLine ("  --help, -h");
        builder.AppendLine ("  --version");
        builder.AppendLine ("  --opencli");
        builder.AppendLine ("  --json");
        builder.AppendLine ("  --initial <value>");
        builder.AppendLine ("  --title, --prompt <value>");
        builder.AppendLine ("  --timeout <duration>");
        builder.AppendLine ("  --cat");

        return builder.ToString ();
    }

    /// <inheritdoc />
    public string? GetCommandHelp (ICliCommand command)
    {
        ArgumentNullException.ThrowIfNull (command);

        StringBuilder builder = new ();
        builder.AppendLine ($"# {command.PrimaryAlias}");
        builder.AppendLine (command.Description);

        if (command.Options.Count > 0)
        {
            builder.AppendLine ();
            builder.AppendLine ("Options:");

            foreach (CommandOptionDescriptor option in command.Options)
            {
                var shortName = option.ShortName is null ? string.Empty : $" -{option.ShortName},";
                builder.AppendLine ($" {shortName} --{option.Name}\t{option.Description}");
            }
        }

        return builder.ToString ();
    }
}
