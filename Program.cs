Deck deck = new ();
deck.CreateDeck();
Hand hand1 = new();
deck.ShuffleDeck();
for (int i = 0 ; i < deck.DeckCards.Count; i++)
{
    Console.WriteLine(i + ". " + deck.DeckCards[i].Suit + "" + deck.DeckCards[i].Rank);
}
hand1.CardsOnHand.Add(deck.DeckCards[0]);
deck.DeckCards.RemoveAt(0);
// Check all cards
for (int i = 0 ; i < deck.DeckCards.Count; i++)
{

    Console.WriteLine(i + ". " + deck.DeckCards[i].Suit + "" + deck.DeckCards[i].Rank);
}


/*int value = deck.DeckCards[51].IndividualCardValue();
System.Console.WriteLine(value); 
System.Console.WriteLine(hand1.CardsOnHand);
System.Console.WriteLine(hand1.CardsOnHand[0].Rank + " " + hand1.CardsOnHand[0].Suit);*/
System.Console.WriteLine("");
System.Console.WriteLine(hand1.CardsOnHand[0].Rank + " " + hand1.CardsOnHand[0].Suit);


