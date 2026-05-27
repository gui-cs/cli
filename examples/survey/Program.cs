using Terminal.Gui.App;
using Terminal.Gui.Cli.Survey;

Application.AppModel = AppModel.Inline;
return await SurveyApp.CreateHost ().RunAsync (args);
