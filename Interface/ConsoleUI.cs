using System;
using Spectre.Console;

public partial class ConsoleUi
{
    public void ShowMessage(string message)
    {
        AnsiConsole.MarkupLine(
            $"[white]{Markup.Escape(message)}[/]"
        );
    }

    public string AskMove()
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold gold1]Vad vill du göra?[/]")
                .HighlightStyle(
                    new Style(
                        foreground: Color.White,
                        background: new Color(0, 100, 50)
                    )
                )
                .AddChoices(
                    "♠ Dra kort",
                    "♥ Stanna"
                )
        );
    }

    public void ShowResult(string result)
    {
        var panel = new Panel(
            new Markup(
                $"[bold gold1]{Markup.Escape(result)}[/]"
            )
        )
        .Border(BoxBorder.Rounded)
        .BorderColor(Color.Gold1);

        AnsiConsole.Write(panel);
    }
}