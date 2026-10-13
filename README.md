# Mai26ma Grupp 1 - Blackjack

A console-based Blackjack (twenty-one) game for one to five players playing against a dealer.
The project focuses on a playable round-based game, correct hand scoring, and saving player
statistics between program runs.

## Features

- Play repeated rounds with one to five named players.
- Load each returning player's wins, losses, and pushes.
- Deal from a 52-card deck and reshuffle the discard pile when fewer than 15 cards remain.
- Apply the dealer's fixed rule: hit on 16 or less and stand on 17 or more.
- Save player statistics as JSON when the game exits.

This is the basic game: it does not use bets, chips, insurance, doubling down, or splitting pairs.

## Project responsibilities

- `Program.cs` starts the application and connects the user interface, game, and storage.
- `ConsoleUI` handles console input and output.
- `Game` coordinates rounds, turns, and outcomes.
- `Player` and `Dealer` represent the people playing; `Participant` contains behavior shared by both.
- `Deck`, `Hand`, and `Card` handle cards, dealing, and hand contents.
- `GameStorage` loads and saves player statistics in `player-results.json`.

See [Uml.md](Uml.md) for the class diagram and more detail about each class's responsibility.
The project's detailed requirements and user stories are in [UserStories.md](UserStories.md).

## Run

From the project directory, run:

```text
dotnet run
```

Player statistics are stored in `player-results.json` in the current working directory.

## Language

Class, method, and variable names are in English. Console prompts and messages are currently in
English; Swedish terminology and user stories may also appear in the project documentation.
