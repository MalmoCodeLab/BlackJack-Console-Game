using System;

// Adds the overload expected by Game.cs without replacing other ConsoleUi methods.
public partial class ConsoleUi
{
    public void ShowRules(RuleSet rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        ShowRules();
    }
}
