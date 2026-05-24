using System.Reflection;
using Terminal.Gui.Cli;
using Terminal.Gui.Cli.ExampleApp;

CliHost host = new (options =>
{
    options.ApplicationName = "example-app";
    options.Version = "1.0.0";
    options.AgentGuide = "Terminal.Gui.Cli.ExampleApp.agent-guide.md";
    options.AgentGuideIsResource = true;
    options.ResourceAssembly = Assembly.GetExecutingAssembly ();
});

host.Registry.Register (new GreetCommand ());
host.Registry.Register (new InfoCommand ());

return await host.RunAsync (args);
