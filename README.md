# Mai26ma Grupp 1 - Blackjack

A console-based Blackjack (twenty-one) game for one to five players playing against a dealer.
The project focuses on a playable round-based game, correct hand scoring, and saving player
statistics between program runs.

## Features

- Play repeated rounds with one to five named players.
- Load each returning player's wins, losses, and pushes.
- Deal from a 52-card deck and reshuffle the discard pile when fewer than 15 cards remain.
- Apply the dealer's fixed rule: hit on 16 or less and stand on 17 or more.
- Save cumulative player statistics as JSON for the game and as a readable `player-results.txt`
  report for players.

This is the basic game: it does not use bets, chips, insurance, doubling down, or splitting pairs.

## Project responsibilities

- `Program.cs` starts the application.
- `Game` coordinates the main menu, player sessions, and round lifecycle.
- `GameTableUI` renders the active table and handles gameplay navigation.
- `ConsoleUI` handles shared console screens and saved-player selection.
- `PlayerSessionService` creates and loads players and coordinates statistics persistence.
- `RoundResultFormatter` formats the outcomes resolved by a round.
- `Player` and `Dealer` represent the people playing; `Participant` contains behavior shared by both.
- `Deck`, `Hand`, and `Card` handle cards, dealing, and hand contents.
- `GameStorage` loads and saves player statistics in the user's local
application-data folder.
On macOS, the save will typically be under ~/Library/Application Support/Mai26ma-Grupp1-BlackJack; on Windows, it’s under %LOCALAPPDATA%\Mai26ma-Grupp1-BlackJack.

See [Uml.md](Uml.md) for the class diagram and more detail about each class's responsibility.
The project's detailed requirements and user stories are in [UserStories.md](UserStories.md).

## Run

From the project directory, run:

```text
dotnet run
```

To run the automated game and UI tests after a change:

```text
dotnet test
```

The test summary is printed in the terminal. To also save a report that can be
opened later, run:

```text
dotnet test --logger "trx;LogFileName=blackjack-tests.trx"
```

The report is written to `Test/TestResults/blackjack-tests.trx`. It lists each
test as passed or failed and includes failure details, such as expected and
actual values. A report can only detect a rule or count that the tests assert;
add or update a test when adding functionality or changing expected behavior.

The xUnit suite uses deterministic card sequences to check card scoring, dealing,
player and dealer rules, round outcomes, reshuffling, and statistics persistence. It
also checks UI menu navigation and betting controls without requiring interactive
terminal input. `dotnet test` returns a nonzero exit code if any test fails.

Player statistics are saved as `player-results.json` and summarized in `player-results.txt` under
the user's local application-data folder (`%LOCALAPPDATA%\Mai26ma-Grupp1-BlackJack` on Windows).
The text report contains cumulative totals from previous sessions as well as the latest saved
results. If the game finds an older `player-results.json` in its current working directory and no
app-data results exist yet, it imports those results into the new location without removing the
older file. Results are local to one computer and are not automatically synchronized.

## Language

Class, method, and variable names are in English. Console prompts and messages are currently in
English; Swedish terminology and user stories may also appear in the project documentation.
