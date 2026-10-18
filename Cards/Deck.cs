class Deck
{
    public RuleSet Rules { get; }
    public List<Card> DeckCards { get; } = [];
    public List<Card> DiscardedCards { get; } = [];
    public List<Hand> ListOfHands { get; } = [];
    public int CutCardPosition { get; private set; }
    public bool CutCardReached { get; private set; }
    public int RunningCount {get; private set;}
    public double TrueCount {get; private set;}

    public Deck(RuleSet? rules = null)
    {
        Rules = rules ?? RuleSet.Default;
        Rules.Validate();
    }

    public Hand CreateHand()
    {
        Hand hand = new Hand();
        ListOfHands.Add(hand);
        return hand;
    }

    public void CreateDeck()
    {
        for (int i = 0; i < Rules.DeckCount; i++)
        {
            for (int s = 0; s < 4; s++)
            {
                for (int r = 0; r < 13; r++)
                {
                    Card card = new((Suit)s, (Rank)r);
                    DeckCards.Add(card);
                }
            }
        }
    }
    private void PlaceCutCard()
    {
        int lastQuarter = DeckCards.Count / 4;
        int min = Math.Min(Rules.ReshuffleThreshold, DeckCards.Count);
        int max = Math.Max(min, lastQuarter);
        CutCardPosition = Random.Shared.Next(min, max + 1);
        CutCardReached = false;
    }
    public void CardCounter(Card card)
    {
        if (card.Rank <= Rank.Six)
        {
            RunningCount++;
        }
        else if (card.Rank >= Rank.Ten)
        {
            RunningCount--;
        }
        double decksRemaining = DeckCards.Count / 52.0;
        double rawTrueCount = (double)RunningCount / decksRemaining;
        TrueCount = Math.Round(rawTrueCount, 2, MidpointRounding.AwayFromZero);

    }
    public void ShuffleDeck()
    {
        if (DiscardedCards.Count > 0)
        {
            int nrOfCardsToTransfer = DiscardedCards.Count;
            for (int i = 0; i < nrOfCardsToTransfer; i++)
            {
                DeckCards.Add(DiscardedCards[0]);
                DiscardedCards.RemoveAt(0);
            }
        }
        for (int i = DeckCards.Count - 1; i > 0; i--)
        {
            int number = Random.Shared.Next(0, i + 1);
            Card TempCard = DeckCards[number];
            DeckCards[number] = DeckCards[i];
            DeckCards[i] = TempCard;
        }
        PlaceCutCard();
        RunningCount = 0;
        TrueCount = 0;

    }
    public void CheckIfNewShuffleIsNeeded(int cardsNeeded = 0)
    {
        if (cardsNeeded < 0)
            throw new ArgumentOutOfRangeException(nameof(cardsNeeded));

        if (CutCardReached || DeckCards.Count < cardsNeeded)
        {
            ShuffleDeck();
        }
    }
    public Card DealCard(bool faceUp = true)
    {
        if (DeckCards.Count == 0)
            throw new InvalidOperationException("Leken är tom.");
        Card dealCard = DeckCards[0];
        DeckCards.RemoveAt(0);
        if (DeckCards.Count <= CutCardPosition)
            CutCardReached = true;
        if (faceUp)
        {
            CardCounter(dealCard);
        }
        return dealCard;
    }
    public void ForceRetriveAllCards()
    {
        for (int i = 0; i < ListOfHands.Count; i++)
        {
            ReciveDiscardedCard(ListOfHands[i]);
        }
    }
    public void ReciveDiscardedCard(Hand hand)
    {
        List<Card> recievedDiscardedCard = hand.DiscardCards();
        int numberOfCards = recievedDiscardedCard.Count();
        for (int i = 0; i < numberOfCards; i++)
        {
            DiscardedCards.Add(recievedDiscardedCard[0]);
            recievedDiscardedCard.RemoveAt(0);
        }
    }


}