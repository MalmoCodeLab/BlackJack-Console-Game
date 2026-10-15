internal static class MenuNavigation
{
    public static int MoveWrapped(int selectedOption, int optionCount, ConsoleKey key)
    {
        ValidateSelection(selectedOption, optionCount);

        return key switch
        {
            ConsoleKey.UpArrow => (selectedOption - 1 + optionCount) % optionCount,
            ConsoleKey.DownArrow => (selectedOption + 1) % optionCount,
            _ => selectedOption
        };
    }

    public static int MoveClamped(int selectedOption, int optionCount, ConsoleKey key)
    {
        ValidateSelection(selectedOption, optionCount);

        return key switch
        {
            ConsoleKey.LeftArrow => Math.Max(0, selectedOption - 1),
            ConsoleKey.RightArrow => Math.Min(optionCount - 1, selectedOption + 1),
            _ => selectedOption
        };
    }

    private static void ValidateSelection(int selectedOption, int optionCount)
    {
        if (optionCount < 1)
            throw new ArgumentOutOfRangeException(nameof(optionCount));
        if (selectedOption < 0 || selectedOption >= optionCount)
            throw new ArgumentOutOfRangeException(nameof(selectedOption));
    }
}
