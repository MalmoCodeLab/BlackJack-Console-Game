class Dealer : Participant
{
    private const int DealerStandsOn = 17;
    private readonly RuleSet rules;

    public RuleSet Rules => rules;
    public bool HoleCardVisible { get; private set; }

    public Dealer(RuleSet? rules = null) : base("Dealer")
    {
        this.rules = rules ?? RuleSet.Default;
        this.rules.Validate();
    }

    public void ShowHoleCard()
    {
        HoleCardVisible = true;
    }

    // True when the dealer's face-up card is an ace (used for insurance).
    public bool ShowsAce => Hand.CardsOnHand.Count > 0 && Hand.CardsOnHand[0].Rank == Rank.Ace;

    // Casino rule: with an ace or ten-value up card the dealer peeks at the
    // hole card, and a natural ends the round before any player acts.
    public bool PeeksAndHasBlackjack =>
        Hand.CardsOnHand.Count > 0 &&
        (ShowsAce || Hand.CardsOnHand[0].Rank is Rank.Ten or Rank.Jack or Rank.Queen or Rank.King) &&
        HasBlackjack;

    public bool ShouldHit()
    {
        return Score < DealerStandsOn ||
               (Score == DealerStandsOn && Hand.IsSoft && rules.DealerHitsSoft17);
    }
    
    public override void NewRound(Deck deck)
    {
        base.NewRound(deck);
        HoleCardVisible = false;
    }
}