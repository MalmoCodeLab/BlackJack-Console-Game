Shared part (whole group)

Players and startup

1. As a player, I want to add 1–5 players by name at startup, so that several of us can play at the same table.
2. As a returning player, I want my previous results loaded when the program starts, so that my stats carry over.
3. As a player, I want to see my wins, losses and pushes, so that I know how I’m doing.

The deal
4. As a player, I want to receive two face-up cards at the start of the round, so that I can make my decision.
5. As a player, I want to see the dealer’s upcard but not the hole card, so that the game is fair.

Player’s turn
6. As a player, I want to choose hit or stand repeatedly until I’m done, so that I control my own hand.
7. As a player, I want my hand value calculated correctly, including aces switching between 11 and 1 and hands with several aces, so that I can trust the number shown.
8. As a player, I want my turn to end immediately and count as a loss if I bust.
9. As a player, I want to be told when I have a natural blackjack.

Dealer’s turn and outcome
10. As a player, I want the dealer to reveal the hole card and follow the fixed rule, so that the outcome is predictable and fair. The dealer hits on 16 or lower and stands on 17 or higher, including soft 17.
11. As a player, I want each hand settled against the dealer according to the rules table, with the result shown clearly. For example, a busted player always loses, and a natural beats 21 made with three or more cards.

Robustness and multiple rounds
12. As a player, I want invalid input to show an error message and ask again instead of crashing.
13. As a player, I want to play many rounds in a row, with a new shuffled deck brought in when fewer than 15 cards remain.
14. As a player, I want the deck properly shuffled, using Fisher–Yates with one shared Random, so that every round is unpredictable.
15. As a player, I want results saved to a file when I quit, so that they’re still there next time.
