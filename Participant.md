# Player, Dealer and Betting

## Player and Dealer

`Player` and `Dealer` inherit from the abstract class `Participant`, which holds what they share:
name, hand, score, bust and blackjack. `Player` also keeps the statistics, and `Dealer` decides
when to hit, hides its hole card and shows whether its face-up card is an ace.

## Betting and chips

- Every new player starts with 1000 chips and bets before the deal, using chips of 5, 25, 100
  and 500.
- The bet may not be zero or larger than the player's chips.
- A win pays 1:1, a natural blackjack 3:2, and a push returns the bet.
- A player who runs out of chips leaves the table and cannot be loaded again.
- The chips are saved together with the other player statistics.

The money logic lives in `Wallet`. `Player` has a private `Wallet` and passes the betting calls
on to it, so `Player` handles statistics and `Wallet` handles the economy.

## Insurance

When the dealer shows an ace, each player can take insurance by answering J or N. It costs half
the bet and pays 2:1 if the dealer has a natural blackjack. Insured players are marked with (F)
in the side menu.

Main files: `Participant/Participant.cs`, `Participant/Player.cs`, `Participant/Dealer.cs`,
`Participant/Wallet.cs`, `GameLogic/Outcome.cs`.
