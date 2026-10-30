// Svår: follows the published basic strategy for hit and stand.
// The game has no double down or split, so only those two columns are used.
//
// Hard hands (no ace counted as 11):
//   17+      stand
//   13–16    stand if the dealer shows 2–6, otherwise hit
//   12       stand if the dealer shows 4–6, otherwise hit
//   11 or less  hit
// Soft hands (an ace counted as 11):
//   19+      stand
//   18       stand if the dealer shows 2–8, otherwise hit
//   17 or less  hit
class BasicStrategy : IPlayerStrategy
{
    private const int HardAlwaysStand = 17;
    private const int HardStandVsWeakDealer = 13;
    private const int HardTwelve = 12;
    private const int SoftAlwaysStand = 19;
    private const int SoftEighteen = 18;

    private const int WeakDealerHighest = 6;   // dealer shows 2–6
    private const int TwelveStandFrom = 4;     // 12 stands against 4–6
    private const int SoftEighteenStandUpTo = 8;

    public virtual Move Decide(Hand hand, Card dealerUpcard, double trueCount)
    {
        int score = hand.Score;
        int dealer = dealerUpcard.IndividualCardValue(dealerUpcard); // 2–11

        if (hand.IsSoft)
        {
            if (score >= SoftAlwaysStand)
                return Move.Stand;
            if (score == SoftEighteen && dealer <= SoftEighteenStandUpTo)
                return Move.Stand;
            return Move.Hit;
        }

        if (score >= HardAlwaysStand)
            return Move.Stand;
        if (score >= HardStandVsWeakDealer)
            return dealer <= WeakDealerHighest ? Move.Stand : Move.Hit;
        if (score == HardTwelve)
            return dealer >= TwelveStandFrom && dealer <= WeakDealerHighest
                ? Move.Stand
                : Move.Hit;
        return Move.Hit;
    }
}
