public class Dealer : Participant
{
    public bool HoleCardVisible { get; private set; }
    public Dealer() : base("Dealer")
    {
    
    }

    public void ShowHoleCard()
    {
        HoleCardVisible = true;
    }

    public override void NewRound()
    {
        base.NewRound();
        HoleCardVisible = false;
    }

}