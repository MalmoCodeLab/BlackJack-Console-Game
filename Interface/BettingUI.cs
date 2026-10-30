using System;
using Spectre.Console;
using Spectre.Console.Rendering;

public static class BettingUI
{
    private static readonly int[] chipValues = { Wallet.MinimumBet, 25, 100, 500 };
    private static readonly string[] chipColors = { "red", "green3", "deepskyblue1", "gold1" };
    public static int Balance { get; private set; } = Wallet.StartingChips;
    public static int CurrentBet { get; private set; }
    public static int SelectedChip { get; private set; }
    private const int MaxNameLength = 12;

    public static void LoadBalance(int balance)
    {
        Balance = Math.Max(0, balance);
        CurrentBet = 0;
        SelectedChip = 0;
    }

    // Keep the public overload without Player for existing callers.
    public static IRenderable Create() => Create(null);

    // Player may be internal in the project, so this overload is internal too.
    internal static IRenderable Create(Player? player)
    {
        var chips = new Grid();
        for (int i = 0; i < chipValues.Length; i++)
            chips.AddColumn(new GridColumn().PadRight(2));

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

        // Show the active player's actual chips/bet instead of a global total.
        string title = player is null
            ? "[bold gold1]♠ SPELMARKER ♠[/]"
            : $"[bold gold1]♠ SPELMARKER ♠[/] " +
              $"[white]{Markup.Escape(player.Name.Length > MaxNameLength ? player.Name[..(MaxNameLength - 1)] + "…" : player.Name)}[/] " +
              $"[green1]Saldo: {player.Chips}[/]  [gold1]Insats: {player.CurrentBet}[/]";

        return new Rows(
            new Align(new Markup(title), HorizontalAlignment.Center),
            new Align(chips, HorizontalAlignment.Center));
    }

    // Keep the original public method signature for compatibility.
    public static IRenderable CreateInfoPanel() => CreateInfoPanel(null);

    internal static IRenderable CreateInfoPanel(Player? player)
    {
        string balance = player is null ? Balance.ToString() : player.Chips.ToString();
        string currentBet = player is null ? CurrentBet.ToString() : player.CurrentBet.ToString();
        return new Panel(new Rows(
            new Markup("[bold gold1]SPELMARKER[/]"),
            new Text(""),
            new Markup($"[white]Saldo:[/] [green1]{balance}[/]"),
            new Markup($"[white]Nuvarande insats:[/] [gold1]{currentBet}[/]"),
            new Text(""),
            new Markup("[grey]← / →: Välj[/]"),
            new Markup("[grey]Enter: Lägg insats[/]"),
            new Markup("[grey]Mellanslag: Bekräfta[/]"),
            new Markup("[grey]Esc: Spelmeny[/]")))
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
                // The Player class validates that the bet is affordable.
                player.TryPlaceBet(chipValues[SelectedChip]);
                return true;
            case ConsoleKey.Backspace:
                player.CancelBet();
                return true;
            default:
                return false;
        }
    }

    public static void Reset() => LoadBalance(Wallet.StartingChips);
}
