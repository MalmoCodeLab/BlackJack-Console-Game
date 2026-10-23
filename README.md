# Mai26ma Grupp 1 - Blackjack

A console-based Blackjack (twenty-one) game for one to five players playing against a dealer.
The project focuses on a playable round-based game, correct hand scoring, and saving player
statistics between program runs.

## Features

- Play repeated rounds with one to five named players.
- Load each returning player's wins, losses, and pushes.
- Configure the house rules from the settings menu or the JSON configuration file.
- Save cumulative player statistics as JSON for the game and as a readable `player-results.txt`
  report for players.

## Scrum board

See Trello for our work process : https://trello.com/b/oAcGF39l/group1-blackjack

This game uses chips and betting, with insurance. It does not support doubling down or splitting pairs.

## Project responsibilities

- `Program.cs` starts the application.
- `Game` coordinates the main menu, player sessions, and round lifecycle.
- `GameTableUI` renders the active table and handles gameplay navigation.
- `ConsoleUI` handles shared console screens and saved-player selection.
- `PlayerSessionService` creates and loads players and coordinates statistics persistence.
- `RoundResultFormatter` formats the outcomes resolved by a round.
- `Player` and `Dealer` represent the people playing; `Participant` contains behavior shared by both.
- `Deck`, `Hand`, and `Card` handle cards, dealing, and hand contents.
- `GameStorage` loads, saves, and deletes player statistics in the user's local
application-data folder.

### Class diagram

See [Uml.md](Uml.md) for the class diagram and more detail about each class's responsibility.
The project's detailed requirements and user stories are in [UserStories.md](UserStories.md).

## Individual assignments

Each group member describes their own part in a separate file and links to it here.

### 1. Interface – Alice

**GitHub:** [@Chocolatesand14](https://github.com/Chocolatesand14)

See [Interface.md](Interface.md).

### 2. House rules and configuration – Sofia

**GitHub:** [@sofiauhrbom73](https://github.com/sofiauhrbom73)

See [HouseRules.md](HouseRules.md).

### 3. Player, Dealer and Betting – Niclas

**GitHub:** [@n1cl4s](https://github.com/n1cl4s)

See [Participant.md](Participant.md).

### 4. Shoe and card counting – Christoffer

**GitHub:** [@Mr-Christoffer](https://github.com/Mr-Christoffer)

See [Cardcounting.md](Cardcounting.md).

## Run

From the project directory, run:

```text
dotnet run
```

Player statistics are saved as `player-results.json` and summarized in `player-results.txt` under
the user's local application-data folder (`%LOCALAPPDATA%\Mai26ma-Grupp1-BlackJack` on Windows).
On macOS, the save will typically be under (`~/Library/Application Support/Mai26ma-Grupp1-BlackJack`).
The text report contains cumulative totals from previous sessions as well as the latest saved
results. If the game finds an older `player-results.json` in its current working directory and no
app-data results exist yet, it imports those results into the new location without removing the
older file. Results are local to one computer and are not automatically synchronized.

## Tests

The house rules are covered by xUnit tests in the `Tests` folder (missing or invalid rules
file, dealer on soft 17, number of decks, reshuffle threshold, and natural 21 versus a
three-card 21). Run them from the project directory with:

```text
dotnet test Tests
```

## Language

Class, method, and variable names and the documentation are in English. Console prompts and
messages shown to the player are in Swedish.