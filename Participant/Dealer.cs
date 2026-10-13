class Dealer : Participant
{
    public bool HoleCardVisible { get; private set; }

    public Dealer() : base("Dealer")
    {
    }

    // Dealerns första kort, det som alltid syns (story 5)
    public Card UpCard => Hand.CardsOnHand[0];

    public void ShowHoleCard()
    {
        HoleCardVisible = true;
    }

    // Fast regel (story 10): tar kort på 16 eller lägre,
    // stannar på 17 eller högre, även soft 17
    public bool ShouldHit()
    {
        return Score < 17;
    }

    // Start of a new round and hiding the hole card
    public override void NewRound()
    {
        base.NewRound();
        HoleCardVisible = false;
    }
}
