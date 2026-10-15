using System.Text.Json;

var scenarios = new (string Name, Action Run)[]
{
    ("Card values and ace scoring", CardValuesAndAceScoring),
    ("Standard deck and dealing", StandardDeckAndDealing),
    ("Initial deal and dealer hole-card visibility", InitialDealAndHoleCardVisibility),
    ("One-to-five player tables", OneToFivePlayerTables),
    ("Round construction validation", RoundConstructionValidation),
    ("Player blackjack and hit eligibility", PlayerBlackjackAndHitEligibility),
    ("Dealer hit and stand rules", DealerHitAndStandRules),
    ("Natural blackjack beats a three-card 21", NaturalBeatsThreeCardTwentyOne),
    ("Dealer blackjack and mutual blackjack", DealerBlackjackOutcomes),
    ("Regular win, loss, and push", RegularOutcomes),
    ("Player bust and dealer bust", BustOutcomes),
    ("Busted player loses when another player continues", BustedPlayerLosesInMultiplayerRound),
    ("Round lifecycle and repeat-round reshuffle", RoundLifecycleAndReshuffle),
    ("Player result counters", PlayerResultCounters),
    ("Statistics save and reload", StatisticsSaveAndReload),
};

int failures = 0;
foreach ((string name, Action run) in scenarios)
{
    try
    {
        run();
        Console.WriteLine($"PASS {name}");
    }
    catch (Exception exception)
    {
        failures++;
        Console.Error.WriteLine($"FAIL {name}: {exception.Message}");
    }
}

Console.WriteLine($"{scenarios.Length - failures}/{scenarios.Length} scenarios passed.");
return failures == 0 ? 0 : 1;

static void CardValuesAndAceScoring()
{
    Hand hand = new();
    AddCards(hand, Rank.Ace, Rank.Ace, Rank.Nine);
    Equal(21, hand.Score, "Two aces should be scored as 11 and 1 when possible.");

    hand.CardsOnHand.Clear();
    AddCards(hand, Rank.Ace, Rank.Ace, Rank.Ace, Rank.Eight);
    Equal(21, hand.Score, "Several aces should each reduce from 11 to 1 as needed.");

    hand.CardsOnHand.Clear();
    AddCards(hand, Rank.King, Rank.Queen, Rank.Two);
    Equal(22, hand.Score, "Face cards should each count as ten.");
}

static void StandardDeckAndDealing()
{
    Deck deck = new();
    deck.CreateDeck();
    Equal(52, deck.DeckCards.Count, "A standard deck should contain 52 cards.");
    Equal(52, deck.DeckCards
        .Select(card => (card.Suit, card.Rank))
        .Distinct()
        .Count(), "A standard deck should contain each card exactly once.");
    (Suit Suit, Rank Rank)[] originalCards = deck.DeckCards
        .Select(card => (card.Suit, card.Rank))
        .OrderBy(card => card.Suit)
        .ThenBy(card => card.Rank)
        .ToArray();

    deck.ShuffleDeck();
    True(originalCards.SequenceEqual(deck.DeckCards
        .Select(card => (card.Suit, card.Rank))
        .OrderBy(card => card.Suit)
        .ThenBy(card => card.Rank)),
        "Shuffling should preserve every card exactly once.");
    deck.DealCard();
    Equal(51, deck.DeckCards.Count, "Dealing should remove one card from the deck.");
}

static void InitialDealAndHoleCardVisibility()
{
    Deck deck = CreateDeck(
        [Rank.Eight, Rank.Four, Rank.Ace, Rank.Seven, Rank.Five, Rank.Six]);
    List<Player> players = [new("One"), new("Two")];
    Dealer dealer = new();
    Round round = new(1, players, dealer, deck);

    round.StartRound();

    Equal(2, players[0].Hand.CardsOnHand.Count, "The first player should get two cards.");
    Equal(2, players[1].Hand.CardsOnHand.Count, "The second player should get two cards.");
    Equal(1, dealer.VisibleCards().Count, "Only the dealer's upcard should be visible.");
    Equal(2, dealer.Hand.CardsOnHand.Count, "The dealer should have one visible card and one hole card.");
    Equal(15, players[0].Score, "The first player's cards should be dealt in order.");
    Equal(9, players[1].Score, "The second player's cards should be dealt in order.");

    round.EndRound();
    Equal(2, dealer.VisibleCards().Count, "The dealer's hole card should be revealed at round end.");
}

