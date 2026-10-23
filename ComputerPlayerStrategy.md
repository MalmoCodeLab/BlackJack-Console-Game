# Computer player with strategy

## 2. Computer players with strategy

Only touches new code, provided the group solved design question three correctly. Can be started
early.

Add computer-controlled players at the table. They should be able to sit at the same time as
human players, and several computer players should be able to use different strategies at the
same time.

Build a common interface, for example `IPlayerStrategy`, with one method that receives the
player's hand and the dealer's upcard and returns a `Move`. Implement at least two:

- A simple one: hit as long as the hand is under 17.
- One that follows basic strategy: look up the right move in a table based on the value of the
  player's own hand, whether it is soft or hard, and which card the dealer shows.

The basic strategy table is published and you may use a ready-made table, but you must be able to
explain how your code reads it.

This is polymorphism that makes something understandable: the same call, different behavior,
interchangeable at runtime.
