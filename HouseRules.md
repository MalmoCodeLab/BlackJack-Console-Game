# House Rules

The game's house rules are collected in the `RuleSet` class. It contains rules for the dealer,
the deck, and round outcomes. `Game` loads the rules at startup and passes the same rule set
to `Dealer`, `Deck`, and `Round`.

## Changing the rules

Rules can be changed in the game's settings menu. Use the up and down arrows to select a rule,
the left and right arrows or Enter to change its value, and Esc to go back. Changes made in
the menu take effect in the next game but are not saved when the program exits.

For permanent changes, edit `blackjack-rules.json` directly. The program first looks in the
current working directory and then beside the built application. The file is copied beside the
application during the build. Restart the game to load changes from the JSON file; rebuilding
is not required.

| JSON property | Description | Default |
| --- | --- | --- |
| `DealerHitsSoft17` | Whether the dealer takes a card on a soft 17. | `false` (dealer stands) |
| `DeckCount` | Number of decks used. Valid values are 1–8. | `6` |
| `ReshuffleThreshold` | Lower bound for where the cut card is placed. A shuffle is triggered when the number of cards in the deck reaches this threshold. The value must be at least 1 and cannot exceed the total number of cards. | `15` |
| `NaturalBlackjackBeatsThreeCard21` | Whether a natural blackjack beats a 21 made with three cards. | `true` |

With the default settings, the dealer stands on soft 17, six decks are used, the cut card is
placed no earlier than when 15 cards remain, and a natural blackjack beats a three-card 21.

If the JSON file is missing, cannot be read, or contains invalid JSON or values, an English
notice is displayed on the main menu and the game uses the default rules.