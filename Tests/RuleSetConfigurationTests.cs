using System.Text.Json;

namespace Mai26ma_Grupp1_BlackJack.Tests;

// Checklist items: "Saknad konfigurationsfil" and "Felaktig inställning".
public sealed class RuleSetConfigurationTests : IDisposable
{
    private readonly string directory = Path.Combine(
        Path.GetTempPath(), "blackjack-rules-tests-" + Guid.NewGuid().ToString("N"));

    public RuleSetConfigurationTests() => Directory.CreateDirectory(directory);

    public void Dispose() => Directory.Delete(directory, recursive: true);

    private string WriteRulesFile(string content)
    {
        string path = Path.Combine(directory, RuleSet.FileName);
        File.WriteAllText(path, content);
        return path;
    }

    private static void AssertIsDefault(RuleSet rules)
    {
        Assert.False(rules.DealerHitsSoft17);
        Assert.Equal(6, rules.DeckCount);
        Assert.Equal(15, rules.ReshuffleThreshold);
        Assert.True(rules.NaturalBlackjackBeatsThreeCard21);
    }

    [Fact]
    public void MissingFile_UsesDefaultsAndNotifies()
    {
        string missing = Path.Combine(directory, RuleSet.FileName);
        List<string> notices = new();

        RuleSet rules = RuleSet.LoadFromFile(missing, notices.Add);

        AssertIsDefault(rules);
        string notice = Assert.Single(notices);
        Assert.Contains(RuleSet.FileName, notice);
    }

    [Fact]
    public void MissingFile_WithoutNotifyCallback_DoesNotThrow()
    {
        RuleSet rules = RuleSet.LoadFromFile(Path.Combine(directory, "missing.json"));

        AssertIsDefault(rules);
    }

    [Fact]
    public void ValidFile_IsLoadedWithoutNotice()
    {
        string path = WriteRulesFile("""
            { "DealerHitsSoft17": true, "DeckCount": 2,
              "ReshuffleThreshold": 30, "NaturalBlackjackBeatsThreeCard21": false }
            """);
        List<string> notices = new();

        RuleSet rules = RuleSet.LoadFromFile(path, notices.Add);

        Assert.True(rules.DealerHitsSoft17);
        Assert.Equal(2, rules.DeckCount);
        Assert.Equal(30, rules.ReshuffleThreshold);
        Assert.False(rules.NaturalBlackjackBeatsThreeCard21);
        Assert.Empty(notices);
    }

    [Fact]
    public void PropertyNames_AreCaseInsensitive()
    {
        string path = WriteRulesFile("""{ "deckcount": 3, "reshufflethreshold": 20 }""");

        RuleSet rules = RuleSet.LoadFromFile(path);

        Assert.Equal(3, rules.DeckCount);
        Assert.Equal(20, rules.ReshuffleThreshold);
    }

    [Fact]
    public void MissingProperties_FallBackToDefaultValues()
    {
        string path = WriteRulesFile("""{ "DealerHitsSoft17": true }""");

        RuleSet rules = RuleSet.LoadFromFile(path);

        Assert.True(rules.DealerHitsSoft17);
        Assert.Equal(6, rules.DeckCount);
        Assert.Equal(15, rules.ReshuffleThreshold);
        Assert.True(rules.NaturalBlackjackBeatsThreeCard21);
    }

    [Fact]
    public void ShippedRulesFile_IsValidAndEqualsDefaults()
    {
        string? folder = AppContext.BaseDirectory;
        string? shipped = null;
        while (folder is not null)
        {
            string candidate = Path.Combine(folder, RuleSet.FileName);
            if (File.Exists(candidate) && !candidate.Contains(Path.Combine("Tests", "bin")))
            {
                shipped = candidate;
                break;
            }
            folder = Path.GetDirectoryName(folder);
        }

        Assert.NotNull(shipped);
        Assert.Equal(RuleSet.Default, RuleSet.Load(shipped!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not json at all")]
    [InlineData("{ \"DeckCount\": 6,")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("{ \"DeckCount\": \"six\" }")]
    [InlineData("{ \"DeckCount\": 2.5 }")]
    [InlineData("{ \"DealerHitsSoft17\": \"maybe\" }")]
    [InlineData("{ \"DeckCount\": 0 }")]
    [InlineData("{ \"DeckCount\": 9 }")]
    [InlineData("{ \"DeckCount\": -1 }")]
    [InlineData("{ \"ReshuffleThreshold\": 0 }")]
    [InlineData("{ \"ReshuffleThreshold\": -5 }")]
    [InlineData("{ \"DeckCount\": 1, \"ReshuffleThreshold\": 53 }")]
    [InlineData("{ \"ReshuffleThreshold\": 313 }")]
    public void InvalidFile_UsesDefaultsAndNotifiesWithoutThrowing(string content)
    {
        string path = WriteRulesFile(content);
        List<string> notices = new();

        RuleSet rules = RuleSet.LoadFromFile(path, notices.Add);

        AssertIsDefault(rules);
        Assert.Single(notices);
        Assert.False(string.IsNullOrWhiteSpace(notices[0]));
    }

    [Fact]
    public void UnreadableFile_UsesDefaultsAndNotifiesWithoutThrowing()
    {
        string path = WriteRulesFile("{}");
        List<string> notices = new();

        // An open exclusive handle makes File.ReadAllText fail with an IOException.
        using FileStream locked = new(path, FileMode.Open, FileAccess.Read, FileShare.None);
        RuleSet rules = RuleSet.LoadFromFile(path, notices.Add);

        AssertIsDefault(rules);
        Assert.Single(notices);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 52)]
    [InlineData(8, 416)]
    [InlineData(6, 15)]
    public void Validate_AcceptsBoundaryValues(int deckCount, int threshold)
    {
        RuleSet rules = new() { DeckCount = deckCount, ReshuffleThreshold = threshold };

        rules.Validate();
    }

    [Theory]
    [InlineData(0, 15)]
    [InlineData(9, 15)]
    [InlineData(1, 0)]
    [InlineData(1, 53)]
    public void Validate_RejectsOutOfRangeValues(int deckCount, int threshold)
    {
        RuleSet rules = new() { DeckCount = deckCount, ReshuffleThreshold = threshold };

        Assert.Throws<ArgumentOutOfRangeException>(rules.Validate);
    }

    [Fact]
    public void Load_StillThrowsOnInvalidJson_SoCallersCanChooseToHandleIt()
    {
        string path = WriteRulesFile("not json");

        Assert.ThrowsAny<JsonException>(() => RuleSet.Load(path));
    }
}
