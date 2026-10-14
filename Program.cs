Deck deck = new();
deck.CreateDeck();
Hand hand1 = deck.CreateHand();
deck.ShuffleDeck();
for (int i = 0; i < deck.DeckCards.Count; i++)
{
    Console.WriteLine(i + ". " + deck.DeckCards[i].Suit + "" + deck.DeckCards[i].Rank);
}
hand1.ReceiveCard(deck);
// Check all cards
for (int i = 0; i < deck.DeckCards.Count; i++)
{

    Console.WriteLine(i + ". " + deck.DeckCards[i].Suit + "" + deck.DeckCards[i].Rank);
}

System.Console.WriteLine("");
System.Console.WriteLine("Cards on hand:");
for (int i = 0; i < hand1.CardsOnHand.Count; i++)
{
    System.Console.WriteLine(hand1.CardsOnHand[i].Rank + " " + hand1.CardsOnHand[i].Suit);
}
deck.ReciveDiscardedCard(hand1);
System.Console.WriteLine("Discard");
if (hand1.CardsOnHand.Count > 0)
{
    for (int i = 0; i < hand1.CardsOnHand.Count; i++)
    {
        System.Console.WriteLine(hand1.CardsOnHand[i].Rank + " " + hand1.CardsOnHand[i].Suit);
    }

}
System.Console.WriteLine("Discarded cards in deck");
if (deck.DiscardedCards.Count > 0)
{
    for (int i = 0; i < deck.DiscardedCards.Count; i++)
    {
        System.Console.WriteLine(deck.DiscardedCards[i].Rank + " " + deck.DiscardedCards[i].Suit);
    }

}
System.Console.WriteLine("");
for (int i = 0; i < deck.DeckCards.Count; i++)
{

    Console.WriteLine(i + ". " + deck.DeckCards[i].Suit + "" + deck.DeckCards[i].Rank);


}
System.Console.WriteLine("Cards on hand:");
for (int i = 0; i < hand1.CardsOnHand.Count; i++)
{
    System.Console.WriteLine(hand1.CardsOnHand[i].Rank + " " + hand1.CardsOnHand[i].Suit);
}
hand1.ReceiveCard(deck);
hand1.ReceiveCard(deck);

System.Console.WriteLine("Cards on hand:");
for (int i = 0; i < hand1.CardsOnHand.Count; i++)
{
    System.Console.WriteLine(hand1.CardsOnHand[i].Rank + " " + hand1.CardsOnHand[i].Suit);
}
deck.ForceRetriveAllCards();
deck.ShuffleDeck();
System.Console.WriteLine("Shuffle deck \n");

System.Console.WriteLine("Discarded cards in deck");
if (deck.DiscardedCards.Count > 0)
{
    for (int i = 0; i < deck.DiscardedCards.Count; i++)
    {
        System.Console.WriteLine(deck.DiscardedCards[i].Rank + " " + deck.DiscardedCards[i].Suit);
    }

}

for (int i = 0; i < deck.DeckCards.Count; i++)
{
    Console.WriteLine(i + ". " + deck.DeckCards[i].Suit + "" + deck.DeckCards[i].Rank);
}