static void OneToFivePlayerTables()
{
    for (int playerCount = 1; playerCount <= 5; playerCount++)
    {
        Rank[] cards = Enumerable.Range(0, (playerCount + 1) * 2)
            .Select(index => (Rank)(index % 13))
            .ToArray();
        Deck deck = CreateDeck(cards);
        List<Player> players = Enumerable.Range(1, playerCount)
            .Select(number => new Player($"Player {number}"))
            .ToList();
        Dealer dealer = new();

        new Round(1, players, dealer, deck).StartRound();

        True(players.All(player => player.Hand.CardsOnHand.Count == 2),
            $"All {playerCount} players should receive two cards.");
        Equal(2, dealer.Hand.CardsOnHand.Count,
            "The dealer should receive two cards for every supported table size.");
    }
}

static void RoundConstructionValidation()
{
    List<Player> players = [new("Valid")];
    Dealer dealer = new();
    Deck deck = new();

    Throws<ArgumentOutOfRangeException>(
        () => new Round(0, players, dealer, deck),
        "Round numbers must be positive.");
    Throws<ArgumentException>(
        () => new Round(1, [], dealer, deck),
        "A round must have at least one player.");
    Throws<ArgumentException>(
        () => new Round(1, [null!], dealer, deck),
        "A round cannot contain a null player.");
    Throws<ArgumentNullException>(
        () => new Round(1, null!, dealer, deck),
        "The player list cannot be null.");
}

static void PlayerBlackjackAndHitEligibility()
{
    Player player = new("Natural");
    AddCards(player.Hand, Rank.Ace, Rank.King);
    True(player.HasBlackjack, "An ace and ten-value card should be a natural blackjack.");
    True(!player.CanHit, "A player with blackjack must not be able to hit.");

    player.Hand.CardsOnHand.Clear();
    AddCards(player.Hand, Rank.Ten, Rank.Eight);
    True(player.CanHit, "A player below 21 should be able to hit.");

    AddCards(player.Hand, Rank.Five);
    True(player.IsBust, "A score over 21 should bust.");
    True(!player.CanHit, "A busted player must not be able to hit.");

    player.Hand.CardsOnHand.Clear();
    AddCards(player.Hand, Rank.Seven, Rank.Seven, Rank.Seven);
    Equal(21, player.Score, "Three cards can total 21.");
    True(!player.HasBlackjack, "A 21 made with more than two cards is not natural blackjack.");
}

static void DealerHitAndStandRules()
{
    Dealer dealer = new();
    AddCards(dealer.Hand, Rank.Ten, Rank.Six);
    True(dealer.ShouldHit(), "The dealer should hit on 16.");

    Deck deck = CreateDeck([Rank.Ace]);
    dealer.PlayTurn(deck);
    Equal(17, dealer.Score, "The dealer should draw on 16.");
    True(!dealer.ShouldHit(), "The dealer should stand on 17.");
    True(dealer.HoleCardVisible, "The dealer's cards should be revealed during its turn.");

    dealer.Hand.CardsOnHand.Clear();
    AddCards(dealer.Hand, Rank.Ace, Rank.Six);
    True(!dealer.ShouldHit(), "The dealer should stand on soft 17.");
}

static void NaturalBeatsThreeCardTwentyOne()
{
    (Round round, List<Player> players, _, _) = StartRound(
        [Rank.Ace, Rank.Ten, Rank.King, Rank.Six, Rank.Five]);

    round.EndRound();

    Equal(21, players[0].Score, "The player's natural should total 21.");
    Equal(21, round.Dealer.Score, "The dealer should draw to a three-card 21.");
    Equal(Outcome.Blackjack, round.Outcomes[players[0]],
        "A natural blackjack should beat a three-card 21.");
    True(RoundResultFormatter.Format(round).Contains(
        "DU VANN!", StringComparison.Ordinal),
        "The formatted result should announce a solo blackjack win.");
}

