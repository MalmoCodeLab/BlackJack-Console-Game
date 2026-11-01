# Shoe and card counting

## 5. Shoe with several decks and card counting

This part is implemented in `Cards/Deck.cs`, `Cards/CardCounter.cs` and `GameLogic/Round.cs`.
The table display is in `Interface/ConsoleUi.Table.cs` and `Interface/GameTableUI.cs`.

### The shoe

`Deck` is a shoe: `RuleSet.DeckCount` decks (2-8, see [HouseRules.md](HouseRules.md)) are
created with `CreateDeck()` and shuffled together with a Fisher-Yates shuffle in `ShuffleDeck()`.
The discard pile is put back into the shoe at every shuffle, and new cards are never created
during a game.

### Cut card

Each shuffle places a cut card at a random position between `ReshuffleThreshold` cards remaining
(minimum 25) and a quarter of the shoe remaining. If the threshold is larger than a quarter, the
cut card is placed at the threshold. `ReshuffleThreshold` cannot exceed half of the shoe.

When the number of cards left reaches that position, `CutCardReached` becomes true. The round
in progress is played to the end, and the shoe is reshuffled at the start of the next round
(`Round.StartRound` calls `Deck.CheckIfNewShuffleIsNeeded`). The shoe is also reshuffled when the
remaining cards could not cover the worst case of 11 cards for every participant. If the shoe
runs empty mid-round, the discard pile is reshuffled into it.

### Card counter

`CardCounter` follows the Hi-Lo system and is owned by `Deck` (`Deck.Counter`):

| Card | Count value |
| --- | --- |
| 2, 3, 4, 5, 6 | +1 |
| 7, 8, 9 | 0 |
| 10, jack, queen, king, ace | −1 |

- `RunningCount` is the sum of the values of all cards that have become visible.
- `TrueCount` is the running count divided by the number of decks remaining (cards left in the
  shoe / 52), rounded to two decimals. It is `0` when the shoe is empty.
- `Reset()` sets both to zero. The counter is reset on every shuffle.

Only visible cards are counted:

- `Deck.DealCard(faceUp: true)` counts the card when it is dealt face up (for example when a
  player or the dealer hits).
- The initial cards are dealt face down. `Round.RevealCard` counts a card the first time it is
  revealed, and `Round.RevealInitialCards` reveals the players' cards and the dealer's up card.
  The dealer's hole card is counted when it is revealed in `Round.EndRound`. A card is never
  counted twice.

The counter only observes; it does not affect any game rule or outcome.

### What the player sees

The table shows the current true count as `Fördel: +x.x` for the whole game. The text is green
when the true count is +1 or higher, red when it is −1 or lower, and gold in between. The running
count is calculated but not displayed.

### Tests

`Tests/CardCounterTests.cs` covers the Hi-Lo values, the true count, an empty shoe and reset.
The cut card and reshuffle behavior are covered in `Tests/HouseRulesGameplayTests.cs`.

## Not implemented

Double down and split pairs are not supported (see [README.md](README.md)).
