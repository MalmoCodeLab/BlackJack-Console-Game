// A strategy decides what a computer player does. Every strategy is called
// the same way, so they can be swapped at runtime (polymorphism).
// trueCount comes from the card counter; only the expert strategy uses it.
interface IPlayerStrategy
{
    private const int DefaultBet = 25;

    Move Decide(Hand hand, Card dealerUpcard, double trueCount);

    // How much to bet before the deal. The default is a flat bet.
    int ChooseBet(int chips, double trueCount) =>
        chips >= DefaultBet ? DefaultBet : Wallet.MinimumBet;

    // Basic strategy says insurance does not pay off, so the default is no.
    bool TakeInsurance(double trueCount) => false;
}
