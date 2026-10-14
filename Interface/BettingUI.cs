
using System;
using Spectre.Console;
using Spectre.Console.Rendering;

public static class BettingUI
{
    // Values and colors of the casino chips.
    private static readonly int[] chipValues = { 5, 25, 100, 500 };

    private static readonly string[] chipColors =
    {
        "red",
        "green3",
        "deepskyblue1",
        "gold1"
    };

    // Temporary betting data for UI testing.
    public static int Balance { get; private set; } = 1000;
    public static int CurrentBet { get; private set; } = 0;
    public static int SelectedChip { get; private set; } = 0;

    // Draws casino chips without adding another border.
    public static IRenderable Create()
    {
        var chips = new Grid();

        for (int i = 0; i < chipValues.Length; i++)
        {
            chips.AddColumn(new GridColumn().PadRight(2));
        }

        IRenderable[] chipRow = new IRenderable[chipValues.Length];

        for (int i = 0; i < chipValues.Length; i++)
        {
            string color = chipColors[i];
            int value = chipValues[i];

            // Highlights the currently selected chip.
            string chip = i == SelectedChip
                ? $"[bold white]❯[/][bold black on {color}] ◉ {value} ◉ [/][bold white]❮[/]"
                : $"[bold {color}] ◉ {value} ◉ [/]";

            chipRow[i] = new Markup(chip);
        }

        chips.AddRow(chipRow);

        return new Rows(
            new Align(
                new Markup("[bold gold1]♠ SPELMARKER ♠[/]"),
                HorizontalAlignment.Center),

            new Align(
                chips,
                HorizontalAlignment.Center)
        );
    }

    // Shows betting information in the right panel.
    public static IRenderable CreateInfoPanel()
    {
        return new Panel(
            new Rows(
                new Markup("[bold gold1]SPELMARKER[/]"),
                new Text(""),
                new Markup(
                    $"[white]Saldo:[/] [green1]{Balance}[/]"),
                new Markup(
                    $"[white]Nuvarande insats:[/] [gold1]{CurrentBet}[/]"),
                new Text(""),
                new Markup("[grey]← / →: Välj[/]"),
                new Markup("[grey]Enter: Lägg insats[/]"),
                new Markup("[grey]Esc: Tillbaka[/]")
            ))
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Gold1);
    }

    // Handles chip selection and temporary betting.
    public static bool HandleKey(ConsoleKey key)
    {
        switch (key)
        {
            case ConsoleKey.LeftArrow:
                SelectedChip = Math.Max(0, SelectedChip - 1);
                return true;

            case ConsoleKey.RightArrow:
                SelectedChip = Math.Min(
                    chipValues.Length - 1,
                    SelectedChip + 1);
                return true;

            case ConsoleKey.Enter:
                int value = chipValues[SelectedChip];

                // Prevents betting more than the balance.
                if (value <= Balance)
                {
                    Balance -= value;
                    CurrentBet += value;
                }

                return true;

            default:
                return false;
        }
    }

    // Resets temporary values for UI testing.
    public static void Reset()
    {
        Balance = 1000;
        CurrentBet = 0;
        SelectedChip = 0;
    }
}
