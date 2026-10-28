# Blackjack Console Game

A console-based Blackjack game developed in C# with a focus on object-oriented design, configurable game rules, and maintainable code.

The project implements the core mechanics of Blackjack, including card management, player and dealer logic, configurable rules, and persistent player statistics.

Originally developed as a collaborative project, the application provides a foundation for continued development and technical improvements.

## Features

- **Blackjack gameplay** — Play against the dealer using the implemented game rules.
- **Configurable rules** — Customize game settings through a JSON configuration file.
- **Card and deck management** — Handle cards, decks, hands, players, and the dealer through separate components.
- **Persistent player statistics** — Store player results between sessions.
- **Input validation and error handling** — Handle invalid configuration and other expected error conditions.
- **Automated tests** — Use xUnit to test important parts of the application.

## Technology Stack

- **Language:** C#
- **Platform:** .NET
- **Configuration:** JSON
- **Testing:** xUnit
- **Version control:** Git and GitHub

## Architecture

The application is organized into separate components to keep responsibilities clear and make the code easier to maintain and extend.

The main areas include:

| Component | Responsibility |
|---|---|
| Cards and Deck | Represent cards and manage the deck |
| Hands | Manage cards held by a player or dealer |
| Players and Dealer | Represent the participants in a game |
| Game and Round | Coordinate gameplay and round progression |
| Configuration | Load and manage configurable game rules |
| Storage | Persist player results |
| User Interface | Handle interaction with the player |
| Tests | Verify important game logic and configuration behavior |

## Getting Started

### Prerequisites

- Git
- The .NET SDK compatible with the project

### Clone the repository

```bash
git clone https://github.com/MalmoCodeLab/Blackjack-Console-Game.git
cd Blackjack-Console-Game
```

Replace `ORG-NAME` with the GitHub organization or account that hosts the repository.

### Build the application

From the repository root, run:

```bash
dotnet build
```

### Run the tests

```bash
dotnet test
```

### Run the game

Navigate to the directory containing the application's `.csproj` file and run:

```bash
dotnet run
```

If the repository contains multiple projects, specify the path to the application project:

```bash
dotnet run --project path/to/Application.csproj
```

## Configuration

The game supports configurable rules through a JSON configuration file.

Depending on the implemented configuration options, settings may include:

- Number of decks.
- Whether the dealer hits on soft 17.
- Deck reshuffling thresholds.
- Rules governing natural Blackjack and other winning hands.

This approach separates game rules from the implementation, making it easier to adjust the game without changing the underlying logic.

## Testing

The project includes automated tests written with xUnit.

Tests help verify game behavior, configuration handling, and important edge cases. Run the test suite using:

```bash
dotnet test
```

## Team and Collaboration

Developed collaboratively by a team of four developers.

The project has involved object-oriented programming, shared code development, version control, and collaborative problem-solving.

Each contributor can use their individual GitHub profile to highlight their specific contributions to the project.

## Future Development

Potential areas for further development include:

- Expanding automated test coverage.
- Introducing continuous integration with GitHub Actions.
- Improving the user interface and overall user experience.
- Extending game statistics and reporting.
- Adding new game modes or configurable rules.

These are potential improvements rather than features currently implemented.

## Project Status

The project is available as a foundation for continued development.

New features, testing, and documentation can be introduced incrementally as the project evolves.

---

**Interested in the project?** Explore the source code, review the implementation, or follow its development through GitHub.