// Lätt: a careful beginner who is afraid of going bust and stands on 12 or more.
// It rarely busts, but it also lets the dealer win far too often.
class BeginnerStrategy : IPlayerStrategy
{
    private const int StandsOn = 12;

    public Move Decide(Hand hand, Card dealerUpcard, double trueCount) =>
        hand.Score < StandsOn ? Move.Hit : Move.Stand;
}
