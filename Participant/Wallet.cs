class Wallet
{
    public const int StartingChips = 1000;
    private const int WinMultiplier = 2;                // bet back + 1:1
    private const int BlackjackPayoutNumerator = 3;     // blackjack pays 3:2
    private const int BlackjackPayoutDenominator = 2;
    private const int InsuranceCostDivisor = 2;         // insurance costs half the bet
    private const int InsuranceMultiplier = 3;          // insurance back + 2:1
    public const int MinimumBet = 5; 

    public int Chips { get; private set; }
    public int CurrentBet { get; private set; }
    public int InsuranceBet { get; private set; }
    public int InsuranceCost => CurrentBet / InsuranceCostDivisor;
  
    public bool IsOutOfChips => Chips < MinimumBet;

    public Wallet(int chips = StartingChips)
    {
        if (chips < 0)
            throw new ArgumentOutOfRangeException(nameof(chips), "Marker kan inte vara negativa.");
        Chips = chips;
    }

    // Places a bet before the deal. Returns false if the amount is not allowed.
    public bool TryPlaceBet(int amount)
    {
        if (amount <= 0 || amount > Chips)
            return false;

        Chips -= amount;
        CurrentBet += amount;
        return true;
    }

    // Gives the whole bet back to the player.
    public void CancelBet()
    {
        Chips += CurrentBet;
        CurrentBet = 0;
    }

    // Pays out the bet. The bet was already taken from Chips when it was placed.
    public void SettleBet(Outcome outcome)
    {
        switch (outcome)
        {
            case Outcome.Win:
                Chips += CurrentBet * WinMultiplier;
                break;
            case Outcome.Blackjack:
                Chips += CurrentBet + CurrentBet * BlackjackPayoutNumerator / BlackjackPayoutDenominator;
                break;
            case Outcome.Push:
                Chips += CurrentBet;                      // bet back
                break;
            case Outcome.Loss:
                break;                                    // bet is lost
        }
        CurrentBet = 0;
    }

    // Insurance costs half the bet. Returns false if the player cannot afford it.
    public bool TryTakeInsurance()
    {
        int amount = InsuranceCost;
        if (amount <= 0 || amount > Chips || InsuranceBet > 0)
            return false;

        Chips -= amount;
        InsuranceBet = amount;
        return true;
    }

    // Pays 2:1 if the dealer has blackjack, otherwise the insurance is lost.
    public void SettleInsurance(bool dealerHasBlackjack)
    {
        if (dealerHasBlackjack)
            Chips += InsuranceBet * InsuranceMultiplier; // insurance back + 2:1
        InsuranceBet = 0;
    }
}