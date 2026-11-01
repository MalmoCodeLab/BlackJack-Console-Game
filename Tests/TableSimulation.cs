namespace Mai26ma_Grupp1_BlackJack.Tests;

// Plays full rounds with the real game classes and no UI.
// Players hit below 17, like a simple automatic player.
internal sealed class TableSimulation
{
    private const int HitBelow = 17;
    private readonly Random random;
    private int roundNumber;

    public RuleSet Rules { get; }
    public Deck Deck { get; }
    public Dealer Dealer { get; }
    public List<Player> Seated { get; }
    public int TotalCards => Rules.DeckCount * Deck.CardsPerDeck;

    public TableSimulation(RuleSet rules, int playerCount, int seed, int chips = Wallet.StartingChips)
    {
        Rules = rules;
        random = new Random(seed);
        Deck = new Deck(rules);
        Deck.CreateDeck();
        Deck.ShuffleDeck();
        Dealer = new Dealer(rules);
        Seated = Enumerable.Range(1, playerCount)
            .Select(i => new Player($"P{i}", chips: chips))
            .ToList();
    }

    public int CardsAccountedFor() =>
        Deck.DeckCards.Count
        + Deck.DiscardedCards.Count
        + Dealer.Hand.CardsOnHand.Count
        + Seated.Sum(player => player.Hand.CardsOnHand.Count);

    // Returns null when nobody can afford a bet.
    public RoundRecord? PlayRound()
    {
        List<Player> active = Seated.Where(player => !player.IsOutOfChips).ToList();
        if (active.Count == 0)
            return null;

        Dictionary<Player, int> chipsBefore = new();
        Dictionary<Player, int> bets = new();
        foreach (Player player in active)
        {
            int maxBet = Math.Min(player.Chips, 100);
            int bet = random.Next(Wallet.MinimumBet, maxBet + 1);
            Assert.True(player.TryPlaceBet(bet));
            chipsBefore[player] = player.Chips + bet;
            bets[player] = bet;
        }

        Round round = new(++roundNumber, active, Dealer, Deck, Rules);
        int discardedBefore = Deck.DiscardedCards.Count;
        round.StartRound();
        bool reshuffled = discardedBefore > 0 && Deck.DiscardedCards.Count == 0;
        int cardsLeftAfterDeal = Deck.DeckCards.Count;
        round.RevealInitialCards();

        if (!Dealer.PeeksAndHasBlackjack)
        {
            foreach (Player player in active)
            {
                while (!player.HasBlackjack && player.CanHit && player.Score < HitBelow)
                    player.ReceiveCard(Deck);
            }
        }

        round.EndRound();
        return new RoundRecord(round, active, chipsBefore, bets, reshuffled, cardsLeftAfterDeal);
    }

    public static int ExpectedChipChange(Outcome outcome, int bet) => outcome switch
    {
        Outcome.Win => bet,
        Outcome.Blackjack => bet * 3 / 2,
        Outcome.Push => 0,
        Outcome.Loss => -bet,
        _ => throw new ArgumentOutOfRangeException(nameof(outcome))
    };
}

internal sealed record RoundRecord(
    Round Round,
    List<Player> Players,
    Dictionary<Player, int> ChipsBefore,
    Dictionary<Player, int> Bets,
    bool Reshuffled,
    int CardsLeftAfterDeal);