static void DealerBlackjackOutcomes()
{
    (Round round, List<Player> players, _, _) = StartRound(
        [Rank.Ten, Rank.Ace, Rank.Seven, Rank.King]);

    round.EndRound();

    Equal(Outcome.Lose, round.Outcomes[players[0]],
        "Dealer blackjack should beat a non-natural player hand.");

    (round, players, _, _) = StartRound(
        [Rank.Ace, Rank.Ace, Rank.King, Rank.King]);
    round.EndRound();
    Equal(Outcome.Push, round.Outcomes[players[0]],
        "Two natural blackjacks should push.");
}

static void RegularOutcomes()
{
    (Round round, List<Player> players, _, _) = StartRound(
        [Rank.Ten, Rank.Ten, Rank.Nine, Rank.Seven]);
    round.EndRound();
    Equal(Outcome.Win, round.Outcomes[players[0]], "A higher total should win.");

    (round, players, _, _) = StartRound(
        [Rank.Ten, Rank.Ten, Rank.Six, Rank.Seven]);
    round.EndRound();
    Equal(Outcome.Lose, round.Outcomes[players[0]], "A lower total should lose.");

    (round, players, _, _) = StartRound(
        [Rank.Ten, Rank.Ten, Rank.Seven, Rank.Seven]);
    round.EndRound();
    Equal(Outcome.Push, round.Outcomes[players[0]], "Equal totals should push.");
}

static void BustOutcomes()
{
    (Round round, List<Player> players, _, Deck deck) = StartRound(
        [Rank.Ten, Rank.Ten, Rank.Nine, Rank.Six, Rank.Five]);
    players[0].ReceiveCard(deck);
    round.EndRound();
    True(players[0].IsBust, "The player should bust after drawing to 24.");
    Equal(Outcome.Lose, round.Outcomes[players[0]], "A busted player should lose.");

    (round, players, _, _) = StartRound(
        [Rank.Ten, Rank.Ten, Rank.Eight, Rank.Six, Rank.Six]);
    round.EndRound();
    True(round.Dealer.IsBust, "The dealer should bust after drawing on 16.");
    Equal(Outcome.Win, round.Outcomes[players[0]], "A dealer bust should award the player a win.");
}

static void BustedPlayerLosesInMultiplayerRound()
{
    (Round round, List<Player> players, _, Deck deck) = StartRound(
        [Rank.Ten, Rank.Ten, Rank.Ten, Rank.Nine, Rank.Seven, Rank.Six],
        playerCount: 2,
        extraCards: [Rank.Five, Rank.Six]);
    players[0].ReceiveCard(deck);

    round.EndRound();

    True(round.Dealer.IsBust, "The dealer should bust after the surviving player's round.");
    Equal(Outcome.Lose, round.Outcomes[players[0]],
        "A busted player must lose even if the dealer also busts.");
    Equal(Outcome.Win, round.Outcomes[players[1]],
        "The non-busted player should win when the dealer busts.");
    string summary = RoundResultFormatter.Format(round);
    True(summary.Contains("Player 1: FÖRLORADE!", StringComparison.Ordinal),
        "The multiplayer summary should identify the busted player's loss.");
    True(summary.Contains("Player 2: VANN!", StringComparison.Ordinal),
        "The multiplayer summary should identify the surviving player's win.");
}

static void RoundLifecycleAndReshuffle()
{
    Deck deck = CreateDeck([Rank.Two, Rank.Ten, Rank.Three, Rank.Eight], cardCount: 18);
    List<Player> players = [new("Repeat")];
    Dealer dealer = new();
    Round firstRound = new(1, players, dealer, deck);

    firstRound.StartRound();
    Equal(14, deck.DeckCards.Count, "The first round should leave fewer than 15 cards.");
    firstRound.EndRound();
    Throws<InvalidOperationException>(firstRound.StartRound,
        "A round cannot be started twice.");
    Throws<InvalidOperationException>(firstRound.EndRound,
        "A round cannot be ended twice.");

    Round secondRound = new(2, players, dealer, deck);
    Throws<InvalidOperationException>(secondRound.EndRound,
        "An unstarted round cannot be ended.");
    secondRound.StartRound();
    Equal(14, deck.DeckCards.Count,
        "The next round should recycle the four discarded cards and deal four cards.");
    Equal(0, deck.DiscardedCards.Count, "The discard pile should be recycled at the threshold.");
    Equal(18, deck.DeckCards.Count + deck.DiscardedCards.Count +
        players.Sum(player => player.Hand.CardsOnHand.Count) +
        dealer.Hand.CardsOnHand.Count,
        "Cards should remain accounted for after reshuffling and dealing.");

}

