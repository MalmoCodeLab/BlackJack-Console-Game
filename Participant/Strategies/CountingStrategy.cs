// Expert: basic strategy plus card counting with the shared CardCounter.
// A high true count means many tens and aces are left, which helps the player.
//
// - Bets more when the true count is high (bet spread).
// - Takes insurance from a true count of +3, the only time it pays off.
// - Leaves basic strategy at the best-known count limits ("Illustrious 18").
class CountingStrategy : BasicStrategy, IPlayerStrategy
{
    private const int InsuranceFromCount = 3;
    private const int TenValue = 10;
    private const int NineValue = 9;

    // Bet spread: the bet grows with the true count.
    private static readonly (double FromCount, int Bet)[] BetSpread =
    {
        (3, 100),
        (2, 50),
        (1, 25),
    };

    // Hard hands that stand from a certain true count instead of following basic strategy.
    // (player total, dealer card, stand when the true count is at least this)
    private static readonly (int Total, int Dealer, double StandFrom)[] Deviations =
    {
        (16, TenValue, 0),
        (16, NineValue, 5),
        (15, TenValue, 4),
        (13, 2, -1),
        (13, 3, -2),
        (12, 2, 3),
        (12, 3, 2),
        (12, 4, 0),
        (12, 5, -2),
        (12, 6, -1),
    };

    public override Move Decide(Hand hand, Card dealerUpcard, double trueCount)
    {
        if (!hand.IsSoft)
        {
            int dealer = dealerUpcard.IndividualCardValue(dealerUpcard);
            foreach ((int total, int dealerCard, double standFrom) in Deviations)
            {
                if (hand.Score == total && dealer == dealerCard)
                    return trueCount >= standFrom ? Move.Stand : Move.Hit;
            }
        }

        return base.Decide(hand, dealerUpcard, trueCount);
    }

    public int ChooseBet(int chips, double trueCount)
    {
        foreach ((double fromCount, int bet) in BetSpread)
        {
            if (trueCount >= fromCount && chips >= bet)
                return bet;
        }
        return Wallet.MinimumBet;
    }

    public bool TakeInsurance(double trueCount) => trueCount >= InsuranceFromCount;
}
