using System.Text;

namespace Terminal.Gui.Cli;

/// <summary>Generates an OpenCLI JSON document from registry metadata.</summary>
public static class OpenCliWriter
{
    /// <summary>Generates OpenCLI JSON for the registered commands and framework options.</summary>
    public static string Generate (ICommandRegistry registry, CliHostOptions options)
    {
        ArgumentNullException.ThrowIfNull (registry);
        ArgumentNullException.ThrowIfNull (options);

        StringBuilder builder = new ();
        builder.Append ('{');
        AppendProperty (builder, "name", options.ApplicationName);
        builder.Append (',');
        AppendProperty (builder, "version", options.Version ?? "0.0.0");
        builder.Append (",\"commands\":[");

        var firstCommand = true;

        foreach (ICliCommand command in registry.All)
        {
            if (!firstCommand)
            {
                builder.Append (',');
            }

            firstCommand = false;
            AppendCommand (builder, command);
        }

        builder.Append ("],\"frameworkOptions\":[");
        string[] frameworkOptions =
        [
            "help", "version", "opencli", "initial", "title", "json", "timeout", "fullscreen", "cat", "output", "rows"
        ];

        for (var i = 0; i < frameworkOptions.Length; i++)
        {
            if (i > 0)
            {
                builder.Append (',');
            }

            builder.Append ('\"');
            builder.Append (Escape (frameworkOptions[i]));
            builder.Append ('\"');
        }

        builder.Append ("]}");
        return builder.ToString ();
    }

    private static void AppendCommand (StringBuilder builder, ICliCommand command)
    {
        builder.Append ('{');
        AppendProperty (builder, "alias", command.PrimaryAlias);
        builder.Append (',');
        AppendProperty (builder, "description", command.Description);
        builder.Append (',');
        AppendProperty (builder, "kind", command.Kind.ToString ().ToLowerInvariant ());
        builder.Append (',');
        AppendProperty (builder, "resultType", TypeNames.WireName (command.ResultType));
        builder.Append (",\"aliases\":[");

        for (var i = 0; i < command.Aliases.Count; i++)
        {
            if (i > 0)
            {
                builder.Append (',');
            }

            builder.Append ('\"');
            builder.Append (Escape (command.Aliases[i]));
            builder.Append ('\"');
        }

        builder.Append ("],\"options\":[");

        for (var i = 0; i < command.Options.Count; i++)
        {
            if (i > 0)
            {
                builder.Append (',');
            }

            AppendOption (builder, command.Options[i]);
        }

        builder.Append ("]}");
    }

    private static void AppendOption (StringBuilder builder, CommandOptionDescriptor option)
    {
        builder.Append ('{');
        AppendProperty (builder, "name", option.Name);
        builder.Append (',');
        AppendProperty (builder, "description", option.Description);
        builder.Append (',');
        AppendProperty (builder, "type", TypeNames.WireName (option.ValueType));
        builder.Append (",\"required\":");
        builder.Append (option.Required ? "true" : "false");

        if (option.ShortName is not null)
        {
            builder.Append (',');
            AppendProperty (builder, "shortName", option.ShortName);
        }

        if (option.DefaultValue is not null)
        {
            builder.Append (',');
            AppendProperty (builder, "defaultValue", option.DefaultValue);
        }

        builder.Append ('}');
    }

    private static void AppendProperty (StringBuilder builder, string name, string value)
    {
        builder.Append ('\"');
        builder.Append (Escape (name));
        builder.Append ("\":\"");
        builder.Append (Escape (value));
        builder.Append ('\"');
    }

    private static string Escape (string value)
    {
        StringBuilder builder = new (value.Length);

        foreach (var c in value)
        {
            switch (c)
            {
                case '\\':
                    builder.Append ("\\\\");
                    break;
                case '\"':
                    builder.Append ("\\\"");
                    break;
                case '\n':
                    builder.Append ("\\n");
                    break;
                case '\r':
                    builder.Append ("\\r");
                    break;
                case '\t':
                    builder.Append ("\\t");
                    break;
                default:
                    if (char.IsControl (c))
                    {
                        builder.Append ("\\u");
                        builder.Append (((int)c).ToString ("x4"));
                    }
                    else
                    {
                        builder.Append (c);
                    }

                    break;
            }
        }

        return builder.ToString ();
    }
}