static void PlayerResultCounters()
{
    Player player = new("Stats");
    player.RecordResult(Outcome.Win);
    player.RecordResult(Outcome.Blackjack);
    player.RecordResult(Outcome.Lose);
    player.RecordResult(Outcome.Push);

    Equal(2, player.Wins, "A regular win and blackjack should count as wins.");
    Equal(1, player.Losses, "A loss should increment the loss counter.");
    Equal(1, player.Pushes, "A push should increment the push counter.");
    Equal(4, player.GamesPlayed, "Games played should equal all recorded results.");
}

static void StatisticsSaveAndReload()
{
    string directory = Path.Combine(
        Path.GetTempPath(), $"blackjack-scenarios-{Guid.NewGuid():N}");
    string jsonPath = Path.Combine(directory, "results.json");

    try
    {
        GameStorage storage = new(jsonPath);
        Dictionary<string, PlayerStatistics> results = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Returning player"] = new() { Wins = 3, Losses = 2, Pushes = 1 }
        };

        storage.SaveResults(results);
        Dictionary<string, PlayerStatistics> loaded = storage.LoadResults();

        True(loaded.TryGetValue("RETURNING PLAYER", out PlayerStatistics? stats),
            "Saved player names should load case-insensitively.");
        Equal(3, stats!.Wins, "Wins should persist.");
        Equal(2, stats.Losses, "Losses should persist.");
        Equal(1, stats.Pushes, "Pushes should persist.");
        True(File.Exists(Path.ChangeExtension(jsonPath, ".txt")),
            "Saving results should also write the readable report.");

        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(jsonPath));
        True(document.RootElement.TryGetProperty("Returning player", out _),
            "The JSON file should contain the saved player.");
    }
    finally
    {
        if (Directory.Exists(directory))
            Directory.Delete(directory, recursive: true);
    }
}

static (Round Round, List<Player> Players, Dealer Dealer, Deck Deck) StartRound(
    Rank[] initialCards,
    int playerCount = 1,
    Rank[]? extraCards = null)
{
    List<Rank> orderedCards = [.. initialCards];
    if (extraCards is not null)
        orderedCards.AddRange(extraCards);

    Deck deck = CreateDeck(orderedCards.ToArray());
    List<Player> players = Enumerable.Range(1, playerCount)
        .Select(number => new Player($"Player {number}"))
        .ToList();
    Dealer dealer = new();
    Round round = new(1, players, dealer, deck);
    round.StartRound();
    return (round, players, dealer, deck);
}

static Deck CreateDeck(Rank[] topCards, int cardCount = 52)
{
    if (cardCount < topCards.Length)
        throw new ArgumentOutOfRangeException(nameof(cardCount));

    Deck deck = new();
    deck.CreateDeck();
    for (int index = 0; index < topCards.Length; index++)
    {
        int cardIndex = deck.DeckCards.FindIndex(
            index, card => card.Rank == topCards[index]);
        if (cardIndex < 0)
            throw new InvalidOperationException($"No {topCards[index]} card remains in the deck.");

        Card card = deck.DeckCards[cardIndex];
        deck.DeckCards.RemoveAt(cardIndex);
        deck.DeckCards.Insert(index, card);
    }

    if (cardCount < deck.DeckCards.Count)
        deck.DeckCards.RemoveRange(cardCount, deck.DeckCards.Count - cardCount);

    return deck;
}

static void AddCards(Hand hand, params Rank[] ranks)
{
    foreach (Rank rank in ranks)
        hand.CardsOnHand.Add(new Card(Suit.Hearts, rank));
}

static void Equal<T>(T expected, T actual, string message)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new InvalidOperationException($"{message} Expected: {expected}; actual: {actual}.");
}

static void True(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}

static void Throws<TException>(Action action, string message)
    where TException : Exception
{
    try
    {
        action();
    }
    catch (TException)
    {
        return;
    }

    throw new InvalidOperationException(message);
}
