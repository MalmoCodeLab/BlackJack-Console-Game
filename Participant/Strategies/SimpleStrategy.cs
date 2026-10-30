// Medel: takes a card as long as the hand is under 17, just like the dealer.
class SimpleStrategy : IPlayerStrategy
{
    private const int StandsOn = 17;

    public Move Decide(Hand hand, Card dealerUpcard, double trueCount) =>
        hand.Score < StandsOn ? Move.Hit : Move.Stand;
}
