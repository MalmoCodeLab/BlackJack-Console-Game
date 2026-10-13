
using System;

ConsoleUi ui = new ConsoleUi();

string choice = ui.ShowMenu();

Console.Clear();
Console.WriteLine($"Du valde: {choice}");

Console.ReadKey(true);


