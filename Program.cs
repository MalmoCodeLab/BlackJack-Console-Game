Player niclas = new Player("Niclas", 5, 3, 1);
Player anna = new Player("Anna", 2, 8, 9);  

Console.WriteLine("W = Wins, L = Losses, P = Pushes");
Console.WriteLine($"{niclas.Name}: W: {niclas.Wins}, L: {niclas.Losses}, P: {niclas.Pushes}");
Console.WriteLine($"{anna.Name}: W: {anna.Wins}, L: {anna.Losses}, P: {anna.Pushes}");

Dealer dealer = new Dealer();
dealer.ShowHoleCard();
Console.WriteLine($"Visar hålkort: {dealer.HoleCardVisible}"); 
dealer.NewRound();
Console.WriteLine($"Ny runda: {dealer.HoleCardVisible}"); 