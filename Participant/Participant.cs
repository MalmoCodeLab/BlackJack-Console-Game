public abstract class Participant
{
    public string Name { get; }

    protected Participant (string name)
    {
        Name = name;
    }
}