using System;
using System.Collections.Generic;
using Spectre.Console;
using Spectre.Console.Rendering;

public partial class ConsoleUi
{
    private const int ReshuffleStep = 5;

    // Lets the player change the four house rules and returns the new rule set.
    // Esc returns to the previous screen.
    public RuleSet EditRules(RuleSet current)
    {
        ArgumentNullException.ThrowIfNull(current);

        RuleSet rules = current;
        int selected = 0;
        AnsiConsole.Clear();
        RunScreen(
            () => BuildSettingsMenu(rules, selected),
            key =>
            {
                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selected = (selected + 3) % 4;
                        break;
                    case ConsoleKey.DownArrow:
                        selected = (selected + 1) % 4;
                        break;
                    case ConsoleKey.LeftArrow:
                        rules = ChangeRule(rules, selected, -1);
                        break;
                    case ConsoleKey.RightArrow:
                    case ConsoleKey.Enter:
                        rules = ChangeRule(rules, selected, 1);
                        break;
                    case ConsoleKey.Escape:
                        return true;
                }
                return false;
            });
        AnsiConsole.Clear();
        return rules;
    }

    private static RuleSet ChangeRule(RuleSet rules, int index, int direction)
    {
        switch (index)
        {
            case 0:
                return rules with { DealerHitsSoft17 = !rules.DealerHitsSoft17 };
            case 1:
                int decks = Math.Clamp(rules.DeckCount + direction, 1, 8);
                return rules with
                {
                    DeckCount = decks,
                    ReshuffleThreshold = Math.Min(rules.ReshuffleThreshold, decks * 52)
                };
            case 2:
                int maxThreshold = rules.DeckCount * 52;
                int threshold = rules.ReshuffleThreshold + direction * ReshuffleStep;
                return rules with
                {
                    ReshuffleThreshold = Math.Clamp(threshold, ReshuffleStep, maxThreshold)
                };
            default:
                return rules with
                {
                    NaturalBlackjackBeatsThreeCard21 = !rules.NaturalBlackjackBeatsThreeCard21
                };
        }
    }

    private IRenderable BuildSettingsMenu(RuleSet rules, int selected)
    {
        (string Name, string Value)[] items =
        {
            ("Givare vid mjuk 17", rules.DealerHitsSoft17 ? "Tar kort" : "Stannar"),
            ("Antal kortlekar", rules.DeckCount.ToString()),
            ("Omblandning", $"Under {rules.ReshuffleThreshold} kort kvar"),
            ("Naturlig Blackjack",
                rules.NaturalBlackjackBeatsThreeCard21 ? "Slår vanlig 21" : "Samma värde som vanlig 21")
        };

        List<IRenderable> rows = new()
        {
            new Align(new Markup(GameMessages.GameTitleMarkup), HorizontalAlignment.Center),
            new Align(new Markup(GameMessages.GameSubtitleMarkup), HorizontalAlignment.Center),
            new Text(""),
            new Rule("[gold1]♦ INSTÄLLNINGAR ♦[/]").RuleStyle(Color.Gold1),
            new Text("")
        };

        for (int i = 0; i < items.Length; i++)
        {
            string label = $"{items[i].Name,-20} ◄ {items[i].Value,-26} ►";
            string line = i == selected
                ? $"[bold white on #961923] ❯ {Markup.Escape(label)} [/]"
                : $"[white]   {Markup.Escape(label)} [/]";
            rows.Add(new Align(new Markup(line), HorizontalAlignment.Center));
            rows.Add(new Text(""));
        }

        rows.Add(new Rule().RuleStyle(Color.Gold1));
        rows.Add(new Align(
            new Markup("[grey]↑ ↓ Välj regel  •  ← → / Enter Ändra  •  Esc Tillbaka[/]"),
            HorizontalAlignment.Center));
        rows.Add(new Align(
            new Markup("[grey]Ändringarna gäller från nästa spel.[/]"),
            HorizontalAlignment.Center));
        return CenterMenuPanel(rows);
    }
}
