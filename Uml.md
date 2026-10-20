# Blackjack Class Diagram and Responsibilities

This document describes the current runtime structure of the console Blackjack game. It focuses
on which class owns each responsibility and how the main objects collaborate.

## Class diagram

```mermaid
classDiagram
    class Program {
        +Main()
    }
    class ConsoleUI {
        +CreatePlayers(savedResults) List~Player~
        +ReadMove(player) Move
        +AskToPlayAgain() bool
        +ShowHand(participant, hideSecondCard)
        +ShowOutcome(player, outcome)
    }
    class Game {
        -List~Player~ players
        -ConsoleUI ui
        -Dealer dealer
        -Deck deck
        +Run()
        -PlayRound()
        -DetermineOutcome(player) Outcome
    }
    class GameStorage {
        -string filePath
        +LoadResults() Dictionary
        +SaveResults(results)
    }
    class PlayerStatistics {
        +int Wins
        +int Losses
        +int Pushes
        +int Chips
    }
    class Participant {
        <<abstract>>
        +int BlackjackValue$
        +string Name
        +Hand Hand
        +int Score
        +bool IsBust
        +bool HasBlackjack
        +ReceiveCard(deck)
        +NewRound(deck)
        +NewRound()
    }
    class Player {
        +int StartingChips$
        +int Wins
        +int Losses
        +int Pushes
        +int Chips
        +int CurrentBet
        +bool IsOutOfChips
        +int GamesPlayed
        +bool CanHit
        +RecordResult(outcome)
        +TryPlaceBet(amount) bool
        +CancelBet()
        +SettleBet(outcome)
    }
    class Dealer {
        +RuleSet Rules
        +bool HoleCardVisible
        +bool ShowsAce
        +ShowHoleCard()
        +ShouldHit() bool
        +NewRound(deck)
    }
    class Deck {
        +CreateDeck()
        +ShuffleDeck()
        +CheckIfNewShuffleIsNeeded()
        +DealCard() Card
        +DiscardCards(cards)
    }
    class Hand {
        +List~Card~ CardsOnHand
        +ReceiveCard(deck)
        +DiscardCards() List~Card~
    }
    class Card {
        +Suit Suit
        +Rank Rank
        +IndividualCardValue() int
    }
    class Suit {
        <<enumeration>>
        Hearts
        Diamonds
        Clubs
        Spades
    }
    class Rank {
        <<enumeration>>
        Two
        Three
        Four
        Five
        Six
        Seven
        Eight
        Nine
        Ten
        Jack
        Queen
        King
        Ace
    }
    class Move {
        <<enumeration>>
        Hit
        Stand
    }
    class Outcome {
        <<enumeration>>
        Win
        Loss
        Push
        Blackjack
    }

    Program ..> ConsoleUI : creates
    Program ..> Game : creates and runs
    Program ..> GameStorage : loads and saves
    Program ..> PlayerStatistics : maps player totals
    ConsoleUI ..> PlayerStatistics : reads saved totals
    ConsoleUI ..> Move : returns player choice
    ConsoleUI ..> Outcome : displays result
    Game --> ConsoleUI : requests input and display
    Game o-- "1..5" Player : plays
    Game *-- Dealer : owns
    Game *-- Deck : owns
    Player --|> Participant
    Dealer --|> Participant
    Participant *-- Hand : owns
    Hand ..> Deck : receives cards from
    Deck o-- Card : contains cards
    Hand o-- Card : contains dealt cards
    Card --> Suit
    Card --> Rank
    Game ..> Outcome : determines
    GameStorage ..> PlayerStatistics : persists
```

## Responsibility boundaries

| Class | Responsibility | Should not own |
|---|---|---|
| `Program` | Composition root: load saved totals, create the UI and game, start play, and save totals on exit. | Round rules or card scoring. |
| `ConsoleUI` | Read and validate console input; display players, hands, prompts, and outcomes. | Game flow, scoring, or file access. |
| `Game` | Coordinate rounds, player turns, dealer turn, result calculation, and statistics updates. | JSON serialization or low-level console input parsing. |
| `GameStorage` | Load, save, and delete cumulative player statistics from JSON and keep the readable `player-results.txt` summary in sync. | Running a round or interacting with the console. |
| `PlayerStatistics` | Represent the persisted wins, losses, and pushes for one player. | Game behavior or persistence operations. |
| `Participant` | Share a name, hand, card receiving, score calculation, bust/blackjack checks, and round reset. | Player-specific statistics or dealer policy. |
| `Player` | Track one player's wins, losses, and pushes. | Console interaction or saved-file management. |
| `Dealer` | Represent the dealer, reveal the hole card, and apply the hit/stand threshold. | Choosing moves interactively. |
| `Deck` | Create and shuffle the deck, deal cards, and recycle discards when needed. | Hand scoring or round outcomes. |
| `Hand` | Hold cards and receive or clear them. | Deciding the winner. |
| `Card` | Store a suit and rank and provide the card's base value. | Adjusting ace values for a complete hand. |
| `Move` | Define the player's available choices: hit or stand. | Input or game flow. |
| `Outcome` | Define the possible round results: win, loss, or push. | Calculating results. |

## Runtime flow

1. `Program` loads the saved statistics with `GameStorage`.
2. `ConsoleUI` asks for one to five player names and creates each `Player`, initialized with any
   saved totals.
3. `Program` creates `Game` and starts its `Run` method.
4. `Game` deals and coordinates player turns, reveals the dealer's hole card, plays the dealer's
   turn, and settles each player's result.
5. `ConsoleUI` handles the prompts and output requested by `Game`.
6. When the players quit, `Program` merges the current players' totals into the loaded statistics
   and saves them with `GameStorage`.

## Notes

- `Round.cs` currently exists as an unused placeholder; `Game` owns the round flow directly.
