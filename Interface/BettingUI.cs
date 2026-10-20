using System;
using Spectre.Console;
using Spectre.Console.Rendering;

public static class BettingUI
{
    private static readonly int[] chipValues = { 5, 25, 100, 500 };
    private static readonly string[] chipColors = { "red", "green3", "deepskyblue1", "gold1" };
    public static int Balance { get; private set; } = Player.StartingChips;
    public static int CurrentBet { get; private set; }
    public static int SelectedChip { get; private set; }

    public static void LoadBalance(int balance)
    {
        Balance = Math.Max(0, balance);
        CurrentBet = 0;
        SelectedChip = 0;
    }

    public static IRenderable Create()
    {
        var chips = new Grid();
        for (int i = 0; i < chipValues.Length; i++) chips.AddColumn(new GridColumn().PadRight(2));
        IRenderable[] chipRow = new IRenderable[chipValues.Length];
        for (int i = 0; i < chipValues.Length; i++)
        {
            string color = chipColors[i];
            int value = chipValues[i];
            string chip = i == SelectedChip
                ? $"[bold white]❯[/][bold black on {color}] ◉ {value} ◉ [/][bold white]❮[/]"
                : $"[bold {color}] ◉ {value} ◉ [/]";
            chipRow[i] = new Markup(chip);
        }
        chips.AddRow(chipRow);
        return new Rows(
            new Align(new Markup("[bold gold1]♠ SPELMARKER ♠[/]"), HorizontalAlignment.Center),
            new Align(chips, HorizontalAlignment.Center));
    }

    public static IRenderable CreateInfoPanel()
    {
        return new Panel(new Rows(
            new Markup("[bold gold1]SPELMARKER[/]"),
            new Text(""),
            new Markup($"[white]Saldo:[/] [green1]{Balance}[/]"),
            new Markup($"[white]Nuvarande insats:[/] [gold1]{CurrentBet}[/]"),
            new Text(""),
            new Markup("[grey]← / →: Välj[/]"),
            new Markup("[grey]Enter: Lägg insats[/]"),
            new Markup("[grey]Esc: Tillbaka[/]")))
            .Border(BoxBorder.Rounded).BorderColor(Color.Gold1);
    }

    // Handles chip selection and places the bet on the given player.
    internal static bool HandleKey(ConsoleKey key, Player player)
    {
        switch (key)
        {
            case ConsoleKey.LeftArrow:
                SelectedChip = Math.Max(0, SelectedChip - 1);
                return true;
            case ConsoleKey.RightArrow:
                SelectedChip = Math.Min(chipValues.Length - 1, SelectedChip + 1);
                return true;
            case ConsoleKey.Enter:
                // The player checks that the bet is allowed (not zero, not more than the chips).
                player.TryPlaceBet(chipValues[SelectedChip]);
                return true;
            case ConsoleKey.Backspace:
                player.CancelBet();
                return true;
            default:
                return false;
        }
    }

    public static void Reset() => LoadBalance(Player.StartingChips);
}
