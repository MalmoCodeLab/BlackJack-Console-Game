class Dealer : Participant
{
    public bool HoleCardVisible { get; private set; }

    public Dealer() : base("Dealer")
    {
    }


    public Card UpCard => Hand.CardsOnHand[0];

    public void ShowHoleCard()
    {
        HoleCardVisible = true;
    }

    public bool ShouldHit()
    {
        return Score < 17;
    }

    public override void NewRound()
    {
        base.NewRound();
        HoleCardVisible = false;
    }
}
