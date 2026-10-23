# Interface

## 1. Card graphics in the console

Only touches new code. Can be started early.

The basic version shows cards as text: ♥A ♠10. This option draws them as cards. A card becomes a
box with a border, the rank in two corners and the suit in the middle. The dealer's hole card is
drawn face down. The cards in a hand should be placed next to each other, not below each other.

```text
┌───────┐ ┌───────┐ ┌───────┐
│A      │ │10     │ │▒▒▒▒▒▒▒│
│       │ │       │ │▒▒▒▒▒▒▒│
│   ♥   │ │   ♠   │ │▒▒▒▒▒▒▒│
│       │ │       │ │▒▒▒▒▒▒▒│
│      A│ │     10│ │▒▒▒▒▒▒▒│
└───────┘ └───────┘ └───────┘
```

The side-by-side placement is the technical knot. You cannot print one card at a time: you have
to build row one for all cards, then row two, and so on. Let your renderer return the rows as
`string[]` instead of printing them, so that `ConsoleUi` can weave them together.

Hearts and diamonds should be red, clubs and spades black. Keep `Console.ForegroundColor` in
`ConsoleUi`; your renderer is only responsible for the shape.

For face cards there are the chess glyphs ♚ ♛ ♞ in Unicode. They work better than trying to
draw faces.

This option is a test of the base code. If no rules class has to change when the presentation is
replaced, the division of responsibilities was right. If you have to change `Hand` or `Round` to
change the appearance, you have found a leak. Write about it in your report; both outcomes are
interesting.
