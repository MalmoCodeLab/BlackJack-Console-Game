Player niclas = new Player("Niclas", 5, 3, 1);
Player anna = new Player("Anna", 2, 8, 9);          // ny spelare, börjar på 0-0-0

Console.WriteLine("W = Wins, L = Losses, P = Pushes");
Console.WriteLine($"{niclas.Name}: W: {niclas.Wins}, L: {niclas.Losses}, P: {niclas.Pushes}");
Console.WriteLine($"{anna.Name}: W: {anna.Wins}, L: {anna.Losses}, P: {anna.Pushes}");
