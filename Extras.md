# Extras: computer players

Built after the school project was finished.

When a game starts, the free seats can be filled with computer players, and you choose a level
for each one. They bet, answer the insurance question and play their hands on their own, with a
short pause so you can follow.

## Levels

| Name | How it plays |
| --- | --- |
| Bot (Lätt) | Afraid of going bust: stands on 12 or more. |
| Bot (Medel) | Takes a card while the hand is under 17, like the dealer. |
| Bot (Svår) | Follows the published basic strategy for hit and stand. |
| Bot (Expert) | Basic strategy plus card counting with the shared `CardCounter`. |

Two bots at the same level are told apart with a number: "Bot (Medel) 2".

The expert uses the true count in three ways:

- **Bet spread:** bets 5 at a low count and up to 100 when the count is high.
- **Insurance:** takes it from a true count of +3, the only time it pays off.
- **Deviations:** leaves basic strategy at the best-known count limits ("Illustrious 18"), for
  example standing on 16 against a ten when the true count is 0 or higher.

## Design

Every level is a strategy behind the interface `IPlayerStrategy`:

- `Decide(hand, dealerUpcard, trueCount)` returns `Move.Hit` or `Move.Stand`.
- `ChooseBet(chips, trueCount)` and `TakeInsurance(trueCount)` have default implementations in
  the interface (flat bet of 25, no insurance), which only the expert overrides.

`CountingStrategy` inherits from `BasicStrategy` and only adds what card counting changes. The
table calls the same three methods whatever the level is; that is polymorphism.

## Results

A simulation of 300,000 rounds with all four levels at the same table (six decks, no double or
split) gave this result per chip bet:

| Level | Result |
| --- | --- |
| Lätt | −7.9 % |
| Medel | −5.9 % |
| Svår | −2.4 % |
| Expert | −0.6 % |

Every level loses in the long run, since the game has no double down or split, but each level
loses clearly less than the one below it.

Computer players are not saved, and the game ends when only computer players have chips left.

Main files: `Participant/Strategies/`, `GameLogic/Move.cs`, `Interface/GameTableUI.cs`.
