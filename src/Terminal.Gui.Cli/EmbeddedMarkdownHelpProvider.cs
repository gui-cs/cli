using System.Reflection;
using System.Text;

namespace Terminal.Gui.Cli;

/// <summary>Reads embedded markdown resources for root, command, and agent help.</summary>
public sealed class EmbeddedMarkdownHelpProvider : IHelpProvider
{
    private readonly Assembly _resourceAssembly;

    /// <summary>Creates a provider that reads markdown resources from <paramref name="resourceAssembly" />.</summary>
    public EmbeddedMarkdownHelpProvider (Assembly resourceAssembly)
    {
        _resourceAssembly = resourceAssembly ?? throw new ArgumentNullException (nameof (resourceAssembly));
    }

    /// <inheritdoc />
    public string? GetRootHelp (ICommandRegistry registry)
    {
        return GetMarkdownResource ("help.md");
    }

    /// <inheritdoc />
    public string? GetCommandHelp (ICliCommand command)
    {
        ArgumentNullException.ThrowIfNull (command);
        return GetMarkdownResource ($"{command.PrimaryAlias}.md");
    }

    /// <summary>Reads an embedded markdown resource by exact manifest resource name.</summary>
    public string? GetMarkdownResource (string resourceName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace (resourceName);

        using Stream? stream = _resourceAssembly.GetManifestResourceStream (resourceName);

        if (stream is null)
        {
            return null;
        }

        using var reader = new StreamReader (stream, Encoding.UTF8);
        return reader.ReadToEnd ();
    }
}
