Console.WriteLine("Hello, Earth!");
Deck deck = new ();
deck.CreateDeck();
int count = deck.DeckCards.Count;
System.Console.WriteLine(count);
// Check all cards
for (int i = 0 ; i < deck.DeckCards.Count; i++)
{   
    
    Console.WriteLine(i + ". " + deck.DeckCards[i].Suit + "" + deck.DeckCards[i].Rank);
}