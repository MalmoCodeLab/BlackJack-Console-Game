namespace Mai26ma_Grupp1_BlackJack.Tests;

public class WalletTests
{
    [Fact]
    public void NewWallet_StartsWithDefaultChips() =>
        Assert.Equal(Wallet.StartingChips, new Wallet().Chips);

    [Fact]
    public void Constructor_RejectsNegativeChips() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new Wallet(-1));

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(101)]
    public void TryPlaceBet_RejectsInvalidAmounts(int amount)
    {
        Wallet wallet = new(100);

        Assert.False(wallet.TryPlaceBet(amount));
        Assert.Equal(100, wallet.Chips);
        Assert.Equal(0, wallet.CurrentBet);
    }

    [Fact]
    public void TryPlaceBet_MovesChipsToBet_AndBetsAccumulate()
    {
        Wallet wallet = new(100);

        Assert.True(wallet.TryPlaceBet(25));
        Assert.True(wallet.TryPlaceBet(5));

        Assert.Equal(70, wallet.Chips);
        Assert.Equal(30, wallet.CurrentBet);
    }

    [Fact]
    public void CancelBet_ReturnsEverything()
    {
        Wallet wallet = new(100);
        wallet.TryPlaceBet(40);

        wallet.CancelBet();

        Assert.Equal(100, wallet.Chips);
        Assert.Equal(0, wallet.CurrentBet);
    }

    [Theory]
    [InlineData("Win", 120)]
    [InlineData("Blackjack", 130)]
    [InlineData("Push", 100)]
    [InlineData("Loss", 80)]
    public void SettleBet_PaysCorrectAmount(string outcome, int expectedChips)
    {
        Wallet wallet = new(100);
        wallet.TryPlaceBet(20);

        wallet.SettleBet(Enum.Parse<Outcome>(outcome));

        Assert.Equal(expectedChips, wallet.Chips);
        Assert.Equal(0, wallet.CurrentBet);
    }

    [Fact]
    public void SettleBet_BlackjackWithOddBet_RoundsDown()
    {
        Wallet wallet = new(100);
        wallet.TryPlaceBet(5);

        wallet.SettleBet(Outcome.Blackjack);


        // 95 left + 5 back + 7 (3:2 of 5, rounded down)
        Assert.Equal(107, wallet.Chips);
    }

    [Fact]
    public void InsuranceCost_IsHalfTheBet()
    {
        Wallet wallet = new(100);
        wallet.TryPlaceBet(20);

        Assert.Equal(10, wallet.InsuranceCost);
    }

    [Fact]
    public void TryTakeInsurance_TakesHalfBetOnlyOnce()
    {
        Wallet wallet = new(100);
        wallet.TryPlaceBet(20);

        Assert.True(wallet.TryTakeInsurance());
        Assert.False(wallet.TryTakeInsurance());

        Assert.Equal(70, wallet.Chips);
        Assert.Equal(10, wallet.InsuranceBet);
    }

    [Fact]
    public void TryTakeInsurance_FailsWithoutBetOrChips()
    {
        Wallet noBet = new(100);
        Assert.False(noBet.TryTakeInsurance());

        Wallet broke = new(20);
        broke.TryPlaceBet(20);
        Assert.False(broke.TryTakeInsurance());
    }

    [Fact]
    public void SettleInsurance_DealerBlackjack_PaysTwoToOne()
    {
        Wallet wallet = new(100);
        wallet.TryPlaceBet(20);
        wallet.TryTakeInsurance();

        wallet.SettleInsurance(dealerHasBlackjack: true);

        // 70 chips + 10 back + 20 winnings
        Assert.Equal(100, wallet.Chips);
        Assert.Equal(0, wallet.InsuranceBet);
    }

    [Fact]
    public void SettleInsurance_NoDealerBlackjack_LosesInsurance()
    {
        Wallet wallet = new(100);
        wallet.TryPlaceBet(20);
        wallet.TryTakeInsurance();

        wallet.SettleInsurance(dealerHasBlackjack: false);

        Assert.Equal(70, wallet.Chips);
        Assert.Equal(0, wallet.InsuranceBet);
    }

    [Theory]
    [InlineData(4, true)]
    [InlineData(5, false)]
    public void IsOutOfChips_BelowMinimumBet(int chips, bool expected) =>
        Assert.Equal(expected, new Wallet(chips).IsOutOfChips);
}
