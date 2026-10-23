# Betting and chips

## 4. Betting and chips

Requires changes in shared code: `Player` and `Round`. Wait until the foundation is in place.

The basic version only counts won and lost rounds. Add an economy.

- Each player has a number of chips and bets before the deal.
- A win pays 1:1: the bet back plus the same amount.
- A natural 21 pays 3:2.
- A push returns the bet.
- A player who runs out of chips leaves the table.
- The bet may not be larger than the player's chips, and not zero.

The chips should be saved between runs, together with the other player data.

If there is time left: add insurance. When the dealer's upcard is an ace, the player may bet half
the bet that the dealer has 21. If the dealer has it, the insurance pays 2:1, otherwise it is lost.
