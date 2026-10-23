class CardCounter
{
    public int RunningCount { get; private set; }
    public double TrueCount { get; private set; }
    private const  int TrueCountDecimals = 2;

    public void Count(Card card, int cardsRemaining)
    {
        if (card.Rank <= Rank.Six)
            RunningCount++;
        else if (card.Rank >= Rank.Ten)
            RunningCount--;

        double decksRemaining = (double)cardsRemaining / Deck.CardsPerDeck;
        TrueCount = decksRemaining > 0
            ? Math.Round(RunningCount / decksRemaining, TrueCountDecimals, MidpointRounding.AwayFromZero)
            : 0;
    }

    public void Reset()
    {
        RunningCount = 0;
        TrueCount = 0;
    }
}