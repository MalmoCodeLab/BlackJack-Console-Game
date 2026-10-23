# Shoe and card counting

## 5. Shoe with several decks and card counting

Requires changes in shared code: `Deck`. Wait until the foundation is in place.

Replace the single deck with a shoe: several decks shuffled into one. Place a cut card at a random
position in the last quarter; when it is drawn, the round is played to the end and the shoe is
then reshuffled.

Then add a card counter that follows along in the game. The Hi-Lo system gives each card a value:

| Card | Count value |
| --- | --- |
| 2, 3, 4, 5, 6 | +1 |
| 7, 8, 9 | 0 |
| 10, jack, queen, king, ace | −1 |

The running count is the sum of all seen cards. The true count is the running count divided by
the number of decks remaining. Show both, and an indicator of how favorable the situation is.

The counter should only observe. It must not change anything in the game. If it cannot be built
without touching the rules, you have found a problem worth writing about.

## Extra, for those who finish early

- **Double down:** the player gets one more card and must then stand.
- **Split pairs:** two equal cards become two hands that are played separately. Splitting is
  hard, since it changes the structure of the whole round: a player suddenly has several hands.
  Do not attempt it until your own option is finished and merged.
