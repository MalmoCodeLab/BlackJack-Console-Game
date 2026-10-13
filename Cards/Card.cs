public enum Suit {Hearts, Diamonds, Clubs, Spades}
public enum Rank {Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace}
class Card
{
    public Suit Suit {get; }
    public Rank Rank {get; }
    public Card(Suit suit, Rank rank)
    {
        Suit = suit;
        Rank = rank;
    }


    public int IndividualCardValue (Card card)
    {
        int cardValue;
        if (Rank == Rank.Two)
        {
            cardValue = 2;
        }
        else if (Rank == Rank.Three)
        {
            cardValue = 3;
        }
        else if (Rank == Rank.Four)
        {
            cardValue = 4;
        }
        else if (Rank == Rank.Five)
        {
            cardValue = 5;
        }
        else if (Rank == Rank.Six)
        {
            cardValue = 6;
        }
        else if (Rank == Rank.Seven)
        {
            cardValue = 7;
        }
        else if (Rank == Rank.Eight)
        {
            cardValue = 8;
        }
        else if (Rank == Rank.Nine)
        {
            cardValue = 9;
        }
        else if (Rank == Rank.Ten || Rank == Rank.Jack || Rank == Rank.Queen || Rank == Rank.King)
        {
            cardValue = 10;
        }
        else 
        {
            cardValue = 11;
        }
        return cardValue;
    }
}